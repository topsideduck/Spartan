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

    private static byte[] Serialize<T>(T obj)
    {
        if (obj == null) throw new ArgumentNullException(nameof(obj));

        var type = obj.GetType();

        // Serialize the object
        var serializedData = MessagePackSerializer.Serialize(obj);

        // Create the wrapper
        var wrapper = new DataWrapper
        {
            TypeName = type.AssemblyQualifiedName!,
            Data = serializedData
        };

        // Serialize the wrapper
        return MessagePackSerializer.Serialize(wrapper);
    }

    private static dynamic Deserialize(byte[] serializedWrapper)
    {
        // Deserialize the wrapper
        var wrapper = MessagePackSerializer.Deserialize<DataWrapper>(serializedWrapper);

        // Resolve the type
        var type = Type.GetType(wrapper.TypeName);
        if (type == null)
        {
            throw new InvalidOperationException($"Unable to resolve type: {wrapper.TypeName}");
        }

        // Deserialize the data into the resolved type
        return MessagePackSerializer.Deserialize(type, wrapper.Data)!;
    }

    public void SendData(object rawData, bool encrypt = true)
    {
        var serializedData = Serialize(rawData);

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

        return Deserialize(dataStream.ToArray());
    }
}