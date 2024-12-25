using Spartan.Models.RequestModels;
using Spartan.Models.RequestModels.Filesystem;

namespace Spartan.Server.RequestParsers.Filesystem;

public class DownloadCommandRequestParser : IRequestParser
{
    public ICommandRequestModel ParseRequest(string[] arguments)
    {
        return new DownloadCommandRequestModel
        {
            RemoteSourceDirectoryPath = arguments[0],
            LocalDestinationDirectoryPath = arguments[1]
        };
    }
}