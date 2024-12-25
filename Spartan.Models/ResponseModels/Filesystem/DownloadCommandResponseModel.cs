using MessagePack;

namespace Spartan.Models.ResponseModels.Filesystem;

[MessagePackObject]
public class DownloadCommandResponseModel : ICommandResponseModel
{
    [Key("RemoteSourcePath")] public required string RemoteSourcePath { get; set; }
    [Key("LocalDestinationPath")] public required string LocalDestinationPath { get; set; }
    [Key("FileContents")] public required List<byte[]> FileContents { get; set; }
    [Key("IsDirectory")] public required bool IsDirectory { get; set; }
    [Key("Command")] public required string Command { get; set; } = "download";
    [Key("Output")] public required string Output { get; set; }
}