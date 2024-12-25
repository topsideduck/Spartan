using MessagePack;

namespace Spartan.Models.RequestModels.Filesystem;

[MessagePackObject]
public class MoveCommandRequestModel : ICommandRequestModel
{
    [Key("SourcePath")] public required string SourcePath { get; set; }
    [Key("DestinationPath")] public required string DestinationPath { get; set; }
    [Key("Command")] public string Command { get; set; } = "move";
}