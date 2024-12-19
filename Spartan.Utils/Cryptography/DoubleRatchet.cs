using System.Security.Cryptography;

namespace Spartan.Utils.Cryptography;

public class DoubleRatchet
{
    public byte[] CurrentKey { get; private set; }
    private byte[] chainKey;
    private byte[] messageKey;

    public DoubleRatchet(byte[] initialKey)
    {
        CurrentKey = initialKey;
        chainKey = new byte[32];
        messageKey = new byte[32];
        
        UpdateKeys();
    }

    public void Advance()
    {
        var random = new Random();
        byte[] dhKey = new byte[32];
        random.NextBytes(dhKey);

        var combinedKey = new byte[64];
        Buffer.BlockCopy(dhKey, 0, combinedKey, 0, dhKey.Length);
        Buffer.BlockCopy(chainKey, 0, combinedKey, dhKey.Length, chainKey.Length);

        var hmac = new HMACSHA256(combinedKey);
        chainKey = hmac.ComputeHash(chainKey);
        messageKey = hmac.ComputeHash(chainKey);

        CurrentKey = messageKey;
    }

    private void UpdateKeys()
    {
        var hmac = new HMACSHA256(CurrentKey);
        chainKey = hmac.ComputeHash(chainKey);
        messageKey = hmac.ComputeHash(chainKey);
    }
}