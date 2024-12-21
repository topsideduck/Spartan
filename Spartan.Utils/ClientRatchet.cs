using System.Security.Cryptography;
using System.Text;

namespace Spartan.Utils;

public class ClientRatchet
{
    private readonly ECDiffieHellman _ika;
    private readonly ECDiffieHellman _eka;
    private ECDiffieHellman? _dhRatchet = null;

    private SymmetricRatchet _rootRatchet;
    private SymmetricRatchet _sendRatchet;
    private SymmetricRatchet _receiveRatchet;

    public byte[] IKaPublicKey => _ika.PublicKey.ExportSubjectPublicKeyInfo();
    public byte[] EKaPublicKey => _eka.PublicKey.ExportSubjectPublicKeyInfo();
    public byte[] DhRatchetPublicKey => _dhRatchet?.PublicKey.ExportSubjectPublicKeyInfo() ?? new byte[0];

    public byte[] SharedKey { get; private set; }

    public ClientRatchet()
    {
        _ika = ECDiffieHellman.Create(ECCurve.NamedCurves.nistP256);
        _eka = ECDiffieHellman.Create(ECCurve.NamedCurves.nistP256);
    }

    public static byte[] Hkdf(byte[] input, int length)
    {
        // Use HKDF to derive a key
        return HKDF.DeriveKey(HashAlgorithmName.SHA256, input, length);
    }

    public void InitializeRatchet()
    {
        _rootRatchet = new SymmetricRatchet(SharedKey);
        _receiveRatchet = new SymmetricRatchet(_rootRatchet.Next().Item1);
        _sendRatchet = new SymmetricRatchet(_rootRatchet.Next().Item1);
    }

    public void X3dh(byte[] otherSPKbBytes, byte[] otherIKbBytes, byte[] otherOPKbBytes)
    {
        using var otherSPKb = ECDiffieHellman.Create();
        otherSPKb.ImportSubjectPublicKeyInfo(otherSPKbBytes, out _);

        using var otherIKb = ECDiffieHellman.Create();
        otherIKb.ImportSubjectPublicKeyInfo(otherIKbBytes, out _);

        using var otherOPKb = ECDiffieHellman.Create();
        otherOPKb.ImportSubjectPublicKeyInfo(otherOPKbBytes, out _);

        var dh1 = _ika.DeriveKeyMaterial(otherSPKb.PublicKey);
        var dh2 = _eka.DeriveKeyMaterial(otherIKb.PublicKey);
        var dh3 = _eka.DeriveKeyMaterial(otherSPKb.PublicKey);
        var dh4 = _eka.DeriveKeyMaterial(otherOPKb.PublicKey);

        var combined = new byte[dh1.Length + dh2.Length + dh3.Length + dh4.Length];
        Buffer.BlockCopy(dh1, 0, combined, 0, dh1.Length);
        Buffer.BlockCopy(dh2, 0, combined, dh1.Length, dh2.Length);
        Buffer.BlockCopy(dh3, 0, combined, dh1.Length + dh2.Length, dh3.Length);
        Buffer.BlockCopy(dh4, 0, combined, dh1.Length + dh2.Length + dh3.Length, dh4.Length);

        SharedKey = Hkdf(combined, 32);
    }

    public void DhRatchet(byte[] dhRatchetPublicKeyBytes)
    {
        using var dhRatchetPublicKey = ECDiffieHellman.Create();
        dhRatchetPublicKey.ImportSubjectPublicKeyInfo(dhRatchetPublicKeyBytes, out _);

        if (_dhRatchet != null)
        {
            var dhReceive = _dhRatchet.DeriveKeyMaterial(dhRatchetPublicKey.PublicKey);
            var sharedReceive = _rootRatchet.Next(dhReceive).Item1;

            _receiveRatchet = new SymmetricRatchet(sharedReceive);
        }

        _dhRatchet = ECDiffieHellman.Create(ECCurve.NamedCurves.nistP256);
        var dhSend = _dhRatchet.DeriveKeyMaterial(dhRatchetPublicKey.PublicKey);
        var sharedSend = _rootRatchet.Next(dhSend).Item1;

        _sendRatchet = new SymmetricRatchet(sharedSend);
    }
}