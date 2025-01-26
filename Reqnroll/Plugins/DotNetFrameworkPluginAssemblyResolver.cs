using Microsoft.Extensions.DependencyModel;
using Microsoft.Extensions.DependencyModel.Resolution;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;

namespace Reqnroll.Plugins
{
    public sealed class DotNetFrameworkPluginAssemblyResolver
    {
        public Assembly Assembly { get; }

        DependencyContext resolverRependencyContext;
        CompositeCompilationAssemblyResolver assemblyResolver;

        public DotNetFrameworkPluginAssemblyResolver(string path)
        {
            var absolutePath = Path.GetFullPath(path);
            Assembly = Assembly.LoadFrom(absolutePath);

            try
            {
                SetupDependencyContext(path);
            }
            catch (Exception)
            {
                // Don't throw if we can't load the dependencies from .deps.json
            }
        }
        void SetupDependencyContext(string path)
        {
            resolverRependencyContext = DependencyContext.Load(Assembly);

            if (resolverRependencyContext is null)
                return;

            assemblyResolver = new CompositeCompilationAssemblyResolver(
            [
                new AppBaseCompilationAssemblyResolver(Path.GetDirectoryName(path)!),
                new ReferenceAssemblyPathResolver(),
                new PackageCompilationAssemblyResolver()
            ]);

            AppDomain.CurrentDomain.AssemblyResolve += TryAssemblyResolve;
        }

        Assembly TryAssemblyResolve(object sender, ResolveEventArgs args)
        {
            try
            {
                var assemblyName = new AssemblyName(args.Name);
                var library = resolverRependencyContext.RuntimeLibraries.FirstOrDefault(runtimeLibrary => string.Equals(runtimeLibrary.Name, assemblyName.Name, StringComparison.OrdinalIgnoreCase));
                if (library is null)
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
                if (assemblyResolver.TryResolveAssemblyPaths(wrapper, assemblies))
                {
                    foreach (var asm in assemblies)
                    {
                        try
                        {
                            var assembly = Assembly.LoadFrom(asm);
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
            return new DotNetFrameworkPluginAssemblyResolver(absolutePath).Assembly;
        }
    }
}
