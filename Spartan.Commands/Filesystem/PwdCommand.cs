using Spartan.Models.RequestModels;
using Spartan.Models.RequestModels.Filesystem;
using Spartan.Models.ResponseModels;
using Spartan.Models.ResponseModels.Filesystem;

namespace Spartan.Commands.Filesystem;

public class PwdCommand : ICommand
{
    public ICommandResponseModel Execute(ICommandRequestModel request)
    {
        var pwdRequest = (PwdCommandRequestModel)request;

        return new PwdCommandResponseModel
        {
            Command = pwdRequest.Command,
            Output = PrintWorkingDirectory()
        };
    }

    private static string PrintWorkingDirectory()
    {
        return Directory.GetCurrentDirectory();
    }
}