using Spartan.Models.ResponseModels;
using Spartan.Models.ResponseModels.Filesystem;

namespace Spartan.Server.ResponseFormatters.Filesystem;

public class LsCommandResponseFormatter : IResponseFormatter
{
    public string FormatResponse(ICommandResponseModel response)
    {
        var echoResponse = (LsCommandResponseModel)response;
        return echoResponse.Output;
    }
}