using System.Security.Cryptography;
using Spartan.Models.RequestModels;
using Spartan.Models.RequestModels.Filesystem;
using Spartan.Models.ResponseModels;
using Spartan.Models.ResponseModels.Filesystem;

namespace Spartan.Commands.Filesystem;

public class ChecksumCommand : ICommand
{
    public ICommandResponseModel Execute(ICommandRequestModel request)
    {
        var checksumRequest = (ChecksumCommandRequestModel)request;

        return new ChecksumCommandResponseModel
        {
            Command = checksumRequest.Command,
            Output = CalculateFileChecksum(checksumRequest.FilePath)
        };
    }

    private static string CalculateFileChecksum(string filePath)
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

            if (!File.Exists(filePath)) return $"Error: The file does not exist: {filePath}";

            using var sha256 = SHA256.Create();
            using var fileStream = File.OpenRead(filePath);
            var hashBytes = sha256.ComputeHash(fileStream);
            var checksum = Convert.ToHexStringLower(hashBytes);
            return $"Checksum (SHA-256) of file {filePath}: {checksum}";
        }
        catch (Exception ex)
        {
            return $"Error: Unable to calculate checksum. {ex.Message}";
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