using System.Net;
using System.Net.Sockets;
using Spartan.Utils.Cryptography;

namespace Spartan.Server;

public class SocketServer : IDisposable
{
    private readonly BinaryReader _binaryReader;
    private readonly BinaryWriter _binaryWriter;
    
    private readonly DoubleRatchet _doubleRatchet;
    private readonly AesHandler _aesHandler; 


    public IPAddress ServerIpAddress { get; }
    public int ServerPort { get; }

    public SocketServer(IPAddress serverIpAddress, int serverPort)
    {
        ServerIpAddress = serverIpAddress;
        ServerPort = serverPort;

        var tcpListener = new TcpListener(ServerIpAddress, ServerPort);
        tcpListener.Start();

        var tcpClient = tcpListener.AcceptTcpClient();
        
        _binaryReader = new BinaryReader(tcpClient.GetStream());
        _binaryWriter = new BinaryWriter(tcpClient.GetStream());

        var serverKeyPair = EcdhKeyExchange.GenerateDiffieHellmanKeyPair();
        var clientPublicKey = ReceiveClientPublicKey();
        
        var sharedKey = EcdhKeyExchange.DeriveSharedKey(serverKeyPair, clientPublicKey);

        _doubleRatchet = new DoubleRatchet(sharedKey);
        _aesHandler = new AesHandler();
    }

    private byte[] ReceiveClientPublicKey()
    {
        var clientPublicKeyLength = _binaryReader.ReadInt32();
        var clientPublicKey = _binaryReader.ReadBytes(clientPublicKeyLength);
        
        return clientPublicKey;
    }
}