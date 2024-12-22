using System.Reflection;
using System.Runtime.Loader;

namespace Spartan.Payload;

public class PluginManager
{
    private readonly Dictionary<string, AssemblyLoadContext> _loadedAssemblies = new();
    /*
     * Format of _loadedAssemblies:
     * {
     *    "PluginName": AssemblyLoadContext
     * }
     */

    public Assembly LoadPlugin(Dictionary<string, dynamic> pluginMetadata)
        /*
         * Format of pluginMetadata:
         * {
         *     "PluginDisplayName": "PluginDisplayName",
         *     "PluginName": "The main assembly (name) of the plugin",
         *     "PluginEntryPoint": "The class name from the main assembly",
         *     "AssemblyBinaries": {
         *         "AssemblyName": "Base64EncodedAssembly",
         *         "AssemblyName": "Base64EncodedAssembly",
         *      ...
         *      }
         * }
         *
         */
    {
        string pluginName = pluginMetadata["PluginName"];
        string pluginEntryPoint = pluginMetadata["PluginEntryPoint"];
        Dictionary<string, string> assemblyBinaries = pluginMetadata["AssemblyBinaries"];

        var assemblyLoadContext = new AssemblyLoadContext(pluginName);

        using var mainAssemblyStream = new MemoryStream(Convert.FromBase64String(assemblyBinaries[pluginEntryPoint]));
        var mainAssembly = assemblyLoadContext.LoadFromStream(mainAssemblyStream);

        foreach (var (assemblyName, assemblyBinary) in assemblyBinaries)
        {
            if (assemblyName == pluginEntryPoint) continue;

            using var assemblyStream = new MemoryStream(Convert.FromBase64String(assemblyBinary));
            assemblyLoadContext.LoadFromStream(assemblyStream);
        }

        _loadedAssemblies.Add(pluginName, assemblyLoadContext);

        return mainAssembly;
    }

    public void UnloadPlugin(string pluginName)
    {
        _loadedAssemblies[pluginName].Unload();
        _loadedAssemblies.Remove(pluginName);
    }
}