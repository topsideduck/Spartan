using System.Diagnostics.CodeAnalysis;
using System.Reflection;

namespace Spartan.Utils;

public static class AssemblyResolver
{
    [RequiresUnreferencedCode("Calls System.Reflection.Assembly.GetReferencedAssemblies()")]
    public static Dictionary<string, byte[]> ReadAssemblyWithDependencies(string assemblyName,
        string dependencyDirectory)
    {
        // Dictionary to store dependencies as byte arrays
        var assemblies = new Dictionary<string, byte[]>();

        // Read the main assembly into a byte array
        var mainAssemblyPath = Path.Combine(dependencyDirectory, $"{assemblyName}.dll");
        var mainAssemblyBytes = File.ReadAllBytes(mainAssemblyPath);
        assemblies[assemblyName] = mainAssemblyBytes;

        // Load the main assembly into a reflection-only context
        var mainAssembly = Assembly.Load(mainAssemblyBytes);

        // Get dependencies of the main assembly
        foreach (var reference in mainAssembly.GetReferencedAssemblies())
        {
            var dependencyPath = Path.Combine(dependencyDirectory, $"{reference.Name}.dll");
            if (File.Exists(dependencyPath))
            {
                var dependencyBytes = File.ReadAllBytes(dependencyPath);
                assemblies[reference.Name] = dependencyBytes;
            }
            else
            {
                Console.WriteLine($"Dependency not found: {reference.FullName}");
            }
        }

        return assemblies;
    }
}