using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;

namespace Reqnroll.TestProjectGenerator.Data
{
    public class NuGetPackage
    {
        public NuGetPackage(string name, string version, params NuGetPackageAssembly[] assemblies)
        {
            Name = name;
            Version = version;
            Assemblies = new ReadOnlyCollection<NuGetPackageAssembly>(assemblies.Where(a => a != null).ToList());
        }

        public NuGetPackage(
            string name,
            string version,
            string allowedVersions = null,
            bool isDevelopmentDependency = false,
            params NuGetPackageAssembly[] assemblies)
        {
            Name = name;
            Version = version;
            AllowedVersions = allowedVersions;
            IsDevelopmentDependency = isDevelopmentDependency;
            Assemblies = new ReadOnlyCollection<NuGetPackageAssembly>(assemblies.Where(a => a != null).ToList());
        }

        public string Name { get; }
        public string Version { get; }
        public string AllowedVersions { get; }
        public bool IsDevelopmentDependency { get; }
        public IReadOnlyList<NuGetPackageAssembly> Assemblies { get; }
    }

    public class PackageReference(string include, string version, string privateAssets = null, string includeAssets = null)
    {
        public string Include { get; } = include;
        public string Version { get; } = version;
        public string PrivateAssets { get; } = privateAssets;
        public string IncludeAssets { get; } = includeAssets;
    }
}