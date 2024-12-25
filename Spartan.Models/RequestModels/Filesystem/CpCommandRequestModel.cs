using MessagePack;

namespace Spartan.Models.RequestModels.Filesystem;

[MessagePackObject]
public class CpCommandRequestModel : ICommandRequestModel
{
    [Key("SourceFilePath")] public required string SourceFilePath { get; set; }
    [Key("DestinationFilePath")] public required string DestinationFilePath { get; set; }
    [Key("Command")] public string Command { get; set; } = "cp";
}