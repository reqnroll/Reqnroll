using System.Reflection;

namespace Reqnroll.Plugins;

/// <summary>
/// This class is used for .NET Framework v4.* only. For .NET +6 <see cref="DotNetCorePluginAssemblyLoader"/> is used instead. See <see cref="PlatformCompatibility.PlatformHelper"/>.
/// </summary>
public class DotNetFrameworkPluginAssemblyLoader : IPluginAssemblyLoader
{
    public Assembly LoadAssembly(string assemblyPath) => DotNetFrameworkPluginAssemblyResolver.Load(assemblyPath);
}
