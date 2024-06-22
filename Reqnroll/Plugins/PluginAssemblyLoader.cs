using System.Reflection;

namespace Reqnroll.Plugins;

public class PluginAssemblyLoader : IPluginAssemblyLoader
{
    public Assembly LoadAssembly(string assemblyName) => PluginAssemblyResolver.Load(assemblyName);
}
