using MessagePack;

namespace Spartan.Models.Plugins;

[MessagePackObject]
public class PluginAssemblyModel
{
    [Key("PluginName")] public required string PluginName { get; set; }
    [Key("PluginEntryPointClass")] public required string PluginEntryPointClass { get; set; }
    [Key("PluginEntryPointMethod")] public required string PluginEntryPointMethod { get; set; }
    [Key("AssemblyBinaries")] public required Dictionary<string, byte[]> AssemblyBinaries { get; set; }
}