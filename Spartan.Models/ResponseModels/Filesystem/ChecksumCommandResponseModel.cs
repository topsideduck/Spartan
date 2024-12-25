using MessagePack;

namespace Spartan.Models.ResponseModels.Filesystem;

[MessagePackObject]
public class ChecksumCommandResponseModel : ICommandResponseModel
{
    [Key("Command")] public required string Command { get; set; } = "checksum";
    [Key("Output")] public required string Output { get; set; }
}