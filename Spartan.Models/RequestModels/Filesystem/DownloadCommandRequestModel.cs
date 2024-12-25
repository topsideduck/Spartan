using MessagePack;

namespace Spartan.Models.RequestModels.Filesystem;

[MessagePackObject]
public class DownloadCommandRequestModel : ICommandRequestModel
{
    [Key("RemoteSourcePath")] public required string RemoteSourcePath { get; set; }
    [Key("LocalDestinationPath")] public required string LocalDestinationPath { get; set; }
    [Key("Command")] public string Command { get; set; } = "download";
}