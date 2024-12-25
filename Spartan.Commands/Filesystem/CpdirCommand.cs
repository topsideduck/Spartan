using Spartan.Models.RequestModels;
using Spartan.Models.RequestModels.Filesystem;
using Spartan.Models.ResponseModels;
using Spartan.Models.ResponseModels.Filesystem;

namespace Spartan.Commands.Filesystem;

public class CpdirCommand : ICommand
{
    public ICommandResponseModel Execute(ICommandRequestModel request)
    {
        var cpdirRequest = (CpdirCommandRequestModel)request;

        return new CpdirCommandResponseModel
        {
            Command = cpdirRequest.Command,
            Output = CopyDirectory(cpdirRequest.SourceDirectoryPath, cpdirRequest.DestinationDirectoryPath)
        };
    }

    private static string CopyDirectory(string sourceDir, string destinationDir)
    {
        if (sourceDir.StartsWith("~"))
        {
            var homeDirectory = GetHomeDirectory();
            if (homeDirectory == null) return "Error: Unable to determine the home directory.";

            sourceDir = sourceDir.Replace("~", homeDirectory);
        }

        if (destinationDir.StartsWith("~"))
        {
            var homeDirectory = GetHomeDirectory();
            if (homeDirectory == null) return "Error: Unable to determine the home directory.";

            destinationDir = destinationDir.Replace("~", homeDirectory);
        }

        if (!Directory.Exists(sourceDir)) return $"Error: The source directory does not exist: {sourceDir}";

        try
        {
            // Create the destination directory if it doesn't exist
            Directory.CreateDirectory(destinationDir);

            // Copy all files in the source directory
            foreach (var file in Directory.GetFiles(sourceDir))
            {
                var destFile = Path.Combine(destinationDir, Path.GetFileName(file));
                File.Copy(file, destFile, true); // Overwrite if the file already exists
            }

            // Copy all subdirectories in the source directory
            foreach (var subdir in Directory.GetDirectories(sourceDir))
            {
                var destSubdir = Path.Combine(destinationDir, Path.GetFileName(subdir));
                var result = CopyDirectory(subdir, destSubdir); // Recursively copy subdirectories
                if (!result.StartsWith("Directory copied")) return result; // If there's an error, return it immediately
            }

            return $"Directory copied from {sourceDir} to {destinationDir}";
        }
        catch (Exception ex)
        {
            return $"Error: Unable to copy the directory. {ex.Message}";
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