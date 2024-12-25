using Spartan.Models.RequestModels;
using Spartan.Models.RequestModels.Filesystem;
using Spartan.Models.ResponseModels;
using Spartan.Models.ResponseModels.Filesystem;

namespace Spartan.Commands.Filesystem;

public class RmCommand : ICommand
{
    public ICommandResponseModel Execute(ICommandRequestModel request)
    {
        var rmRequest = (RmCommandRequestModel)request;

        return new RmCommandResponseModel
        {
            Command = rmRequest.Command,
            Output = RemoveFile(rmRequest.FilePath)
        };
    }

    private static string RemoveFile(string filePath)
    {
        if (filePath.StartsWith("~"))
        {
            var homeDirectory = GetHomeDirectory();
            if (homeDirectory == null) return "Error: Unable to determine the home directory.";

            filePath = filePath.Replace("~", homeDirectory);
        }

        if (!File.Exists(filePath)) return $"Error: The file does not exist: {filePath}";

        try
        {
            File.Delete(filePath);
            return $"File removed: {filePath}";
        }
        catch (Exception ex)
        {
            return $"Error: Unable to remove file. {ex.Message}";
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