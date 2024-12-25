using Spartan.Models.RequestModels;

namespace Spartan.Server.RequestParsers;

public interface IRequestParser
{
    public ICommandRequestModel ParseRequest(string[] arguments);
}