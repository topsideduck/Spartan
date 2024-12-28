using MessagePack;

namespace Spartan.Models.ResponseModels.UI;

[MessagePackObject]
public class ScreenshotCommandResponseModel : ICommandResponseModel
{
    [Key("LocalDestinationPath")] public required string LocalDestinationPath { get; set; }
    [Key("FileContents")] public required List<byte[]> FileContents { get; set; }
    [Key("Command")] public required string Command { get; set; } = "screenshot";
    [Key("Output")] public required string Output { get; set; }
}