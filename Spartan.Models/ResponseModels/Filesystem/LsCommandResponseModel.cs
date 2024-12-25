using MessagePack;

namespace Spartan.Models.ResponseModels.Filesystem;

[MessagePackObject]
public class LsCommandResponseModel : ICommandResponseModel
{
    [Key("Command")] public required string Command { get; set; } = "ls";
    [Key("Output")] public required string Output { get; set; }
}