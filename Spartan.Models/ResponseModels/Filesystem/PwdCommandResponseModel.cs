using MessagePack;

namespace Spartan.Models.ResponseModels.Filesystem;

[MessagePackObject]
public class PwdCommandResponseModel : ICommandResponseModel
{
    [Key("Command")] public required string Command { get; set; } = "pwd";
    [Key("Output")] public required string Output { get; set; }
}