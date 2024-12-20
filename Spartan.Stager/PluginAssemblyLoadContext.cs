using System.Diagnostics.CodeAnalysis;
using System.Reflection;
using System.Runtime.Loader;

namespace Spartan.Stager;

public class PluginAssemblyLoadContext : AssemblyLoadContext
{
    public Dictionary<string, Assembly> LoadedAssembliesDictionary { get; } = new();

    [RequiresUnreferencedCode("Calls System.Reflection.Assembly.LoadFromStream(Stream)")]
    public void RegisterAssemblyFromBytes(string assemblyName, byte[] assemblyBytes)
    {
        if (LoadedAssembliesDictionary.ContainsKey(assemblyName))
        {
            return;
        }

        var assembly = LoadFromStream(new MemoryStream(assemblyBytes));
        LoadedAssembliesDictionary[assemblyName] = assembly;
    }

    public Assembly LoadAssembly(string name)
    {
        if (LoadedAssembliesDictionary.TryGetValue(name, out var assembly))
        {
            return assembly;
        }

        throw new Exception($"Assembly {name} not registered.");
    }
}