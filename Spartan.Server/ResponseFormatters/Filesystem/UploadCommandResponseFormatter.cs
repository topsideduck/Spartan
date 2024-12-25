using Spartan.Models.ResponseModels;
using Spartan.Models.ResponseModels.Filesystem;

namespace Spartan.Server.ResponseFormatters.Filesystem;

public class UploadCommandResponseFormatter : IResponseFormatter
{
    public string FormatResponse(ICommandResponseModel response)
    {
        var uploadResponse = (UploadCommandResponseModel)response;
        return uploadResponse.Output;
    }
}