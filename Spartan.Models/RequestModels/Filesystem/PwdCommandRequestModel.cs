using MessagePack;

namespace Spartan.Models.RequestModels.Filesystem;

[MessagePackObject]
public class PwdCommandRequestModel : ICommandRequestModel
{
    [Key("Command")] public string Command { get; set; } = "pwd";
}