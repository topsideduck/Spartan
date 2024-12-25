using Spartan.Models.ResponseModels;
using Spartan.Models.ResponseModels.Filesystem;

namespace Spartan.Server.ResponseFormatters.Filesystem;

public class CdCommandResponseFormatter : IResponseFormatter
{
    public string FormatResponse(ICommandResponseModel response)
    {
        var echoResponse = (CdCommandResponseModel)response;
        return echoResponse.Output;
    }
}