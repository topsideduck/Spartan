using Spartan.Models.RequestModels;
using Spartan.Models.RequestModels.Filesystem;
using Spartan.Models.ResponseModels;
using Spartan.Models.ResponseModels.Filesystem;

namespace Spartan.Commands.Filesystem;

public class TouchCommand : ICommand
{
    public ICommandResponseModel Execute(ICommandRequestModel requestModel)
    {
        var touchRequest = (TouchCommandRequestModel)requestModel;

        return new TouchCommandResponseModel
        {
            Command = touchRequest.Command,
            Output = TouchFile(touchRequest.FileName)
        };
    }

    private static string TouchFile(string filePath)
    {
        try
        {
            // Handle tilde (~) for home directory
            if (filePath.StartsWith("~"))
            {
                var homeDirectory = GetHomeDirectory();
                if (homeDirectory == null) return "Error: Unable to determine the home directory.";

                filePath = filePath.Replace("~", homeDirectory);
            }

            if (File.Exists(filePath))
            {
                // Update the last modified time
                File.SetLastWriteTime(filePath, DateTime.Now);
                return $"File timestamp updated: {filePath}";
            }

            // Create a new empty file
            File.Create(filePath).Dispose(); // Ensure the file is closed immediately
            return $"File created: {filePath}";
        }
        catch (Exception ex)
        {
            return $"Error: Unable to process the file. {ex.Message}";
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