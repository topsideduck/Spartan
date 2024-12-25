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
            Output = ChangeDirectory(ref currentDirectory, cdRequest.DirectoryPath)
        };
    }

    private static string ChangeDirectory(ref string currentDirectory, string targetDirectory)
    {
        string newDirectory;

        if (targetDirectory.StartsWith("~"))
        {
            var homeDirectory = GetHomeDirectory();

            if (homeDirectory == null) return "Error: Unable to determine the home directory.";

            // Replace ~ with the home directory path
            targetDirectory = targetDirectory.Replace("~", homeDirectory);
        }

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

    private static string? GetHomeDirectory()
    {
        // Cross-platform home directory determination
        if (Environment.OSVersion.Platform == PlatformID.Unix || Environment.OSVersion.Platform == PlatformID.MacOSX)
            return Environment.GetEnvironmentVariable("HOME");

        return Environment.GetEnvironmentVariable("USERPROFILE");
    }
}