using System.Security.Cryptography;

namespace Spartan.Utils;

public class ServerRatchet
{
    private readonly ECDiffieHellman _ikb;
    private readonly ECDiffieHellman _spkb;
    private readonly ECDiffieHellman _opkb;
    private ECDiffieHellman _dhRatchet;

    private SymmetricRatchet _rootRatchet;
    private SymmetricRatchet _sendRatchet;
    private SymmetricRatchet _receiveRatchet;

    private byte[] _sharedKey;

    public byte[] IKbPublicKey => _ikb.PublicKey.ExportSubjectPublicKeyInfo();
    public byte[] SPKbPublicKey => _spkb.PublicKey.ExportSubjectPublicKeyInfo();
    public byte[] OPKbPublicKey => _opkb.PublicKey.ExportSubjectPublicKeyInfo();
    public byte[] DhRatchetPublicKey => _dhRatchet.PublicKey.ExportSubjectPublicKeyInfo();

    public ServerRatchet()
    {
        _ikb = ECDiffieHellman.Create(ECCurve.NamedCurves.nistP256);
        _spkb = ECDiffieHellman.Create(ECCurve.NamedCurves.nistP256);
        _opkb = ECDiffieHellman.Create(ECCurve.NamedCurves.nistP256);
        _dhRatchet = ECDiffieHellman.Create(ECCurve.NamedCurves.nistP256);
    }

    public static byte[] Hkdf(byte[] input, int length)
    {
        // Use HKDF to derive a key
        return HKDF.DeriveKey(HashAlgorithmName.SHA256, input, length);
    }

    public void InitializeRatchet()
    {
        _rootRatchet = new SymmetricRatchet(_sharedKey);
        _sendRatchet = new SymmetricRatchet(_rootRatchet.Next().Item1);
        _receiveRatchet = new SymmetricRatchet(_rootRatchet.Next().Item1);
    }

    public void X3dh(byte[] otherIKaBytes, byte[] otherEKaBytes)
    {
        using var otherIKa = ECDiffieHellman.Create();
        otherIKa.ImportSubjectPublicKeyInfo(otherIKaBytes, out _);

        using var otherEKa = ECDiffieHellman.Create();
        otherEKa.ImportSubjectPublicKeyInfo(otherEKaBytes, out _);

        var dh1 = _spkb.DeriveKeyMaterial(otherIKa.PublicKey);
        var dh2 = _ikb.DeriveKeyMaterial(otherEKa.PublicKey);
        var dh3 = _spkb.DeriveKeyMaterial(otherEKa.PublicKey);
        var dh4 = _opkb.DeriveKeyMaterial(otherEKa.PublicKey);

        var combined = new byte[dh1.Length + dh2.Length + dh3.Length + dh4.Length];
        Buffer.BlockCopy(dh1, 0, combined, 0, dh1.Length);
        Buffer.BlockCopy(dh2, 0, combined, dh1.Length, dh2.Length);
        Buffer.BlockCopy(dh3, 0, combined, dh1.Length + dh2.Length, dh3.Length);
        Buffer.BlockCopy(dh4, 0, combined, dh1.Length + dh2.Length + dh3.Length, dh4.Length);

        _sharedKey = Hkdf(combined, 32);
    }

    private void DhRatchet(byte[] dhRatchetPublicKeyBytes)
    {
        using var dhRatchetPublicKey = ECDiffieHellman.Create();
        dhRatchetPublicKey.ImportSubjectPublicKeyInfo(dhRatchetPublicKeyBytes, out _);

        var dhReceive = _dhRatchet.DeriveKeyMaterial(dhRatchetPublicKey.PublicKey);
        var sharedReceive = _rootRatchet.Next(dhReceive).Item1;

        _receiveRatchet = new SymmetricRatchet(sharedReceive);

        _dhRatchet = ECDiffieHellman.Create(ECCurve.NamedCurves.nistP256);

        var dhSend = _dhRatchet.DeriveKeyMaterial(dhRatchetPublicKey.PublicKey);
        var sharedSend = _rootRatchet.Next(dhSend).Item1;

        _sendRatchet = new SymmetricRatchet(sharedSend);
    }

    public byte[] Encrypt(byte[] rawData)
    {
        var (key, iv) = _sendRatchet.Next();

        using var aes = Aes.Create();
        aes.Key = key;
        aes.IV = iv;

        var encryptor = aes.CreateEncryptor(aes.Key, aes.IV);
        var encrypted = encryptor.TransformFinalBlock(rawData, 0, rawData.Length);

        return encrypted;
    }

    public byte[] Decrypt(byte[] encryptedData, byte[] dhRatchetPublicKeyBytes)
    {
        DhRatchet(dhRatchetPublicKeyBytes);
        var (key, iv) = _receiveRatchet.Next();

        using var aes = Aes.Create();
        aes.Key = key;
        aes.IV = iv;

        using var decryptor = aes.CreateDecryptor();
        var decrypted = decryptor.TransformFinalBlock(encryptedData, 0, encryptedData.Length);
        return decrypted;
    }
}