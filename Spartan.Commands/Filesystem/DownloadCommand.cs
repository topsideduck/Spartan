using System.IO.Compression;
using Spartan.Models.RequestModels;
using Spartan.Models.RequestModels.Filesystem;
using Spartan.Models.ResponseModels;
using Spartan.Models.ResponseModels.Filesystem;

namespace Spartan.Commands.Filesystem;

public class DownloadCommand : ICommand
{
    public ICommandResponseModel Execute(ICommandRequestModel request)
    {
        var downloadRequest = (DownloadCommandRequestModel)request;

        var readData = ReadPathAsByteArray(downloadRequest.RemoteSourcePath);

        return new DownloadCommandResponseModel
        {
            Command = downloadRequest.Command,
            RemoteSourcePath = downloadRequest.RemoteSourcePath,
            LocalDestinationPath = downloadRequest.LocalDestinationPath,
            IsDirectory = readData.Item1,
            FileContents = readData.Item2,
            Output =
                $"Downloaded {downloadRequest.RemoteSourcePath} (remote) to {downloadRequest.LocalDestinationPath} (local)."
        };
    }

    private static List<byte[]> ReadInChunks(string filePath, int chunkSize = int.MaxValue)
    {
        if (!File.Exists(filePath))
            throw new FileNotFoundException($"The file does not exist: {filePath}");

        var result = new List<byte[]>();

        try
        {
            using var fileStream = new FileStream(filePath, FileMode.Open, FileAccess.Read);
            var fileLength = fileStream.Length;
            var bytesRemaining = fileLength;

            while (bytesRemaining > 0)
            {
                var currentChunkSize = (int)Math.Min(chunkSize, bytesRemaining);
                var chunk = new byte[currentChunkSize];

                var bytesRead = fileStream.Read(chunk, 0, currentChunkSize);
                if (bytesRead == 0)
                    break; // End of file

                result.Add(chunk);
                bytesRemaining -= bytesRead;
            }

            return result;
        }
        catch (Exception ex)
        {
            throw new Exception($"Error reading the file in chunks: {ex.Message}", ex);
        }
    }


    private static (bool, List<byte[]>) ReadPathAsByteArray(string path)
    {
        if (path.StartsWith("~"))
        {
            var homeDirectory = GetHomeDirectory();
            if (homeDirectory == null) throw new Exception("Error: Unable to determine the home directory.");

            path = path.Replace("~", homeDirectory);
        }

        try
        {
            if (File.Exists(path))
                // Path is a file, read it into a byte array
                return (false, ReadInChunks(path));

            if (!Directory.Exists(path))
                throw new FileNotFoundException("The specified path does not exist or is invalid.");
            // Path is a directory, create a temporary zip file
            var tempZipPath = Path.Combine(Path.GetTempPath(), $"{Path.GetFileName(path)}.zip");

            if (File.Exists(tempZipPath)) File.Delete(tempZipPath); // Clean up any existing file with the same name

            ZipFile.CreateFromDirectory(path, tempZipPath);

            // Read the ZIP file into a byte array
            // var zipBytes = File.ReadAllBytes(tempZipPath);
            var zipBytes = ReadInChunks(tempZipPath);

            // Clean up the temporary zip file
            File.Delete(tempZipPath);

            return (true, zipBytes);
        }
        catch (Exception ex)
        {
            throw new Exception($"Error processing the path: {ex.Message}", ex);
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