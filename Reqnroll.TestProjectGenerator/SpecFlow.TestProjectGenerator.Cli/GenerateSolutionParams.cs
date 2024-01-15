using System;
using System.IO;
using TechTalk.SpecFlow.TestProjectGenerator;
using TechTalk.SpecFlow.TestProjectGenerator.Data;

namespace SpecFlow.TestProjectGenerator.Cli
{
    public class GenerateSolutionParams
    {
        public DirectoryInfo OutDir { get; set; }

        public string SlnName { get; set; }

        public string SpecFlowNuGetVersion { get; set; }

        public UnitTestProvider UnitTestProvider { get; set; }

        public TargetFramework TargetFramework { get; set; }

        public string SdkVersion { get; set; }

        public ProjectFormat ProjectFormat { get; set; }

        public ConfigurationFormat ConfigurationFormat { get; set; }

        public string SpecrunNuGetVersion { get; set; }

        public int FeatureCount { get; set; }
    }
}
