using Spartan.Models.ResponseModels;
using Spartan.Models.ResponseModels.UI;

namespace Spartan.Server.ResponseFormatters.UI;

public class ScreenshotCommandResponseFormatter : IResponseFormatter
{
    public string FormatResponse(ICommandResponseModel response)
    {
        var downloadResponse = (ScreenshotCommandResponseModel)response;
        WriteFile(downloadResponse.FileContents, downloadResponse.LocalDestinationPath);

        return downloadResponse.Output;
    }

    private static void WriteFile(List<byte[]> fileChunks, string destinationPath)
    {
        if (destinationPath.StartsWith("~"))
        {
            var homeDirectory = GetHomeDirectory();
            if (homeDirectory == null) throw new Exception("Error: Unable to determine the home directory.");

            destinationPath = destinationPath.Replace("~", homeDirectory);
        }

        if (fileChunks == null || fileChunks.Count == 0)
            throw new ArgumentException("The fileChunks list is empty or null.");

        try
        {
            // Write the file data to the destinationPath
            using var fileStream = new FileStream(destinationPath, FileMode.Create, FileAccess.Write);
            foreach (var chunk in fileChunks.Where(chunk => chunk.Length > 0))
                fileStream.Write(chunk, 0, chunk.Length);
        }
        catch (Exception ex)
        {
            throw new Exception($"Error processing the file: {ex.Message}", ex);
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