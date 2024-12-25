using MessagePack;

namespace Spartan.Models.RequestModels.Filesystem;

[MessagePackObject]
public class CpdirCommandRequestModel : ICommandRequestModel
{
    [Key("SourceDirectoryPath")] public required string SourceDirectoryPath { get; set; }
    [Key("DestinationDirectoryPath")] public required string DestinationDirectoryPath { get; set; }
    [Key("Command")] public string Command { get; set; } = "cpdir";
}