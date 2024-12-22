using System.Net;
using System.Text;
using Spartan.Server;

namespace Spartan.CLI;

class Program
{
    static void Main(string[] args)
    {
        var socketServer = new SocketServer(IPAddress.Parse("127.0.0.1"), 12345,
            "/Users/hkeshavr/Developer/Spartan/Spartan.Payload/bin/Release/net9.0/publish");

        while (true)
        {
            Console.Write("> ");
            var input = Console.ReadLine();
            socketServer.SendData(Encoding.UTF8.GetBytes(input));
            var response = Encoding.UTF8.GetString(socketServer.ReceiveData()).Replace("\0", string.Empty);
            Console.WriteLine(response);
        }
    }
}