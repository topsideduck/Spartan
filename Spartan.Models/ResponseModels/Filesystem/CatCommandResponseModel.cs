using MessagePack;

namespace Spartan.Models.ResponseModels.Filesystem;

[MessagePackObject]
public class CatCommandResponseModel : ICommandResponseModel
{
    [Key("Command")] public required string Command { get; set; } = "cat";
    [Key("Output")] public required string Output { get; set; }
}