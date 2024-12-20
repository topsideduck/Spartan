using System.Net;
using System.Net.Sockets;
using Spartan.Utils.Cryptography;

namespace Spartan.Payload;

public class SocketClient : IDisposable
{
    private readonly BinaryReader _binaryReader;
    private readonly BinaryWriter _binaryWriter;

    private readonly DoubleRatchet _doubleRatchet;
    private readonly AesHandler _aesHandler;

    private IPAddress _serverIpAddress;
    private int _serverPort;
    
    public SocketClient(string serverIpAddress, int serverPort)
    {
        _serverIpAddress = IPAddress.Parse(serverIpAddress);
        _serverPort = serverPort;

        var tcpClient = new TcpClient();
        tcpClient.Connect(_serverIpAddress.ToString(), _serverPort);

        _binaryReader = new BinaryReader(tcpClient.GetStream());
        _binaryWriter = new BinaryWriter(tcpClient.GetStream());

        var sharedKey = PerformHandshake();
        
        _doubleRatchet = new DoubleRatchet(sharedKey);
        _aesHandler = new AesHandler();
    }
    
    public void Dispose()
    {
        _binaryReader.Dispose();
        _binaryWriter.Dispose();
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
}