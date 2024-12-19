using System.Security.Cryptography;

namespace Spartan.Utils.Cryptography;

public class DoubleRatchet
{
    private byte[] _currentKey;
    private byte[] _chainKey;
    public byte[] MessageKey { get; private set; }

    public DoubleRatchet(byte[] initialKey)
    {
        _currentKey = initialKey;
        _chainKey = new byte[32];
        MessageKey = new byte[32];
        
        UpdateKeys();
    }

    public void Advance()
    {
        var random = new Random();
        byte[] dhKey = new byte[32];
        random.NextBytes(dhKey);

        var combinedKey = new byte[64];
        Buffer.BlockCopy(dhKey, 0, combinedKey, 0, dhKey.Length);
        Buffer.BlockCopy(_chainKey, 0, combinedKey, dhKey.Length, _chainKey.Length);

        var hmac = new HMACSHA256(combinedKey);
        _chainKey = hmac.ComputeHash(_chainKey);
        MessageKey = hmac.ComputeHash(_chainKey);

        _currentKey = MessageKey;
    }

    private void UpdateKeys()
    {
        var hmac = new HMACSHA256(_currentKey);
        _chainKey = hmac.ComputeHash(_chainKey);
        MessageKey = hmac.ComputeHash(_chainKey);
    }
}