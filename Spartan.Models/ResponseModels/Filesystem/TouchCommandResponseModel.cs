using MessagePack;

namespace Spartan.Models.ResponseModels.Filesystem;

[MessagePackObject]
public class TouchCommandResponseModel : ICommandResponseModel
{
    [Key("Command")] public required string Command { get; set; } = "touch";
    [Key("Output")] public required string Output { get; set; }
}