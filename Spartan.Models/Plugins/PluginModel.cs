using MessagePack;

namespace Spartan.Models.Plugins;

[MessagePackObject]
public class PluginModel
{
    [Key("PluginName")] public required string PluginName { get; set; }

    [Key("PluginDisplayName")] public required string PayloadDisplayName { get; set; }

    [Key("PluginEntryPoint")] public required string PluginEntryPoint { get; set; }

    [Key("AssemblyBinaries")] public required Dictionary<string, byte[]> AssemblyBinaries { get; set; }
}