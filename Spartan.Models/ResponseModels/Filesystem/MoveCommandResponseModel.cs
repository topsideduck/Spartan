using MessagePack;

namespace Spartan.Models.ResponseModels.Filesystem;

[MessagePackObject]
public class MoveCommandResponseModel : ICommandResponseModel
{
    [Key("Command")] public required string Command { get; set; } = "move";
    [Key("Output")] public required string Output { get; set; }
}