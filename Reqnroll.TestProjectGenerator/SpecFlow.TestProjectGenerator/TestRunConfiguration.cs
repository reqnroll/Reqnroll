using TechTalk.SpecFlow.TestProjectGenerator.Data;

namespace TechTalk.SpecFlow.TestProjectGenerator
{
    public class TestRunConfiguration
    {
        public ProgrammingLanguage ProgrammingLanguage { get; set; }
        public ProjectFormat ProjectFormat { get; set; }

        public TargetFramework TargetFramework { get; set; }

        public UnitTestProvider UnitTestProvider { get; set; }
        public string SpecFlowVersion { get; set; }
        public string SpecFlowNuGetVersion { get; set; }
        public ConfigurationFormat ConfigurationFormat { get; set; }

        public string SpecFlowAllowedNuGetVersion { get; set; }
    }
}