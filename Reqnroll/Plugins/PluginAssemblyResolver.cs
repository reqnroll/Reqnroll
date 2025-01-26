using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Runtime.Loader;
using Microsoft.Extensions.DependencyModel;
using Microsoft.Extensions.DependencyModel.Resolution;

namespace Reqnroll.Plugins
{
    /// <summary>
    /// This class is used for .NET Core based frameworks (.NET 6+) only. See <see cref="PlatformCompatibility.PlatformHelper"/>.
    /// </summary>
    public sealed class PluginAssemblyResolver
    {
        private readonly ICompilationAssemblyResolver _assemblyResolver;
        private readonly DependencyContext _dependencyContext;
        private readonly AssemblyLoadContext _loadContext;

        public Assembly Assembly { get; }

        public PluginAssemblyResolver(string path)
        {
            _loadContext = AssemblyLoadContext.GetLoadContext(typeof(PluginAssemblyResolver).Assembly);
            Assembly = _loadContext.LoadFromAssemblyPath(path);

            try
            {
                _dependencyContext = DependencyContext.Load(Assembly);

                if (_dependencyContext is null)
                    return;

                _assemblyResolver = new CompositeCompilationAssemblyResolver(
                [
                    new AppBaseCompilationAssemblyResolver(Path.GetDirectoryName(path)!),
                    new ReferenceAssemblyPathResolver(),
                    new PackageCompilationAssemblyResolver()
                ]);

                _loadContext.Resolving += OnResolving;
                _loadContext.Unloading += OnUnloading;
            }
            catch (Exception)
            {
                // Don't throw if we can't load the dependencies from .deps.json
            }
        }

        private void OnUnloading(AssemblyLoadContext context)
        {
            _loadContext.Resolving -= OnResolving;
            _loadContext.Unloading -= OnUnloading;
        }

        private Assembly OnResolving(AssemblyLoadContext context, AssemblyName name)
        {
            try
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

                if (_assemblyResolver.TryResolveAssemblyPaths(wrapper, assemblies))
                {
                    foreach (var asm in assemblies)
                    {
                        try
                        {
                            var assembly = _loadContext.LoadFromAssemblyPath(asm);
                            return assembly;
                        }
                        catch
                        {
                            // Don't throw if we can't load the specified assembly (perhaps something is missing or misconfigured)
                            continue;
                        }
                    }
                }

                return null;
            }
            catch
            {
                // Don't throw if we can't load the dependencies from .deps.json
                return null;
            }
        }

        public static Assembly Load(string path)
        {
            var absolutePath = Path.GetFullPath(path);
            return new PluginAssemblyResolver(absolutePath).Assembly;
        }
    }
}
