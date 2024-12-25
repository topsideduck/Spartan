using Spartan.Models.RequestModels;
using Spartan.Models.RequestModels.Filesystem;

namespace Spartan.Server.RequestParsers.Filesystem;

public class FindCommandRequestParser : IRequestParser
{
    public ICommandRequestModel ParseRequest(string[] arguments)
    {
        return new FindCommandRequestModel
        {
            DirectoryPath = arguments[0],
            SearchPattern = arguments[1]
        };
    }
}