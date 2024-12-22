using System.Net;
using Spartan.Models;
using Spartan.Server;
using System.CommandLine;


namespace Spartan.CLI;

class Program
{
    // static async Task<int> Main(string[] args)
    // {
    //     var fileOption = new Option<FileInfo?>(
    //         name: "--file",
    //         description: "The file to read and display on the console.");
    //
    //     var rootCommand = new RootCommand("Sample app for System.CommandLine");
    //     rootCommand.AddOption(fileOption);
    //
    //     rootCommand.SetHandler((file) => 
    //         { 
    //             ReadFile(file!); 
    //         },
    //         fileOption);
    //
    //     return await rootCommand.InvokeAsync(args);
    // }
    //
    // static void ReadFile(FileInfo file)
    // {
    //     File.ReadLines(file.FullName).ToList()
    //         .ForEach(line => Console.WriteLine(line));
    // }

    static void Main()
    {
        var shell = new Shell();
        shell.ExecuteShell(IPAddress.Parse("127.0.0.1"), 12345, "/Users/hkeshavr/Developer/Spartan/Spartan.Payload/bin/Release/net9.0/publish");
    }
}