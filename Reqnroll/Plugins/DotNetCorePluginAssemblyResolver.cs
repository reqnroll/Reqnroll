using System.Reflection;
using System.Runtime.Loader;

namespace Reqnroll.Plugins;

/// <summary>
/// This class is used for .NET (not .NET Framework) based frameworks only. See <see cref="PlatformCompatibility.PlatformHelper"/>.
/// </summary>
public sealed class DotNetCorePluginAssemblyResolver(string path) : AssemblyResolverBase(path)
{
    private AssemblyLoadContext _loadContext;

    protected override Assembly Initialize(string path)
    {
        _loadContext = AssemblyLoadContext.GetLoadContext(typeof(DotNetCorePluginAssemblyResolver).Assembly);
        var assembly = LoadAssemblyFromPath(path);

        SetupDependencyContext(path, assembly, true);

        _loadContext.Resolving += OnResolving;
        _loadContext.Unloading += OnUnloading;

        return assembly;
    }

    protected override Assembly LoadAssemblyFromPath(string assemblyPath)
        => _loadContext.LoadFromAssemblyPath(assemblyPath);

    private void OnUnloading(AssemblyLoadContext context)
    {
        _loadContext.Resolving -= OnResolving;
        _loadContext.Unloading -= OnUnloading;
    }

    private Assembly OnResolving(AssemblyLoadContext context, AssemblyName name)
    {
        return TryResolveAssembly(name);
    }

    public static Assembly Load(string path)
    {
        return new DotNetCorePluginAssemblyResolver(path).GetAssembly();
    }
}