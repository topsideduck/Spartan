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
        // Echo commands
        { "echo", new EchoCommand() },

        // Filesystem commands
        { "cat", new CatCommand() },
        { "cd", new CdCommand() },
        { "checksum", new ChecksumCommand() },
        { "cp", new CpCommand() },
        { "cpdir", new CpdirCommand() },
        { "file", new FileCommand() },
        { "find", new FindCommand() },
        { "ls", new LsCommand() },
        { "mkdir", new MkdirCommand() },
        { "move", new MoveCommand() },
        { "pwd", new PwdCommand() },
        { "rm", new RmCommand() },
        { "rmdir", new RmdirCommand() },
        { "touch", new TouchCommand() }
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