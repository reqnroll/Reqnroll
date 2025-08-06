using System.Reflection;

namespace Reqnroll.Plugins;

/// <summary>
/// This class is used for .NET only (not .NET Framework)
/// </summary>
public class DotNetCorePluginAssemblyLoader : IPluginAssemblyLoader
{
    public Assembly LoadAssembly(string assemblyPath) => DotNetCorePluginAssemblyResolver.Load(assemblyPath);
}
