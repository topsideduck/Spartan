using Spartan.Models.RequestModels;
using Spartan.Models.RequestModels.Filesystem;
using Spartan.Models.ResponseModels;
using Spartan.Models.ResponseModels.Filesystem;

namespace Spartan.Commands.Filesystem;

public class MkdirCommand : ICommand
{
    public ICommandResponseModel Execute(ICommandRequestModel request)
    {
        var mkdirRequest = (MkdirCommandRequestModel)request;

        return new MkdirCommandResponseModel
        {
            Command = mkdirRequest.Command,
            Output = MakeDirectory(mkdirRequest.DirectoryPath)
        };
    }

    private static string MakeDirectory(string directoryPath)
    {
        if (directoryPath.StartsWith("~"))
        {
            var homeDirectory = GetHomeDirectory();
            if (homeDirectory == null) return "Error: Unable to determine the home directory.";

            directoryPath = directoryPath.Replace("~", homeDirectory);
        }

        if (Directory.Exists(directoryPath)) return $"Error: The directory already exists: {directoryPath}";

        try
        {
            Directory.CreateDirectory(directoryPath);
            return $"Directory created: {directoryPath}";
        }
        catch (Exception ex)
        {
            return $"Error: Unable to create directory. {ex.Message}";
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