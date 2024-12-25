using Spartan.Models.RequestModels;
using Spartan.Models.RequestModels.Filesystem;

namespace Spartan.Server.RequestParsers.Filesystem;

public class PwdCommandRequestParser : IRequestParser
{
    public ICommandRequestModel ParseRequest(string[] arguments)
    {
        return new PwdCommandRequestModel();
    }
}