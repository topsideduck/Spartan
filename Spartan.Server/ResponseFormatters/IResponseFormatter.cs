using Spartan.Models.ResponseModels;

namespace Spartan.Server.ResponseFormatters;

public interface IResponseFormatter
{
    public string FormatResponse(ICommandResponseModel response);
}