using Spartan.Models.ResponseModels;
using Spartan.Server.ResponseFormatters;
using Spartan.Server.ResponseFormatters.Echo;
using Spartan.Server.ResponseFormatters.Filesystem;

namespace Spartan.Server;

public class ResponseFormatter
{
    private readonly Dictionary<string, IResponseFormatter> _formatters = new()
    {
        { "echo", new EchoCommandResponseFormatter() },
        { "cd", new CdCommandResponseFormatter() },
        { "ls", new LsCommandResponseFormatter() },
        { "pwd", new PwdCommandResponseFormatter() },
        { "cat", new CatCommandResponseFormatter() }
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