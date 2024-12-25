using Spartan.Models.RequestModels;
using Spartan.Models.RequestModels.Filesystem;

namespace Spartan.Server.RequestParsers.Filesystem;

public class CpdirCommandRequestParser : IRequestParser
{
    public ICommandRequestModel ParseRequest(string[] arguments)
    {
        return new CpdirCommandRequestModel
        {
            SourceDirectoryPath = arguments[0],
            DestinationDirectoryPath = arguments[1]
        };
    }
}