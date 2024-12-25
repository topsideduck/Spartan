using Spartan.Models.ResponseModels;
using Spartan.Server.ResponseFormatters;
using Spartan.Server.ResponseFormatters.Echo;
using Spartan.Server.ResponseFormatters.Filesystem;

namespace Spartan.Server;

public class ResponseFormatter
{
    private readonly Dictionary<string, IResponseFormatter> _formatters = new()
    {
        // Echo commands
        { "echo", new EchoCommandResponseFormatter() },

        // Filesystem commands
        { "cat", new CatCommandResponseFormatter() },
        { "cd", new CdCommandResponseFormatter() },
        { "cp", new CpCommandResponseFormatter() },
        { "cpdir", new CpdirCommandResponseFormatter() },
        { "find", new FindCommandResponseFormatter() },
        { "ls", new LsCommandResponseFormatter() },
        { "mkdir", new MkdirCommandResponseFormatter() },
        { "move", new MoveCommandResponseFormatter() },
        { "pwd", new PwdCommandResponseFormatter() },
        { "rm", new RmCommandResponseFormatter() },
        { "rmdir", new RmdirCommandResponseFormatter() },
        { "touch", new TouchCommandResponseFormatter() }
    };

    private IResponseFormatter GetFormatter(string command)
    {
        return _formatters[command];
    }

    public string Format(ICommandResponseModel response)
    {
        var command = response.Command;
        var formatter = GetFormatter(command);

        return formatter.FormatResponse(response);
    }
}