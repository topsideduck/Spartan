using System.Reflection;
using System.Runtime.Loader;
using Spartan.Models.Plugins;

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

    public (Assembly, string) LoadPlugin(PluginAssemblyModel pluginAssemlyMetadata)
        /*
         * Format of pluginMetadata:
         * {
         *     "PluginName": "The main assembly (name) of the plugin",
         *     "PluginEntryPointClass": "The class name from the main assembly",
         *     "PluginEntryPointMethod": "The method name from the main assembly",
         *     "AssemblyBinaries": {
         *         "AssemblyName": "Base64EncodedAssembly",
         *         "AssemblyName": "Base64EncodedAssembly",
         *      ...
         *      }
         * }
         *
         */
    {
        var pluginName = pluginAssemlyMetadata.PluginName;
        var pluginEntryPointClass = pluginAssemlyMetadata.PluginEntryPointClass;
        var pluginEntryPointMethod = pluginAssemlyMetadata.PluginEntryPointMethod;
        var assemblyBinaries = pluginAssemlyMetadata.AssemblyBinaries;

        var assemblyLoadContext = new AssemblyLoadContext(pluginName);

        using var mainAssemblyStream = new MemoryStream(assemblyBinaries[pluginEntryPointClass]);
        var mainAssembly = assemblyLoadContext.LoadFromStream(mainAssemblyStream);

        foreach (var (assemblyName, assemblyBinary) in assemblyBinaries)
        {
            if (assemblyName == pluginEntryPointClass) continue;

            using var assemblyStream = new MemoryStream(assemblyBinary);
            assemblyLoadContext.LoadFromStream(assemblyStream);
        }

        _loadedAssemblies.Add(pluginName, assemblyLoadContext);

        return (mainAssembly, pluginEntryPointMethod);
    }

    public void UnloadPlugin(string pluginName)
    {
        _loadedAssemblies[pluginName].Unload();
        _loadedAssemblies.Remove(pluginName);
    }
}