using MessagePack;

namespace Spartan.Models.ResponseModels.Filesystem;

[MessagePackObject]
public class UploadCommandResponseModel : ICommandResponseModel
{
    [Key("RemoteSourceDirectoryPath")] public required string RemoteSourceDirectoryPath { get; set; }
    [Key("LocalDestinationDirectoryPath")] public required string LocalDestinationDirectoryPath { get; set; }
    [Key("Command")] public required string Command { get; set; } = "upload";
    [Key("Output")] public required string Output { get; set; }
}