using Spartan.Models.RequestModels;
using Spartan.Models.RequestModels.Echo;
using Spartan.Models.ResponseModels;
using Spartan.Models.ResponseModels.Echo;

namespace Spartan.Commands.Echo;

public class EchoCommand : ICommand
{
    public ICommandResponseModel Execute(ICommandRequestModel request)
    {
        var echoRequest = (EchoCommandRequestModel)request;

        return new EchoCommandResponseModel
        {
            Command = echoRequest.Command,
            Output = $"You said: {echoRequest.Message}"
        };
    }
}