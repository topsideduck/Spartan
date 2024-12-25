using Spartan.Models.ResponseModels;
using Spartan.Models.ResponseModels.Filesystem;

namespace Spartan.Server.ResponseFormatters.Filesystem;

public class TouchCommandResponseFormatter : IResponseFormatter
{
    public string FormatResponse(ICommandResponseModel response)
    {
        var touchResponse = (TouchCommandResponseModel)response;
        return touchResponse.Output;
    }
}