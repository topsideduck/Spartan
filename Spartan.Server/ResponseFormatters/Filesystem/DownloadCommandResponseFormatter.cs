using System.IO.Compression;
using Spartan.Models.ResponseModels;
using Spartan.Models.ResponseModels.Filesystem;

namespace Spartan.Server.ResponseFormatters.Filesystem;

public class DownloadCommandResponseFormatter : IResponseFormatter
{
    public string FormatResponse(ICommandResponseModel response)
    {
        var downloadResponse = (DownloadCommandResponseModel)response;
        WriteOrExtractFile(downloadResponse.FileContents, downloadResponse.LocalDestinationDirectoryPath,
            downloadResponse.IsDirectory);

        return downloadResponse.Output;
    }

    private static void WriteOrExtractFile(List<byte[]> fileChunks, string destinationPath, bool isDirectory)
    {
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
}