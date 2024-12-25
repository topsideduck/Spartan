using Spartan.Models.ResponseModels;
using Spartan.Models.ResponseModels.Echo;

namespace Spartan.Server.ResponseFormatters.Echo;

public class EchoCommandResponseFormatter : IResponseFormatter
{
    public string FormatResponse(ICommandResponseModel response)
    {
        var echoResponse = (EchoCommandResponseModel)response;
        return echoResponse.Output;
    }
}