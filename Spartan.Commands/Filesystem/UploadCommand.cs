using System.IO.Compression;
using Spartan.Models.RequestModels;
using Spartan.Models.RequestModels.Filesystem;
using Spartan.Models.ResponseModels;
using Spartan.Models.ResponseModels.Filesystem;

namespace Spartan.Commands.Filesystem;

public class UploadCommand : ICommand
{
    public ICommandResponseModel Execute(ICommandRequestModel request)
    {
        var uploadRequest = (UploadCommandRequestModel)request;

        WriteOrExtractFile(uploadRequest.FileContents, uploadRequest.RemoteDestinationPath, uploadRequest.IsDirectory);

        return new UploadCommandResponseModel
        {
            Command = uploadRequest.Command,
            Output =
                $"Uploaded {uploadRequest.LocalSourcePath} (local) to {uploadRequest.RemoteDestinationPath} (remote)."
        };
    }

    private static void WriteOrExtractFile(List<byte[]> fileChunks, string destinationPath, bool isDirectory)
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
            if (isDirectory)
            {
                // Create a temporary file to store the ZIP data
                var tempZipPath = Path.Combine(Path.GetTempPath(), $"{Guid.NewGuid()}.zip");

                // Write the ZIP data to the temporary file
                using (var tempZipStream = new FileStream(tempZipPath, FileMode.Create, FileAccess.Write))
                {
                    foreach (var chunk in fileChunks.Where(chunk => chunk.Length > 0))
                        tempZipStream.Write(chunk, 0, chunk.Length);
                }

                // Extract the ZIP data to the destinationPath
                if (!Directory.Exists(destinationPath)) Directory.CreateDirectory(destinationPath);

                ZipFile.ExtractToDirectory(tempZipPath, destinationPath);

                // Clean up the temporary ZIP file
                File.Delete(tempZipPath);

                Console.WriteLine($"ZIP file extracted to {destinationPath}");
            }
            else
            {
                // Write the file data to the destinationPath
                using (var fileStream = new FileStream(destinationPath, FileMode.Create, FileAccess.Write))
                {
                    foreach (var chunk in fileChunks.Where(chunk => chunk.Length > 0))
                        fileStream.Write(chunk, 0, chunk.Length);
                }

                Console.WriteLine($"File written to {destinationPath}");
            }
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