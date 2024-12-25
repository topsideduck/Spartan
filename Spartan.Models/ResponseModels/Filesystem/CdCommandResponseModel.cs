using MessagePack;

namespace Spartan.Models.ResponseModels.Filesystem;

[MessagePackObject]
public class CdCommandResponseModel : ICommandResponseModel
{
    [Key("Command")] public required string Command { get; set; } = "cd";
    [Key("Output")] public required string Output { get; set; }
}