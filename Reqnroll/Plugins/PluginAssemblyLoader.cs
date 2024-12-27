using System.Reflection;

namespace Reqnroll.Plugins;

/// <summary>
/// This class can only be used for .NET 6+ because it relies on runtime specifics.
/// </summary>
public class PluginAssemblyLoader : IPluginAssemblyLoader
{
    public Assembly LoadAssembly(string assemblyName) => PluginAssemblyResolver.Load(assemblyName);
}
