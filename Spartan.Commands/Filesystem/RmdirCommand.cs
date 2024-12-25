using Spartan.Models.RequestModels;
using Spartan.Models.RequestModels.Filesystem;
using Spartan.Models.ResponseModels;
using Spartan.Models.ResponseModels.Filesystem;

namespace Spartan.Commands.Filesystem;

public class RmdirCommand : ICommand
{
    public ICommandResponseModel Execute(ICommandRequestModel request)
    {
        var rmRequest = (RmdirCommandRequestModel)request;

        return new RmdirCommandResponseModel
        {
            Command = rmRequest.Command,
            Output = RemoveDirectory(rmRequest.DirectoryPath)
        };
    }

    private static string RemoveDirectory(string directoryPath)
    {
        if (directoryPath.StartsWith("~"))
        {
            var homeDirectory = GetHomeDirectory();
            if (homeDirectory == null) return "Error: Unable to determine the home directory.";

            directoryPath = directoryPath.Replace("~", homeDirectory);
        }

        if (!Directory.Exists(directoryPath)) return $"Error: The directory does not exist: {directoryPath}";

        try
        {
            Directory.Delete(directoryPath, true); // true for recursive deletion
            return $"Directory removed: {directoryPath}";
        }
        catch (Exception ex)
        {
            return $"Error: Unable to remove directory. {ex.Message}";
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