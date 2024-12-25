using MessagePack;

namespace Spartan.Models.RequestModels.Filesystem;

[MessagePackObject]
public class DownloadCommandRequestModel : ICommandRequestModel
{
    [Key("RemoteSourceDirectoryPath")] public required string RemoteSourceDirectoryPath { get; set; }
    [Key("LocalDestinationDirectoryPath")] public required string LocalDestinationDirectoryPath { get; set; }
    [Key("Command")] public string Command { get; set; } = "download";
}