using System.Security.Cryptography;
using Org.BouncyCastle.Asn1.Sec;
using Org.BouncyCastle.Crypto;
using Org.BouncyCastle.Crypto.Agreement;
using Org.BouncyCastle.Crypto.Generators;
using Org.BouncyCastle.Crypto.Parameters;
using Org.BouncyCastle.Security;

namespace Spartan.Utils.Cryptography;

public static class EcdhKeyExchange
{
    public static AsymmetricCipherKeyPair GenerateDiffieHellmanKeyPair()
    {
        var keyGen = new ECKeyPairGenerator();
        var secureRandom = new SecureRandom();
        var curve = SecNamedCurves.GetByName("secp256r1");
        var domainParams = new ECDomainParameters(curve.Curve, curve.G, curve.N, curve.H);
        var keyGenParam = new ECKeyGenerationParameters(domainParams, secureRandom);
        keyGen.Init(keyGenParam);
        return keyGen.GenerateKeyPair();
    }

    public static byte[] DeriveSharedKey(AsymmetricCipherKeyPair ownKeyPair, byte[] otherPublicKeyBytes)
    {
        var curve = SecNamedCurves.GetByName("secp256r1");
        var domainParams = new ECDomainParameters(curve.Curve, curve.G, curve.N, curve.H);
        var otherPublicKey = new ECPublicKeyParameters(curve.Curve.DecodePoint(otherPublicKeyBytes), domainParams);

        var agreement = new ECDHBasicAgreement();
        agreement.Init(ownKeyPair.Private);
        var sharedSecret = agreement.CalculateAgreement(otherPublicKey);

        return SHA256.HashData(sharedSecret.ToByteArrayUnsigned());
    }
}