using MessagePack;

namespace Spartan.Models.ResponseModels.Filesystem;

[MessagePackObject]
public class RmCommandResponseModel : ICommandResponseModel
{
    [Key("Command")] public required string Command { get; set; } = "rm";
    [Key("Output")] public required string Output { get; set; }
}