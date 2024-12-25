using Spartan.Models.RequestModels;
using Spartan.Models.RequestModels.Filesystem;

namespace Spartan.Server.RequestParsers.Filesystem;

public class DownloadCommandRequestParser : IRequestParser
{
    public ICommandRequestModel ParseRequest(string[] arguments)
    {
        return new DownloadCommandRequestModel
        {
            RemoteSourcePath = arguments[0],
            LocalDestinationPath = arguments[1]
        };
    }
}