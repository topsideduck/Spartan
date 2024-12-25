using Spartan.Models.RequestModels;
using Spartan.Models.RequestModels.Filesystem;
using Spartan.Models.ResponseModels;
using Spartan.Models.ResponseModels.Filesystem;

namespace Spartan.Commands.Filesystem;

public class FileCommand : ICommand
{
    public ICommandResponseModel Execute(ICommandRequestModel request)
    {
        var fileRequest = (FileCommandRequestModel)request;

        return new FileCommandResponseModel
        {
            Command = fileRequest.Command,
            Output = GetFileInformation(fileRequest.FilePath)
        };
    }

    private static string GetFileInformation(string filePath)
    {
        // Handle tilde (~) for home directory
        if (filePath.StartsWith("~"))
        {
            var homeDirectory = GetHomeDirectory();
            if (homeDirectory == null) return "Error: Unable to determine the home directory.";

            filePath = filePath.Replace("~", homeDirectory);
        }

        if (!File.Exists(filePath)) return $"Error: The file does not exist: {filePath}";

        try
        {
            var fileInfo = new FileInfo(filePath);
            return
                $"File: {filePath}\nSize: {fileInfo.Length} bytes\nCreated: {fileInfo.CreationTime}\nModified: {fileInfo.LastWriteTime}";
        }
        catch (Exception ex)
        {
            return $"Error: Unable to retrieve file information. {ex.Message}";
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