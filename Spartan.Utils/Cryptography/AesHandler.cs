using System.Security.Cryptography;

namespace Spartan.Utils.Cryptography;

public class AesHandler
{
    private readonly Aes _aes;

    public byte[] AesKey
    {
        get => _aes.Key;
        set => _aes.Key = value;
    }
    
    public byte[] AesIv
    {
        get => _aes.IV;
        set => _aes.IV = value;
    }

    public AesHandler()
    {
        _aes = Aes.Create();
        _aes.Mode = CipherMode.CBC;
        _aes.Padding = PaddingMode.PKCS7;
    }

    public void GenerateNewIv()
    {
        _aes.GenerateIV();
    }

    public byte[] Encrypt(byte[] data)
    {
        using var encryptor = _aes.CreateEncryptor();
        return encryptor.TransformFinalBlock(data, 0, data.Length);
    }

    public byte[] Decrypt(byte[] data)
    {
        using var decryptor = _aes.CreateDecryptor();
        return decryptor.TransformFinalBlock(data, 0, data.Length);
    }
}