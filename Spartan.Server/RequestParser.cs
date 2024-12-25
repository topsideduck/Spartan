using Spartan.Models.RequestModels;
using Spartan.Server.RequestParsers;
using Spartan.Server.RequestParsers.Echo;
using Spartan.Server.RequestParsers.Filesystem;

namespace Spartan.Server;

public class RequestParser
{
    private readonly Dictionary<string, IRequestParser> _parsers = new()
    {
        { "echo", new EchoCommandRequestParser() },
        { "cd", new CdCommandRequestParser() }
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