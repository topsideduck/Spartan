using MessagePack;

namespace Spartan.Models.RequestModels.Filesystem;

[MessagePackObject]
public class MkdirCommandRequestModel : ICommandRequestModel
{
    [Key("DirectoryPath")] public required string DirectoryPath { get; set; }
    [Key("Command")] public string Command { get; set; } = "mkdir";
}