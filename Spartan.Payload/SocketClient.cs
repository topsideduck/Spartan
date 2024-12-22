using System.Security.Cryptography;
using Spartan.Utils;
using MessagePack;
using Spartan.Models;

namespace Spartan.Payload;

public class SocketClient : IDisposable
{
    private readonly BinaryReader _binaryReader;
    private readonly BinaryWriter _binaryWriter;
    private readonly ClientRatchet _clientRatchet;

    public SocketClient(BinaryReader binaryReader, BinaryWriter binaryWriter)
    {
        _binaryReader = binaryReader;
        _binaryWriter = binaryWriter;

        _clientRatchet = new ClientRatchet();

        // PerformX3dhHandshake();

        // _clientRatchet.InitializeRatchet();
    }

    public void Dispose()
    {
        _binaryReader.Dispose();
        _binaryWriter.Dispose();
        GC.SuppressFinalize(this);
    }

    private void SendClientPublicKeys()
    {
        // var clientPublicKeysDictionary = new Dictionary<string, byte[]>
        // {
        //     { "IKaPublicKey", _clientRatchet.IKaPublicKey },
        //     { "EKaPublicKey", _clientRatchet.EKaPublicKey }
        // };

        var clientPublicKeys = new ClientPublicKeys
        {
            IKaPublicKey = _clientRatchet.IKaPublicKey,
            EKaPublicKey = _clientRatchet.EKaPublicKey
        };

        // // Serialize the dictionary into json
        // var serializedClientPublicKeysDictionary = MessagePackSerializer.Serialize(clientPublicKeysDictionary);
        //
        // _binaryWriter.Write(serializedClientPublicKeysDictionary.Length);
        // _binaryWriter.Write(serializedClientPublicKeysDictionary);
        SendData(clientPublicKeys, encrypt: false);
    }

    private ServerPublicKeys ReceiveServerPublicKeys()
    {
        // var serializedServerPublicKeysDictionaryBytesLength = _binaryReader.ReadInt32();
        // var serializedServerPublicKeysDictionaryBytes =
        //     _binaryReader.ReadBytes(serializedServerPublicKeysDictionaryBytesLength);
        //
        // var serverPublicKeysDictionary =
        //     MessagePackSerializer.Deserialize<Dictionary<string, byte[]>>(serializedServerPublicKeysDictionaryBytes);

        var serverPublicKeys = ReceiveData<ServerPublicKeys>(encrypt: false);

        return serverPublicKeys;
    }

    public void PerformX3dhHandshake()
    {
        SendClientPublicKeys();

        var serverPublicKeysDictionary = ReceiveServerPublicKeys();
        _clientRatchet.X3dh(serverPublicKeysDictionary.SPKbPublicKey, serverPublicKeysDictionary.IKbPublicKey,
            serverPublicKeysDictionary.OPKbPublicKey);
    }

    public void InitializeRatchet()
    {
        _clientRatchet.InitializeRatchet();
    }

    public void SendData(object rawData, bool encrypt = true)
    {
        byte[] serializedData = MessagePackSerializer.Serialize(rawData);

        using var dataStream = new MemoryStream(serializedData);
        var buffer = new byte[8192];

        if (!encrypt)
        {
            while (dataStream.Read(buffer, 0, buffer.Length) > 0)
            {
                _binaryWriter.Write(buffer.Length);
                _binaryWriter.Write(buffer);
            }
        }
        else
        {
            var (key, iv) = _clientRatchet.SendRotate();

            using var aes = Aes.Create();
            aes.Key = key;
            aes.IV = iv;

            _binaryWriter.Write(_clientRatchet.DhRatchetPublicKey.Length);
            _binaryWriter.Write(_clientRatchet.DhRatchetPublicKey);

            using var encryptor = aes.CreateEncryptor();

            while (dataStream.Read(buffer, 0, buffer.Length) > 0)
            {
                var encryptedChunk = encryptor.TransformFinalBlock(buffer, 0, buffer.Length);

                _binaryWriter.Write(encryptedChunk.Length);
                _binaryWriter.Write(encryptedChunk);
            }
        }

        _binaryWriter.Write(0); // Signal end of file
    }

    public T ReceiveData<T>(bool encrypt = true)
    {
        using var dataStream = new MemoryStream();

        if (!encrypt)
        {
            while (true)
            {
                var chunkSize = _binaryReader.ReadInt32();
                if (chunkSize == 0) break; // End of file

                var chunk = _binaryReader.ReadBytes(chunkSize);
                dataStream.Write(chunk, 0, chunk.Length);
            }
        }
        else
        {
            var dhRatchetPublicKeyLength = _binaryReader.ReadInt32();
            var dhRatchetPublicKeyBytes = _binaryReader.ReadBytes(dhRatchetPublicKeyLength);

            var (key, iv) = _clientRatchet.ReceiveRotate(dhRatchetPublicKeyBytes);

            using var aes = Aes.Create();
            aes.Key = key;
            aes.IV = iv;

            using var decryptor = aes.CreateDecryptor();

            while (true)
            {
                var chunkSize = _binaryReader.ReadInt32();
                if (chunkSize == 0) break; // End of file

                var encryptedChunk = _binaryReader.ReadBytes(chunkSize);
                var decryptedChunk = decryptor.TransformFinalBlock(encryptedChunk, 0, encryptedChunk.Length);

                dataStream.Write(decryptedChunk, 0, decryptedChunk.Length);
            }
        }

        return MessagePackSerializer.Deserialize<T>(dataStream.ToArray());
    }
}