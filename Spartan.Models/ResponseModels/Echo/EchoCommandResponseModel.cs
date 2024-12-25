using MessagePack;

namespace Spartan.Models.ResponseModels.Echo;

[MessagePackObject]
public class EchoCommandResponseModel : ICommandResponseModel
{
    [Key("Command")] public required string Command { get; set; } = "echo";
    [Key("Output")] public required string Output { get; set; }
}