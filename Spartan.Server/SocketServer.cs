using System.Net;
using System.Net.Sockets;
using System.Security.Cryptography;
using System.Text;
using System.Text.Json;
using Spartan.Utils;

namespace Spartan.Server;

public class SocketServer : IDisposable
{
    private readonly ServerRatchet _serverRatchet;
    private readonly BinaryReader _binaryReader;
    private readonly BinaryWriter _binaryWriter;

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

        PerformX3dhHandshake();
        
        _serverRatchet.InitializeRatchet();
    }

    public IPAddress ServerIpAddress { get; }
    public int ServerPort { get; }

    public void Dispose()
    {
        _binaryReader.Dispose();
        _binaryWriter.Dispose();
        GC.SuppressFinalize(this);
    }

    private void SendServerPublicKeys()
    {
        var serverPublicKeysDictionary = new Dictionary<string, byte[]>
        {
            { "IKbPublicKey", _serverRatchet.IKbPublicKey },
            { "SPKbPublicKey", _serverRatchet.SPKbPublicKey },
            { "OPKbPublicKey", _serverRatchet.OPKbPublicKey }
        };

        // Serialize the dictionary into json
        var serializedServerPublicKeysDictionary = JsonSerializer.Serialize(serverPublicKeysDictionary);

        // convert to byte array
        var serializedServerPublicKeysDictionaryBytes = Encoding.UTF8.GetBytes(serializedServerPublicKeysDictionary);

        // send the length of the json
        _binaryWriter.Write(serializedServerPublicKeysDictionaryBytes.Length);

        // send the json
        _binaryWriter.Write(serializedServerPublicKeysDictionaryBytes);
    }

    private Dictionary<string, byte[]>? ReceiveClientPublicKeys()
    {
        var serializedClientPublicKeysDictionaryBytesLength = _binaryReader.ReadInt32();
        var serializedClientPublicKeysDictionaryBytes =
            _binaryReader.ReadBytes(serializedClientPublicKeysDictionaryBytesLength);

        // convert to json
        var serializedClientPublicKeysDictionary = Encoding.UTF8.GetString(serializedClientPublicKeysDictionaryBytes);

        // deserialize the json
        var serverClientKeysDictionary =
            JsonSerializer.Deserialize<Dictionary<string, byte[]>>(serializedClientPublicKeysDictionary);

        return serverClientKeysDictionary;
    }

    private void PerformX3dhHandshake()
    {
        var clientPublicKeysDictionary = ReceiveClientPublicKeys();
        _serverRatchet.X3dh(clientPublicKeysDictionary["IKaPublicKey"], clientPublicKeysDictionary["EKaPublicKey"]);
        
        SendServerPublicKeys();
    }

    public void SendData(byte[] rawData)
    {
        var (key, iv) = _serverRatchet.SendRotate();

        using var aes = Aes.Create();
        aes.Key = key;
        aes.IV = iv;

        _binaryWriter.Write(_serverRatchet.DhRatchetPublicKey.Length);
        _binaryWriter.Write(_serverRatchet.DhRatchetPublicKey);

        using var encryptor = aes.CreateEncryptor();
        using var dataStream = new MemoryStream(rawData);

        var buffer = new byte[8192];

        while (dataStream.Read(buffer, 0, buffer.Length) > 0)
        {
            var encryptedChunk = encryptor.TransformFinalBlock(buffer, 0, buffer.Length);

            _binaryWriter.Write(encryptedChunk.Length);
            _binaryWriter.Write(encryptedChunk);
        }

        _binaryWriter.Write(0); // Signal end of file
    }

    public byte[] ReceiveData()
    {
        var dhRatchetPublicKeyLength = _binaryReader.ReadInt32();
        var dhRatchetPublicKeyBytes = _binaryReader.ReadBytes(dhRatchetPublicKeyLength);

        var (key, iv) = _serverRatchet.ReceiveRotate(dhRatchetPublicKeyBytes);

        using var aes = Aes.Create();
        aes.Key = key;
        aes.IV = iv;

        using var decryptor = aes.CreateDecryptor();
        using var dataStream = new MemoryStream();

        while (true)
        {
            var chunkSize = _binaryReader.ReadInt32();
            if (chunkSize == 0) break; // End of file

            var encryptedChunk = _binaryReader.ReadBytes(chunkSize);
            var decryptedChunk = decryptor.TransformFinalBlock(encryptedChunk, 0, encryptedChunk.Length);

            dataStream.Write(decryptedChunk, 0, decryptedChunk.Length);
        }

        return dataStream.ToArray();
    }
}