using Spartan.Models.RequestModels;
using Spartan.Models.RequestModels.Filesystem;

namespace Spartan.Server.RequestParsers.Filesystem;

public class RmCommandRequestParser : IRequestParser
{
    public ICommandRequestModel ParseRequest(string[] arguments)
    {
        return new RmCommandRequestModel
        {
            FilePath = arguments[0]
        };
    }
}