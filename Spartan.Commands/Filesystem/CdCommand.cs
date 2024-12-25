using Spartan.Models.RequestModels;
using Spartan.Models.RequestModels.Filesystem;
using Spartan.Models.ResponseModels;
using Spartan.Models.ResponseModels.Filesystem;

namespace Spartan.Commands.Filesystem;

public class CdCommand : ICommand
{
    public ICommandResponseModel Execute(ICommandRequestModel request)
    {
        var currentDirectory = Directory.GetCurrentDirectory();

        var cdRequest = (CdCommandRequestModel)request;

        return new CdCommandResponseModel
        {
            Command = cdRequest.Command,
            Output = ChangeDirectory(ref currentDirectory, cdRequest.Path)
        };
    }

    private static string ChangeDirectory(ref string currentDirectory, string targetDirectory)
    {
        string newDirectory;

        if (targetDirectory == "..")
        {
            var parentDirectory = Directory.GetParent(currentDirectory);
            newDirectory = parentDirectory?.FullName ?? currentDirectory;
        }
        else if (Path.IsPathRooted(targetDirectory))
        {
            newDirectory = targetDirectory;
        }
        else
        {
            newDirectory = Path.Combine(currentDirectory, targetDirectory);
        }

        if (string.IsNullOrEmpty(newDirectory) || !Directory.Exists(newDirectory))
            return $"Error: The system cannot find the path specified: {targetDirectory}";

        currentDirectory = newDirectory;
        Directory.SetCurrentDirectory(currentDirectory);
        return $"Changed directory to: {currentDirectory}";
    }
}