using MessagePack;

namespace Spartan.Models.RequestModels.Filesystem;

[MessagePackObject]
public class UploadCommandRequestModel : ICommandRequestModel
{
    [Key("RemoteDestinationPath")] public required string RemoteDestinationPath { get; set; }

    [Key("LocalSourcePath")] public required string LocalSourcePath { get; set; }
    [Key("FileContents")] public required List<byte[]> FileContents { get; set; }
    [Key("IsDirectory")] public required bool IsDirectory { get; set; }
    [Key("Command")] public string Command { get; set; } = "upload";
}