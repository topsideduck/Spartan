using Spartan.Models.RequestModels;
using Spartan.Models.RequestModels.Filesystem;

namespace Spartan.Server.RequestParsers.Filesystem;

public class LsCommandRequestParser : IRequestParser
{
    public ICommandRequestModel ParseRequest(string[] arguments)
    {
        return new LsCommandRequestModel
        {
            DirectoryPath = arguments.Length == 0 ? "." : arguments[0]
        };
    }
}