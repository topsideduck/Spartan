using System.Security.Cryptography;
using MessagePack;
using Spartan.Encryption;
using Spartan.Models;
using Spartan.Models.Payload;

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
    }

    public void Dispose()
    {
        _binaryReader.Dispose();
        _binaryWriter.Dispose();
        GC.SuppressFinalize(this);
    }

    private void SendClientPublicKeys()
    {
        var clientPublicKeys = new ClientPublicKeysModel
        {
            IKaPublicKey = _clientRatchet.IKaPublicKey,
            EKaPublicKey = _clientRatchet.EKaPublicKey
        };

        SendData(clientPublicKeys, false);
    }

    private ServerPublicKeysModel ReceiveServerPublicKeys()
    {
        ServerPublicKeysModel serverPublicKeys = ReceiveData(false);

        return serverPublicKeys;
    }

    public void PerformX3dhHandshake()
    {
        SendClientPublicKeys();

        var serverPublicKeys = ReceiveServerPublicKeys();
        _clientRatchet.X3dh(serverPublicKeys.SPKbPublicKey, serverPublicKeys.IKbPublicKey,
            serverPublicKeys.OPKbPublicKey);
    }

    public void InitializeRatchet()
    {
        _clientRatchet.InitializeRatchet();
    }

    public void SendData(object rawData, bool encrypt = true)
    {
        var serializedData = MessagePackSerializer.Serialize(rawData);

        var messageWrapper = new MessageWrapperModel
        {
            TypeName = rawData.GetType().AssemblyQualifiedName,
            Data = serializedData
        };

        var serializedMessage = MessagePackSerializer.Serialize(messageWrapper);

        using var dataStream = new MemoryStream(serializedMessage);
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

    public dynamic ReceiveData(bool encrypt = true)
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

        var deserializedData = MessagePackSerializer.Deserialize<MessageWrapperModel>(dataStream.ToArray());
        var typeName = Type.GetType(deserializedData.TypeName);
        var data = deserializedData.Data;

        return Convert.ChangeType(MessagePackSerializer.Deserialize(typeName, data), typeName);

        // return MessagePackSerializer.Deserialize<T>(dataStream.ToArray());
    }
}