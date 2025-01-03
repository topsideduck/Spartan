using System.Net;
using System.Net.Sockets;
using System.Security.Cryptography;
using MessagePack;
using Spartan.Encryption;
using Spartan.Models;
using Spartan.Models.Payload;

namespace Spartan.Server;

public class SocketServer : IDisposable
{
    private readonly BinaryReader _binaryReader;
    private readonly BinaryWriter _binaryWriter;
    private readonly ServerRatchet _serverRatchet;

    public SocketServer(IPAddress serverIpAddress, int serverPort)
    {
        ServerIpAddress = serverIpAddress;
        ServerPort = serverPort;

        var tcpListener = new TcpListener(ServerIpAddress, ServerPort);
        tcpListener.Start();

        var tcpClient = tcpListener.AcceptTcpClient();

        ClientIpAddress = ((IPEndPoint)tcpClient.Client.RemoteEndPoint!).Address;
        ClientPort = ((IPEndPoint)tcpClient.Client.RemoteEndPoint).Port;

        _binaryReader = new BinaryReader(tcpClient.GetStream());
        _binaryWriter = new BinaryWriter(tcpClient.GetStream());

        _serverRatchet = new ServerRatchet();
    }

    public IPAddress ServerIpAddress { get; }
    public IPAddress ClientIpAddress { get; private set; }
    public int ServerPort { get; }
    public int ClientPort { get; private set; }


    public void Dispose()
    {
        _binaryReader.Dispose();
        _binaryWriter.Dispose();
        GC.SuppressFinalize(this);
    }

    private void SendServerPublicKeys()
    {
        var serverPublicKeys = new ServerPublicKeysModel
        {
            IKbPublicKey = _serverRatchet.IKbPublicKey,
            SPKbPublicKey = _serverRatchet.SPKbPublicKey,
            OPKbPublicKey = _serverRatchet.OPKbPublicKey
        };

        SendData(serverPublicKeys, false);
    }

    private ClientPublicKeysModel ReceiveClientPublicKeys()
    {
        ClientPublicKeysModel clientPublicKeys = ReceiveData(false);
        return clientPublicKeys;
    }

    public void PerformX3dhHandshake()
    {
        var clientPublicKeys = ReceiveClientPublicKeys();
        _serverRatchet.X3dh(clientPublicKeys.IKaPublicKey, clientPublicKeys.EKaPublicKey);

        SendServerPublicKeys();
    }

    public void InitializeRatchet()
    {
        _serverRatchet.InitializeRatchet();
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
            var (key, iv) = _serverRatchet.SendRotate();

            using var aes = Aes.Create();
            aes.Key = key;
            aes.IV = iv;

            _binaryWriter.Write(_serverRatchet.DhRatchetPublicKey.Length);
            _binaryWriter.Write(_serverRatchet.DhRatchetPublicKey);

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

            var (key, iv) = _serverRatchet.ReceiveRotate(dhRatchetPublicKeyBytes);

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