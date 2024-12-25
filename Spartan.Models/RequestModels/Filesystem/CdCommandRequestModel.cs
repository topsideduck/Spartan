using MessagePack;

namespace Spartan.Models.RequestModels.Filesystem;

[MessagePackObject]
public class CdCommandRequestModel : ICommandRequestModel
{
    [Key("Path")] public required string Path { get; set; }
    [Key("Command")] public string Command { get; set; } = "cd";
}