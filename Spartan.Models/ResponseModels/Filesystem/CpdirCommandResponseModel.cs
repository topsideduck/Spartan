using MessagePack;

namespace Spartan.Models.ResponseModels.Filesystem;

[MessagePackObject]
public class CpdirCommandResponseModel : ICommandResponseModel
{
    [Key("Command")] public required string Command { get; set; } = "cpdir";
    [Key("Output")] public required string Output { get; set; }
}