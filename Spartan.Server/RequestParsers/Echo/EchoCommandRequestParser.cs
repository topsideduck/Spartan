using Spartan.Models.RequestModels;
using Spartan.Models.RequestModels.Echo;

namespace Spartan.Server.RequestParsers.Echo;

public class EchoCommandRequestParser : IRequestParser
{
    public ICommandRequestModel ParseRequest(string[] arguments)
    {
        return new EchoCommandRequestModel
        {
            Message = arguments[0]
        };
    }
}