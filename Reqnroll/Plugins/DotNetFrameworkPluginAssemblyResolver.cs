using System;
using System.Reflection;

namespace Reqnroll.Plugins;

/// <summary>
/// This class is used for .NET Framework 4.* only. See <see cref="PlatformCompatibility.PlatformHelper"/>.
/// </summary>
public sealed class DotNetFrameworkPluginAssemblyResolver(string path) : AssemblyResolverBase(path)
{
    protected override Assembly Initialize(string path)
    {
        var assembly = LoadAssemblyFromPath(path);

        SetupDependencyContext(path, assembly, false);
        AppDomain.CurrentDomain.AssemblyResolve += OnAssemblyResolve;

        return assembly;
    }

    protected override Assembly LoadAssemblyFromPath(string assemblyPath) 
        => Assembly.LoadFrom(assemblyPath);

    private Assembly OnAssemblyResolve(object sender, ResolveEventArgs args)
    {
        var assemblyName = new AssemblyName(args.Name);
        return TryResolveAssembly(assemblyName);
    }

    public static Assembly Load(string path)
    {
        return new DotNetFrameworkPluginAssemblyResolver(path).GetAssembly();
    }
}