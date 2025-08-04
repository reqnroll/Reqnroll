using System.Reflection;

namespace Reqnroll.Plugins;

/// <summary>
/// This class is used for .NET 5+ based frameworks only. For .NET Framework <see cref="DotNetFrameworkPluginAssemblyLoader"/> is used instead. See <see cref="PlatformCompatibility.PlatformHelper"/>.
/// </summary>
public class DotNetCorePluginAssemblyLoader : IPluginAssemblyLoader
{
    public Assembly LoadAssembly(string assemblyPath) => DotNetCorePluginAssemblyResolver.Load(assemblyPath);
}
