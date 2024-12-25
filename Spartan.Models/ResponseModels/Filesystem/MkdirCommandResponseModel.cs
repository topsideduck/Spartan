using MessagePack;

namespace Spartan.Models.ResponseModels.Filesystem;

[MessagePackObject]
public class MkdirCommandResponseModel : ICommandResponseModel
{
    [Key("Command")] public required string Command { get; set; } = "mkdir";
    [Key("Output")] public required string Output { get; set; }
}