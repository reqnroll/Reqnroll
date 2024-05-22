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

        public NuGetPackage(string name, string version, string allowedVersions, params NuGetPackageAssembly[] assemblies)
        {
            Name = name;
            Version = version;
            AllowedVersions = allowedVersions;
            Assemblies = new ReadOnlyCollection<NuGetPackageAssembly>(assemblies.Where(a => a != null).ToList());
        }

        public string Name { get; }
        public string Version { get; }
        public string AllowedVersions { get; }
        public IReadOnlyList<NuGetPackageAssembly> Assemblies { get; }
    }
}