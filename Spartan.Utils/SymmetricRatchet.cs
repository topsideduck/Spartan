using System.Diagnostics;
using System.Security.Cryptography;

namespace Spartan.Utils;

public class SymmetricRatchet(byte[] key)
{
    private byte[] _state = key;

    private static byte[] Hkdf(byte[] input, int length)
    {
        // Use HKDF to derive a key
        return HKDF.DeriveKey(HashAlgorithmName.SHA256, input, length);
    }

    public (byte[], byte[]) Next(byte[]? input = null)
    {
        var combined = new byte[_state.Length + (input?.Length ?? 0)];
        
        Buffer.BlockCopy(_state, 0, combined, 0, _state.Length);
        if (input != null)
        {
            Buffer.BlockCopy(input, 0, combined, _state.Length, input.Length);
        }

        var output = Hkdf(combined, 80);
        _state = output[..32];
        return (output[32..64], output[64..]);
    }
}