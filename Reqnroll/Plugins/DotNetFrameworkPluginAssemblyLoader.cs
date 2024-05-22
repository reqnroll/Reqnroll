using System.Reflection;

namespace Reqnroll.Plugins;

public class DotNetFrameworkPluginAssemblyLoader : IPluginAssemblyLoader
{
    public Assembly LoadAssembly(string assemblyName) => Assembly.LoadFrom(assemblyName);
}
