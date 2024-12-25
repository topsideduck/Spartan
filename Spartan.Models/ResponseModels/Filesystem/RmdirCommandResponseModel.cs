using MessagePack;

namespace Spartan.Models.ResponseModels.Filesystem;

[MessagePackObject]
public class RmdirCommandResponseModel : ICommandResponseModel
{
    [Key("Command")] public required string Command { get; set; } = "rmdir";
    [Key("Output")] public required string Output { get; set; }
}