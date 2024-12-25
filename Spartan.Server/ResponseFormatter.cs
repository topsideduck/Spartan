using Spartan.Models.ResponseModels;
using Spartan.Server.ResponseFormatters;
using Spartan.Server.ResponseFormatters.Echo;

namespace Spartan.Server;

public class ResponseFormatter
{
    private readonly Dictionary<string, IResponseFormatter> _formatters = new()
    {
        { "echo", new EchoCommandResponseFormatter() }
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