using Spartan.Models.ResponseModels;
using Spartan.Server.ResponseFormatters;
using Spartan.Server.ResponseFormatters.Echo;
using Spartan.Server.ResponseFormatters.Filesystem;
using Spartan.Server.ResponseFormatters.UI;

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
        { "checksum", new ChecksumCommandResponseFormatter() },
        { "cp", new CpCommandResponseFormatter() },
        { "cpdir", new CpdirCommandResponseFormatter() },
        { "download", new DownloadCommandResponseFormatter() },
        { "file", new FileCommandResponseFormatter() },
        { "find", new FindCommandResponseFormatter() },
        { "ls", new LsCommandResponseFormatter() },
        { "mkdir", new MkdirCommandResponseFormatter() },
        { "move", new MoveCommandResponseFormatter() },
        { "pwd", new PwdCommandResponseFormatter() },
        { "rm", new RmCommandResponseFormatter() },
        { "rmdir", new RmdirCommandResponseFormatter() },
        { "touch", new TouchCommandResponseFormatter() },
        { "upload", new UploadCommandResponseFormatter() },

        // UI commands
        { "screenshot", new ScreenshotCommandResponseFormatter() }
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