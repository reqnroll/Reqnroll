using System.Reflection;

namespace Reqnroll.Plugins;

/// <summary>
/// This class is used for .NET Core based frameworks (.NET 6+) only. For .NET Framework <see cref="DotNetFrameworkPluginAssemblyLoader"/> is used instead. See <see cref="PlatformCompatibility.PlatformHelper"/>.
/// </summary>
public class DotNetCorePluginAssemblyLoader : IPluginAssemblyLoader
{
    public Assembly LoadAssembly(string assemblyPath) => DotNetCorePluginAssemblyResolver.Load(assemblyPath);
}
