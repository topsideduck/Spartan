using MessagePack;

namespace Spartan.Models.ResponseModels.Echo;

[MessagePackObject]
public class EchoCommandResponseModel : ICommandResponseModel
{
    [Key("Plugin")] public required string Plugin { get; set; } = "echo";
    [Key("Command")] public required string Command { get; set; } = "echo";
    [Key("Output")] public required string Output { get; set; }
}