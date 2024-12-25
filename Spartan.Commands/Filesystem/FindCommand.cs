using Spartan.Models.RequestModels;
using Spartan.Models.RequestModels.Filesystem;
using Spartan.Models.ResponseModels;
using Spartan.Models.ResponseModels.Filesystem;

namespace Spartan.Commands.Filesystem;

public class FindCommand : ICommand
{
    public ICommandResponseModel Execute(ICommandRequestModel request)
    {
        var findRequest = (FindCommandRequestModel)request;

        return new FindCommandResponseModel
        {
            Command = findRequest.Command,
            Output = FindFiles(findRequest.DirectoryPath, findRequest.SearchPattern)
        };
    }

    private static string FindFiles(string directoryPath, string searchPattern)
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
            var files = Directory.GetFiles(directoryPath, searchPattern, SearchOption.AllDirectories);
            if (files.Length == 0) return $"No files found matching pattern '{searchPattern}' in {directoryPath}";

            var result = "Found files:\n";
            foreach (var file in files) result += $"- {file}\n";

            return result;
        }
        catch (Exception ex)
        {
            return $"Error: Unable to find files. {ex.Message}";
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