using MessagePack;

namespace Spartan.Models.RequestModels.Echo;

[MessagePackObject]
public class EchoCommandRequestModel : ICommandRequestModel
{
    [Key("Message")] public required string Message { get; set; }
    [Key("Plugin")] public string Plugin { get; set; } = "echo";
    [Key("Command")] public string Command { get; set; } = "echo";
}