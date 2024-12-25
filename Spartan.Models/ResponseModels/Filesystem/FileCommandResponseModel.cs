using MessagePack;

namespace Spartan.Models.ResponseModels.Filesystem;

[MessagePackObject]
public class FileCommandResponseModel : ICommandResponseModel
{
    [Key("Command")] public required string Command { get; set; } = "file";
    [Key("Output")] public required string Output { get; set; }
}