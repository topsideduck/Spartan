using Spartan.Models.RequestModels;
using Spartan.Models.RequestModels.Filesystem;

namespace Spartan.Server.RequestParsers.Filesystem;

public class RmdirCommandRequestParser : IRequestParser
{
    public ICommandRequestModel ParseRequest(string[] arguments)
    {
        return new RmdirCommandRequestModel
        {
            DirectoryPath = arguments[0]
        };
    }
}