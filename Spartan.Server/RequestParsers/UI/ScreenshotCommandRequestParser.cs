using Spartan.Models.RequestModels;
using Spartan.Models.RequestModels.UI;

namespace Spartan.Server.RequestParsers.UI;

public class ScreenshotCommandRequestParser : IRequestParser
{
    public ICommandRequestModel ParseRequest(string[] arguments)
    {
        return new ScreenshotCommandRequestModel
        {
            LocalDestinationDirectory = arguments.Length == 0 ? Environment.CurrentDirectory : arguments[0]
        };
    }
}