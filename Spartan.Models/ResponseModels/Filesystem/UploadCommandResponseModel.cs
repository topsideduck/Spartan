using MessagePack;

namespace Spartan.Models.ResponseModels.Filesystem;

[MessagePackObject]
public class UploadCommandResponseModel : ICommandResponseModel
{
    [Key("Command")] public required string Command { get; set; } = "upload";
    [Key("Output")] public required string Output { get; set; }
}