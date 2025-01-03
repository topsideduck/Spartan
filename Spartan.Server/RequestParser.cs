using Spartan.Models.RequestModels;
using Spartan.Server.RequestParsers;
using Spartan.Server.RequestParsers.Echo;
using Spartan.Server.RequestParsers.Filesystem;
using Spartan.Server.RequestParsers.UI;

namespace Spartan.Server;

public class RequestParser
{
    private readonly Dictionary<string, IRequestParser> _parsers = new()
    {
        // Echo commands
        { "echo", new EchoCommandRequestParser() },

        // Filesystem commands
        { "cat", new CatCommandRequestParser() },
        { "cd", new CdCommandRequestParser() },
        { "checksum", new ChecksumCommandRequestParser() },
        { "cp", new CpCommandRequestParser() },
        { "cpdir", new CpdirCommandRequestParser() },
        { "download", new DownloadCommandRequestParser() },
        { "file", new FileCommandRequestParser() },
        { "find", new FindCommandRequestParser() },
        { "ls", new LsCommandRequestParser() },
        { "mkdir", new MkdirCommandRequestParser() },
        { "move", new MoveCommandRequestParser() },
        { "pwd", new PwdCommandRequestParser() },
        { "rm", new RmCommandRequestParser() },
        { "rmdir", new RmdirCommandRequestParser() },
        { "touch", new TouchCommandRequestParser() },
        { "upload", new UploadCommandRequestParser() },

        // UI commands
        { "screenshot", new ScreenshotCommandRequestParser() }
    };

    private static IEnumerable<string> SplitCommandLine(string commandLine)
    {
        var inQuotes = false;

        return Split(commandLine, c =>
            {
                if (c == '\"')
                    inQuotes = !inQuotes;

                return !inQuotes && c == ' ';
            })
            .Select(arg => TrimMatchingQuotes(arg.Trim(), '\"'))
            .Where(arg => !string.IsNullOrEmpty(arg));
    }

    private static IEnumerable<string> Split(string str,
        Func<char, bool> controller)
    {
        var nextPiece = 0;

        for (var c = 0; c < str.Length; c++)
        {
            if (!controller(str[c])) continue;
            yield return str.Substring(nextPiece, c - nextPiece);
            nextPiece = c + 1;
        }

        yield return str.Substring(nextPiece);
    }

    private static string TrimMatchingQuotes(string input, char quote)
    {
        if (input.Length >= 2 &&
            input[0] == quote && input[^1] == quote)
            return input.Substring(1, input.Length - 2);

        return input;
    }

    private IRequestParser GetParser(string command)
    {
        return _parsers[command];
    }

    public ICommandRequestModel Parse(string request)
    {
        var parts = SplitCommandLine(request).ToArray();
        var command = parts[0];
        var args = parts.Skip(1).ToArray();
        var parser = GetParser(command);
        return parser.ParseRequest(args);
    }
}