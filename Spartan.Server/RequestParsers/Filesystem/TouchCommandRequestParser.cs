using Spartan.Models.RequestModels;
using Spartan.Models.RequestModels.Filesystem;

namespace Spartan.Server.RequestParsers.Filesystem;

public class TouchCommandRequestParser : IRequestParser
{
    public ICommandRequestModel ParseRequest(string[] arguments)
    {
        return new TouchCommandRequestModel
        {
            FileName = arguments[0]
        };
    }
}