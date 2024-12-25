using Spartan.Models.RequestModels;
using Spartan.Models.ResponseModels;

namespace Spartan.Commands;

public interface ICommand
{
    public ICommandResponseModel Execute(ICommandRequestModel request);
}