using MessagePack;

namespace Spartan.Models.RequestModels.UI;

[MessagePackObject]
public class ScreenshotCommandRequestModel : ICommandRequestModel
{
    [Key("LocalDestinationPath")]
    public required string LocalDestinationDirectory { get; set; } = Environment.CurrentDirectory;

    [Key("Command")] public string Command { get; set; } = "screenshot";
}