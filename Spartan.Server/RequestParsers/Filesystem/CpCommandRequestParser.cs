using Spartan.Models.RequestModels;
using Spartan.Models.RequestModels.Filesystem;

namespace Spartan.Server.RequestParsers.Filesystem;

public class CpCommandRequestParser : IRequestParser
{
    public ICommandRequestModel ParseRequest(string[] arguments)
    {
        return new CpCommandRequestModel
        {
            SourceFilePath = arguments[0],
            DestinationFilePath = arguments[1]
        };
    }
}