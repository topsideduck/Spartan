using Spartan.Models.RequestModels;
using Spartan.Models.RequestModels.Filesystem;
using Spartan.Models.ResponseModels;
using Spartan.Models.ResponseModels.Filesystem;

namespace Spartan.Commands.Filesystem;

public class LsCommand : ICommand
{
    public ICommandResponseModel Execute(ICommandRequestModel request)
    {
        var lsRequest = (LsCommandRequestModel)request;

        return new LsCommandResponseModel
        {
            Command = lsRequest.Command,
            Output = ListDirectoryContents(lsRequest.Path)
        };
    }

    private static string ListDirectoryContents(string directoryPath)
    {
        // Handle tilde (~) for home directory
        if (directoryPath.StartsWith("~"))
        {
            var homeDirectory = GetHomeDirectory();
            if (homeDirectory == null) return "Error: Unable to determine the home directory.";

            directoryPath = directoryPath.Replace("~", homeDirectory);
        }

        if (!Directory.Exists(directoryPath))
        {
            return $"Error: The system cannot find the path specified: {directoryPath}";
        }

        try
        {
            var directories = Directory.GetDirectories(directoryPath);
            var files = Directory.GetFiles(directoryPath);

            // Sort directories and files alphabetically
            Array.Sort(directories);
            Array.Sort(files);

            var output = directories.Aggregate("Directories:\n",
                (current, dir) => current + $"- {Path.GetFileName(dir)}\n");

            output += "\nFiles:\n";

            return files.Aggregate(output, (current, file) => current + $"- {Path.GetFileName(file)}\n").Trim();
        }
        catch (Exception ex)
        {
            return $"Error: Unable to access the directory contents. {ex.Message}";
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