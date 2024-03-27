using System;
using System.IO;
using System.Reflection;
using Reqnroll.TestProjectGenerator.Conventions;
using Reqnroll.TestProjectGenerator.Helpers;

namespace Reqnroll.TestProjectGenerator
{
    public class Folders
    {
        private readonly AppConfigDriver _appConfigDriver;
        private readonly ArtifactNamingConvention _artifactNamingConvention;
        private static readonly Guid UniqueRunId = Guid.NewGuid();
        protected string _nugetFolder;
        protected string _packageFolder;
        protected string _sourceRoot;
        protected string _reqnroll;
        protected string _vsAdapterFolder;

        protected bool _vsAdapterFolderChanged;
        private string _externalNuGetFolder;

        public string TestFolder => Path.GetDirectoryName(new Uri(Assembly.GetExecutingAssembly().Location).LocalPath);

        public Folders(AppConfigDriver appConfigDriver, ArtifactNamingConvention artifactNamingConvention)
        {
            _appConfigDriver = appConfigDriver;
            _artifactNamingConvention = artifactNamingConvention;
        }

        public virtual string SourceRoot
        {
            get => !string.IsNullOrWhiteSpace(_sourceRoot) ? _sourceRoot : Path.GetFullPath(Path.Combine(TestFolder, "..", "..", "..", "..", ".."));
            set => _sourceRoot = value;
        }

        public string VSAdapterFolder
        {
            get => !string.IsNullOrWhiteSpace(_vsAdapterFolder) ? _vsAdapterFolder : VsAdapterFolderProjectBinaries;
            set
            {
                _vsAdapterFolder = value;
                _vsAdapterFolderChanged = true;
            }
        }

        public virtual string VsAdapterFolderProjectBinaries => throw new NotImplementedException();

        public bool VsAdapterFolderChanged => _vsAdapterFolderChanged;


        public virtual string NuGetFolder
        {
            get => !_nugetFolder.IsNullOrWhiteSpace() ? _nugetFolder :
#if DEBUG
                Path.GetFullPath(Path.Combine(SourceRoot, "GeneratedNuGetPackages", "Debug"));
#else
                Path.GetFullPath(Path.Combine(SourceRoot, "GeneratedNuGetPackages", "Release"));
#endif
            set => _nugetFolder = value;
        }

        public virtual string ExternalNuGetFolder
            => _externalNuGetFolder ??= Path.GetFullPath(Path.Combine(SourceRoot, "NuGet", "feed"));

        public string PackageFolder
        {
            get => !string.IsNullOrWhiteSpace(_packageFolder) ? _packageFolder : Path.GetFullPath(Path.Combine(SourceRoot, "packages"));
            set => _packageFolder = value;
        }

        public string GlobalPackages => Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.UserProfile), ".nuget", "packages");

        public string Reqnroll
        {
            get => !string.IsNullOrWhiteSpace(_reqnroll) ? _reqnroll : Path.GetFullPath(TestFolder);
            set => _reqnroll = value;
        }

        public virtual string FolderToSaveGeneratedSolutions => Path.Combine(Path.GetTempPath(), _appConfigDriver.TestProjectFolderName);

        public virtual string RunUniqueFolderToSaveGeneratedSolutions => Path.Combine(FolderToSaveGeneratedSolutions, _artifactNamingConvention.GetRunName(UniqueRunId));
        public virtual string RunUniqueGlobalPackages => _appConfigDriver.PipelineMode ? GlobalPackages : Path.Combine(RunUniqueFolderToSaveGeneratedSolutions, ".nuget");
    }
}