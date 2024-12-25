using Spartan.Models.RequestModels;
using Spartan.Models.RequestModels.Filesystem;
using Spartan.Models.ResponseModels;
using Spartan.Models.ResponseModels.Filesystem;

namespace Spartan.Commands.Filesystem;

public class CatCommand : ICommand
{
    public ICommandResponseModel Execute(ICommandRequestModel request)
    {
        var currentDirectory = Directory.GetCurrentDirectory();

        var cdRequest = (CatCommandRequestModel)request;

        return new CatCommandResponseModel
        {
            Command = cdRequest.Command,
            Output = DisplayFileContents(cdRequest.FileName)
        };
    }

    private static string DisplayFileContents(string filePath)
    {
        // Handle tilde (~) for home directory
        if (filePath.StartsWith("~"))
        {
            var homeDirectory = GetHomeDirectory();
            if (homeDirectory == null) return "Error: Unable to determine the home directory.";

            filePath = filePath.Replace("~", homeDirectory);
        }

        if (!File.Exists(filePath)) return $"Error: The system cannot find the file specified: {filePath}";

        try
        {
            return File.ReadAllText(filePath);
        }
        catch (Exception ex)
        {
            return $"Error: Unable to read the file. {ex.Message}";
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