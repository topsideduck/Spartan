using Spartan.Models.RequestModels;
using Spartan.Models.RequestModels.Filesystem;
using Spartan.Models.ResponseModels;
using Spartan.Models.ResponseModels.Filesystem;

namespace Spartan.Commands.Filesystem;

public class CpCommand : ICommand
{
    public ICommandResponseModel Execute(ICommandRequestModel request)
    {
        var cpRequest = (CpCommandRequestModel)request;

        return new CpCommandResponseModel
        {
            Command = cpRequest.Command,
            Output = CopyFile(cpRequest.SourceFilePath, cpRequest.DestinationFilePath)
        };
    }

    private static string CopyFile(string sourcePath, string destinationPath)
    {
        // Handle tilde (~) for home directory
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

        if (!File.Exists(sourcePath)) return $"Error: The source file does not exist: {sourcePath}";

        try
        {
            File.Copy(sourcePath, destinationPath, true);
            return $"File copied from {sourcePath} to {destinationPath}";
        }
        catch (Exception ex)
        {
            return $"Error: Unable to copy file. {ex.Message}";
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