using System.Net;
using System.Net.Sockets;
using System.Security.Cryptography;
using MessagePack;
using Spartan.Encryption;
using Spartan.Models;

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

        _binaryReader = new BinaryReader(tcpClient.GetStream());
        _binaryWriter = new BinaryWriter(tcpClient.GetStream());

        _serverRatchet = new ServerRatchet();

        // SendPayload(payloadPath);
        //
        // PerformX3dhHandshake();
        // _serverRatchet.InitializeRatchet();
    }

    public IPAddress ServerIpAddress { get; }
    public int ServerPort { get; }

    public void Dispose()
    {
        _binaryReader.Dispose();
        _binaryWriter.Dispose();
        GC.SuppressFinalize(this);
    }

    // private void SendPayload(string payloadDirectory)
    // {
    //     var files = Directory.GetFiles(payloadDirectory, "*.dll");
    //
    //     Dictionary<string, string> payload = new();
    //
    //     // Write each file as a name-content pair
    //     foreach (var filePath in files)
    //     {
    //         var fileContent = File.ReadAllBytes(filePath);
    //         payload[Path.GetFileNameWithoutExtension(filePath)] = Convert.ToBase64String(fileContent);
    //     }
    //
    //     // var serializedPayload = MessagePackSerializer.Serialize(payload);
    //     //
    //     // _binaryWriter.Write(serializedPayload.Length);
    //     // _binaryWriter.Write(serializedPayload);
    //
    //     SendData(payload, encrypt: false);
    // }

    private void SendServerPublicKeys()
    {
        // var serverPublicKeysDictionary = new Dictionary<string, byte[]>
        // {
        //     { "IKbPublicKey", _serverRatchet.IKbPublicKey },
        //     { "SPKbPublicKey", _serverRatchet.SPKbPublicKey },
        //     { "OPKbPublicKey", _serverRatchet.OPKbPublicKey }
        // };

        var serverPublicKeys = new ServerPublicKeys
        {
            IKbPublicKey = _serverRatchet.IKbPublicKey,
            SPKbPublicKey = _serverRatchet.SPKbPublicKey,
            OPKbPublicKey = _serverRatchet.OPKbPublicKey
        };

        // // Serialize the dictionary
        // var serializedServerPublicKeysDictionary = MessagePackSerializer.Serialize(serverPublicKeysDictionary);
        //
        // _binaryWriter.Write(serializedServerPublicKeysDictionary.Length);
        // _binaryWriter.Write(serializedServerPublicKeysDictionary);
        SendData(serverPublicKeys, encrypt: false);
    }

    private ClientPublicKeys ReceiveClientPublicKeys()
    {
        // var serializedClientPublicKeysDictionaryBytesLength = _binaryReader.ReadInt32();
        // var serializedClientPublicKeysDictionaryBytes =
        //     _binaryReader.ReadBytes(serializedClientPublicKeysDictionaryBytesLength);
        //
        // var serverClientKeysDictionary =
        //     MessagePackSerializer.Deserialize<Dictionary<string, byte[]>>(serializedClientPublicKeysDictionaryBytes);
        //
        // return serverClientKeysDictionary;

        var clientPublicKeys = ReceiveData<ClientPublicKeys>(encrypt: false);
        return clientPublicKeys;
    }

    public void PerformX3dhHandshake()
    {
        var clientPublicKeysDictionary = ReceiveClientPublicKeys();
        _serverRatchet.X3dh(clientPublicKeysDictionary.IKaPublicKey, clientPublicKeysDictionary.EKaPublicKey);

        SendServerPublicKeys();
    }

    public void InitializeRatchet()
    {
        _serverRatchet.InitializeRatchet();
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

        return MessagePackSerializer.Deserialize<T>(dataStream.ToArray());
    }
}