using MessagePack;

namespace Spartan.Models.ResponseModels.Filesystem;

[MessagePackObject]
public class FindCommandResponseModel : ICommandResponseModel
{
    [Key("Command")] public required string Command { get; set; } = "find";
    [Key("Output")] public required string Output { get; set; }
}