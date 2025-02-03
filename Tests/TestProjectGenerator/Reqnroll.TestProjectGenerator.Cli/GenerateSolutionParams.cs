using System.IO;
using Reqnroll.TestProjectGenerator.Data;

namespace Reqnroll.TestProjectGenerator.Cli
{
    public class GenerateSolutionParams
    {
        public DirectoryInfo OutDir { get; set; }

        public string SlnName { get; set; }

        public string ReqnrollNuGetVersion { get; set; }

        public UnitTestProvider UnitTestProvider { get; set; }

        public TargetFramework TargetFramework { get; set; }

        public string SdkVersion { get; set; }

        public ProjectFormat ProjectFormat { get; set; }

        public ConfigurationFormat ConfigurationFormat { get; set; }

        public int FeatureCount { get; set; }
    }
}
