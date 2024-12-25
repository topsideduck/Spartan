using MessagePack;

namespace Spartan.Models.RequestModels.Filesystem;

[MessagePackObject]
public class UploadCommandRequestModel : ICommandRequestModel
{
    [Key("RemoteDestinationDirectoryPath")]
    public required string RemoteDestinationDirectoryPath { get; set; }

    [Key("LocalSourceDirectoryPath")] public required string LocalSourceDirectoryPath { get; set; }
    [Key("FileContents")] public required List<byte[]> FileContents { get; set; }
    [Key("IsDirectory")] public required bool IsDirectory { get; set; }
    [Key("Command")] public string Command { get; set; } = "upload";
}