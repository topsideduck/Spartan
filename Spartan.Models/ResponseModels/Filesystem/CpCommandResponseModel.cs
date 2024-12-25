using MessagePack;

namespace Spartan.Models.ResponseModels.Filesystem;

[MessagePackObject]
public class CpCommandResponseModel : ICommandResponseModel
{
    [Key("Command")] public required string Command { get; set; } = "cp";
    [Key("Output")] public required string Output { get; set; }
}