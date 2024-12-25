using System.IO.Compression;
using Spartan.Models.RequestModels;
using Spartan.Models.RequestModels.Filesystem;

namespace Spartan.Server.RequestParsers.Filesystem;

public class UploadCommandRequestParser : IRequestParser
{
    public ICommandRequestModel ParseRequest(string[] arguments)
    {
        var readData = ReadPathAsByteArray(arguments[0]);

        return new UploadCommandRequestModel
        {
            LocalSourcePath = arguments[0],
            RemoteDestinationPath = arguments[1],
            IsDirectory = readData.Item1,
            FileContents = readData.Item2
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
}