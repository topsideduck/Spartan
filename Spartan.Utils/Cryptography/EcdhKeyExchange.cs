using System.Security.Cryptography;

namespace Spartan.Utils.Cryptography;

public static class EcdhKeyExchange
{
    public static (ECDiffieHellman ecdh, byte[] publicKey) GenerateDiffieHellmanKeyPair()
    {
        var ecdh = ECDiffieHellman.Create(ECCurve.NamedCurves.nistP256);
        return (ecdh, ecdh.PublicKey.ExportSubjectPublicKeyInfo());
    }

    public static byte[] DeriveSharedKey(ECDiffieHellman ownEcdh, byte[] otherPublicKeyBytes)
    {
        using var otherPublicKey = ECDiffieHellman.Create();
        otherPublicKey.ImportSubjectPublicKeyInfo(otherPublicKeyBytes, out _);
        return ownEcdh.DeriveKeyMaterial(otherPublicKey.PublicKey);
    }
}
