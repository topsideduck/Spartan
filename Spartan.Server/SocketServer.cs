using System.Net;
using System.Net.Sockets;
using System.Security.Cryptography;

namespace Spartan.Server;

public class SocketServer : IDisposable
{
    private readonly TcpListener _tcpListener;
    private readonly TcpClient _tcpClient;
    
    private readonly BinaryReader _binaryReader;
    private readonly BinaryWriter _binaryWriter;
    
    private Aes _aes;

    public IPAddress ServerIpAddress { get; }
    public int ServerPort { get; }

    public SocketServer(IPAddress serverIpAddress, int serverPort)
    {
        ServerIpAddress = serverIpAddress;
        ServerPort = serverPort;

        _tcpListener = new TcpListener(ServerIpAddress, ServerPort);
        _tcpListener.Start();

        _tcpClient = _tcpListener.AcceptTcpClient();
        
        var networkStream = _tcpClient.GetStream();
    }
}