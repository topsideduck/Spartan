using MessagePack;

namespace Spartan.Models.RequestModels.Filesystem;

[MessagePackObject]
public class CatCommandRequestModel : ICommandRequestModel
{
    [Key("FilePath")] public required string FilePath { get; set; }
    [Key("Command")] public string Command { get; set; } = "cat";
}