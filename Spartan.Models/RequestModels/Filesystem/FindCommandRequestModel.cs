using MessagePack;

namespace Spartan.Models.RequestModels.Filesystem;

[MessagePackObject]
public class FindCommandRequestModel : ICommandRequestModel
{
    [Key("DirectoryPath")] public required string DirectoryPath { get; set; }
    [Key("SearchPattern")] public required string SearchPattern { get; set; }
    [Key("Command")] public string Command { get; set; } = "find";
}