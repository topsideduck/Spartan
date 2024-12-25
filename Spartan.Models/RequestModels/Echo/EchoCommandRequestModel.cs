using MessagePack;

namespace Spartan.Models.RequestModels.Echo;

[MessagePackObject]
public class EchoCommandRequestModel : ICommandRequestModel
{
    [Key("Message")] public required string Message { get; set; }
    [Key("Command")] public string Command { get; set; } = "echo";
}