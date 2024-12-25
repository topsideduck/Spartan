using Spartan.Models.RequestModels;
using Spartan.Models.RequestModels.Filesystem;
using Spartan.Models.ResponseModels;
using Spartan.Models.ResponseModels.Filesystem;

namespace Spartan.Commands.Filesystem;

public class MoveCommand : ICommand
{
    public ICommandResponseModel Execute(ICommandRequestModel request)
    {
        var moveRequest = (MoveCommandRequestModel)request;

        return new MoveCommandResponseModel
        {
            Command = moveRequest.Command,
            Output = MoveFileOrDirectory(moveRequest.SourcePath, moveRequest.DestinationPath)
        };
    }

    private static string MoveFileOrDirectory(string sourcePath, string destinationPath)
    {
        if (sourcePath.StartsWith("~"))
        {
            var homeDirectory = GetHomeDirectory();
            if (homeDirectory == null) return "Error: Unable to determine the home directory.";

            sourcePath = sourcePath.Replace("~", homeDirectory);
        }

        if (destinationPath.StartsWith("~"))
        {
            var homeDirectory = GetHomeDirectory();
            if (homeDirectory == null) return "Error: Unable to determine the home directory.";

            destinationPath = destinationPath.Replace("~", homeDirectory);
        }

        if (!File.Exists(sourcePath) && !Directory.Exists(sourcePath))
            return $"Error: The source path does not exist: {sourcePath}";

        try
        {
            if (File.Exists(sourcePath))
                File.Move(sourcePath, destinationPath);
            else if (Directory.Exists(sourcePath)) Directory.Move(sourcePath, destinationPath);

            return $"Moved from {sourcePath} to {destinationPath}";
        }
        catch (Exception ex)
        {
            return $"Error: Unable to move file or directory. {ex.Message}";
        }
    }


    private static string? GetHomeDirectory()
    {
        // Cross-platform home directory determination
        if (Environment.OSVersion.Platform == PlatformID.Unix || Environment.OSVersion.Platform == PlatformID.MacOSX)
            return Environment.GetEnvironmentVariable("HOME");

        return Environment.GetEnvironmentVariable("USERPROFILE");
    }
}