using Spartan.Models.RequestModels;
using Spartan.Models.RequestModels.Filesystem;

namespace Spartan.Server.RequestParsers.Filesystem;

public class MoveCommandRequestParser : IRequestParser
{
    public ICommandRequestModel ParseRequest(string[] arguments)
    {
        return new MoveCommandRequestModel
        {
            SourcePath = arguments[0],
            DestinationPath = arguments[1]
        };
    }
}