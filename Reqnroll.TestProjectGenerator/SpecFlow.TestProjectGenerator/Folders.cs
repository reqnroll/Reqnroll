using System;
using System.IO;
using System.Reflection;
using TechTalk.SpecFlow.TestProjectGenerator.Helpers;

namespace TechTalk.SpecFlow.TestProjectGenerator
{
    public class Folders
    {
        private readonly AppConfigDriver _appConfigDriver;
        protected string _nugetFolder;
        protected string _packageFolder;
        protected string _sourceRoot;
        protected string _specFlow;
        protected string _vsAdapterFolder;

        protected bool _vsAdapterFolderChanged;
        private string _externalNuGetFolder;

        public string TestFolder => Path.GetDirectoryName(new Uri(Assembly.GetExecutingAssembly().Location).LocalPath);

        public Folders(AppConfigDriver appConfigDriver)
        {
            _appConfigDriver = appConfigDriver;
        }

        public virtual string SourceRoot
        {
            get => !string.IsNullOrWhiteSpace(_sourceRoot) ? _sourceRoot : Path.GetFullPath(Path.Combine(TestFolder, "..", "..", "..", "..", ".."));
            set => _sourceRoot = value;
        }

        public string VSAdapterFolder
        {
            get
            {
                return !string.IsNullOrWhiteSpace(_vsAdapterFolder) ? _vsAdapterFolder : VsAdapterFolderProjectBinaries;
            }
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
            => _externalNuGetFolder = _externalNuGetFolder ?? Path.GetFullPath(Path.Combine(SourceRoot, "NuGet", "feed"));

        public string PackageFolder
        {
            get => !string.IsNullOrWhiteSpace(_packageFolder) ? _packageFolder : Path.GetFullPath(Path.Combine(SourceRoot, "packages"));
            set => _packageFolder = value;
        }

        public string GlobalPackages => Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.UserProfile), ".nuget", "packages");

        public string SpecFlow
        {
            get => !string.IsNullOrWhiteSpace(_specFlow) ? _specFlow : Path.GetFullPath(TestFolder);
            set => _specFlow = value;
        }

        public virtual string FolderToSaveGeneratedSolutions => Path.Combine(Path.GetTempPath(), _appConfigDriver.TestProjectFolderName);
    }
}