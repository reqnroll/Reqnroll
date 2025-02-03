using System.IO;
using Reqnroll.TestProjectGenerator.Conventions;

namespace Reqnroll.TestProjectGenerator.Cli
{
    public class FoldersOverride : Folders
    {
        private readonly SolutionConfiguration _solutionConfiguration;
        public FoldersOverride(SolutionConfiguration solutionConfiguration, ArtifactNamingConvention artifactNamingConvention) : base(new ConfigurationDriver(), artifactNamingConvention)
        {
            _solutionConfiguration = solutionConfiguration;
        }

        public override string FolderToSaveGeneratedSolutions => Path.GetFullPath(_solutionConfiguration.OutDir?.FullName ?? Directory.GetCurrentDirectory());
    }
}
