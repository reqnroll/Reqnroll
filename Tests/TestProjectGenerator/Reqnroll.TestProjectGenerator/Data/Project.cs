using System;
using System.Collections.Generic;
using System.Linq;

namespace Reqnroll.TestProjectGenerator.Data
{
    public class Project
    {
        private readonly List<NuGetPackage> _nuGetPackages = new List<NuGetPackage>();

        private readonly List<ProjectReference> _projectReferences = new List<ProjectReference>();

        private readonly List<Reference> _references = new List<Reference>();

        private readonly List<ProjectFile> _files = new List<ProjectFile>();

        private readonly List<MSBuildTarget> _msBuildTargets = new List<MSBuildTarget>();
        private readonly List<MSBuildImport> _msbuildImports = new List<MSBuildImport>();

        public Project(string name, Guid projectGuid, ProgrammingLanguage programmingLanguage, TargetFramework targetFrameworks, ProjectFormat projectFormat, ProjectType projectType = ProjectType.Library, bool? isTreatWarningsAsErrors = null)
        {
            ProgrammingLanguage = programmingLanguage;
            ProjectFormat = projectFormat;
            ProjectType = projectType;
            IsTreatWarningsAsErrors = isTreatWarningsAsErrors;
            Name = name;
            ProjectGuid = projectGuid;
            TargetFrameworks = targetFrameworks;
            
            if (projectFormat == ProjectFormat.Old)
            {
                AddReference("System");
                AddReference("System.Configuration");
                AddReference("System.Core");
                AddReference("System.Data");
                AddReference("System.Xml");
                AddReference("System.Xml.Linq");
                AddReference("Microsoft.CSharp");
            }
        }
        
        public string Name { get; }
        public Guid ProjectGuid { get; }
        public TargetFramework TargetFrameworks { get; } //net45, net471, net8.0
        public ProjectType ProjectType { get; }
        public bool? IsTreatWarningsAsErrors { get; }

        public ProgrammingLanguage ProgrammingLanguage { get; }
        public ProjectFormat ProjectFormat { get; }

        public IReadOnlyList<NuGetPackage> NuGetPackages => _nuGetPackages;
        public IReadOnlyList<ProjectReference> ProjectReferences => _projectReferences;
        public IReadOnlyList<Reference> References => _references;
        public IReadOnlyList<ProjectFile> Files => _files;

        public IReadOnlyList<MSBuildTarget> MSBuildTargets => _msBuildTargets;
        public IReadOnlyList<MSBuildImport> MSBuildImports => _msbuildImports;

        public void AddNuGetPackage(string name, string version = null)
        {
            if (_nuGetPackages.All(n => n.Name != name))
            {
                _nuGetPackages.Add(new NuGetPackage(name, version, KnownAssemblyNames.Get(name, version)));
            }
        }

        public void AddNuGetPackage(string name, string version, string allowedVersions)
        {
            if (_nuGetPackages.All(n => n.Name != name))
            {
                _nuGetPackages.Add(new NuGetPackage(name, version, allowedVersions, KnownAssemblyNames.Get(name, version)));
            }
        }

        public void AddNuGetPackage(string name, string version, params NuGetPackageAssembly[] assemblies)
        {
            if (_nuGetPackages.All(n => n.Name != name))
            {
                _nuGetPackages.Add(new NuGetPackage(name, version, assemblies));
            }
        }

        public void AddNuGetPackage(string name, string version, string allowedVersions, params NuGetPackageAssembly[] assemblies)
        {
            if (_nuGetPackages.All(n => n.Name != name))
            {
                _nuGetPackages.Add(new NuGetPackage(name, version, allowedVersions, assemblies));
            }
        }

        public void RemoveNuGetPackage(string packageName)
        {
            _nuGetPackages.RemoveAll(p => p.Name == packageName);
        }

        public void AddReference(string name)
        {
            _references.Add(new Reference(name));
        }

        public void AddFile(ProjectFile projectFile)
        {
            _files.Add(projectFile ?? throw new ArgumentNullException(nameof(projectFile)));
        }

        public void AddProjectReference(string fullPath, ProjectBuilder projectToReference)
        {
            _projectReferences.Add(new ProjectReference(fullPath, projectToReference));
        }

        public void AddTarget(string targetName, string implementation)
        {
            _msBuildTargets.Add(new MSBuildTarget(targetName, implementation));
        }

        public void AddMSBuildImport(string msbuildTargetFile)
        {
            _msbuildImports.Add(new MSBuildImport(msbuildTargetFile));
        }
    }
}
