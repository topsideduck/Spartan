using MessagePack;

namespace Spartan.Models.RequestModels.Filesystem;

[MessagePackObject]
public class TouchCommandRequestModel : ICommandRequestModel
{
    [Key("FileName")] public required string FileName { get; set; }
    [Key("Command")] public string Command { get; set; } = "touch";
}