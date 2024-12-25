using Spartan.Commands;
using Spartan.Commands.Echo;
using Spartan.Commands.Filesystem;
using Spartan.Models.RequestModels;
using Spartan.Models.ResponseModels;

namespace Spartan.Payload;

public class CommandExecutor
{
    private readonly Dictionary<string, ICommand> _commands = new()
    {
        { "echo", new EchoCommand() },
        { "cd", new CdCommand() },
        { "ls", new LsCommand() },
        { "pwd", new PwdCommand() },
        { "cat", new CatCommand() },
        { "touch", new TouchCommand() }
        // { "clear", new ClearCommand() },
        // { "exit", new ExitCommand() }
    };

    private ICommand GetCommand(string command)
    {
        return _commands[command];
    }

    public ICommandResponseModel ExecuteCommand(ICommandRequestModel commandModel)
    {
        var command = commandModel.Command;
        var commandInstance = GetCommand(command);

        return commandInstance.Execute(commandModel);
    }
}