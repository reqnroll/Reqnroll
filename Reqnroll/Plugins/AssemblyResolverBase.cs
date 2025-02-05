using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using Microsoft.Extensions.DependencyModel;
using Microsoft.Extensions.DependencyModel.Resolution;

namespace Reqnroll.Plugins;

public abstract class AssemblyResolverBase
{
    private readonly Lazy<Assembly> _assembly;

    public Assembly GetAssembly() => _assembly.Value;

    private ICompilationAssemblyResolver _assemblyResolver;
    private DependencyContext _dependencyContext;

    protected AssemblyResolverBase(string relativePath)
    {
        var path = Path.GetFullPath(relativePath);
        _assembly = new Lazy<Assembly>(() => Initialize(path));
    }

    protected abstract Assembly Initialize(string path);

    protected void SetupDependencyContext(string path, Assembly assembly, bool throwOnError)
    {
        try
        {
            _dependencyContext = DependencyContext.Load(assembly);

            if (_dependencyContext is null) return;

            _assemblyResolver = new CompositeCompilationAssemblyResolver(
            [
                new AppBaseCompilationAssemblyResolver(Path.GetDirectoryName(path)!),
                new ReferenceAssemblyPathResolver(),
                new PackageCompilationAssemblyResolver()
            ]);
        }
        catch (Exception)
        {
            if (throwOnError)
                throw;

            // We ignore if there was a problem with initializing context from .deps.json
        }
    }

    protected abstract Assembly LoadAssemblyFromPath(string assemblyPath);

    protected Assembly TryResolveAssembly(AssemblyName name)
    {
        var library = _dependencyContext?.RuntimeLibraries.FirstOrDefault(
            runtimeLibrary => string.Equals(runtimeLibrary.Name, name.Name, StringComparison.OrdinalIgnoreCase));

        if (library == null)
            return null;

        var wrapper = new CompilationLibrary(
            library.Type,
            library.Name,
            library.Version,
            library.Hash,
            library.RuntimeAssemblyGroups.SelectMany(g => g.AssetPaths),
            library.Dependencies,
            library.Serviceable,
            library.Path,
            library.HashPath);

        var assemblies = new List<string>();
        _assemblyResolver.TryResolveAssemblyPaths(wrapper, assemblies);

        if (_assemblyResolver.TryResolveAssemblyPaths(wrapper, assemblies) && assemblies.Count > 0)
        {
            foreach (var assemblyPath in assemblies)
            {
                try
                {
                    return LoadAssemblyFromPath(assemblyPath);
                }
                catch
                {
                    // Don't throw if we can't load the specified assembly (perhaps something is missing or misconfigured)
                }
            }
        }

        return null;
    }
}
