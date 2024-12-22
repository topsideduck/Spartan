using MessagePack;

namespace Spartan.Models.Payload;

[MessagePackObject]
public class PayloadModel
{
    [Key("PayloadName")] public required string PayloadName { get; set; }

    [Key("PayloadEntryPoint")] public required string PayloadEntryPoint { get; set; }

    [Key("AssemblyBinaries")] public required Dictionary<string, byte[]> AssemblyBinaries { get; set; }
}