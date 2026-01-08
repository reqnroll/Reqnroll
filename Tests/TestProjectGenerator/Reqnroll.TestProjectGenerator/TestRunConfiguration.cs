using Reqnroll.TestProjectGenerator.Data;

namespace Reqnroll.TestProjectGenerator
{
    public class TestRunConfiguration
    {
        public ProgrammingLanguage ProgrammingLanguage { get; set; }
        public ProjectFormat ProjectFormat { get; set; }

        public TargetFramework TargetFramework { get; set; }

        public UnitTestProvider UnitTestProvider { get; set; }
        public string ReqnrollVersion { get; set; }
        public string ReqnrollNuGetVersion { get; set; }
        public ConfigurationFormat ConfigurationFormat { get; set; }

        public string ReqnrollAllowedNuGetVersion { get; set; }

        /// <summary>
        /// Whether to add an explicit reference to `Reqnroll` package in addition to the unit test specific packages, like `Reqnroll.MsTest`.
        /// </summary>
        public bool ForceAddingExplicitReferenceToReqnrollPackage { get; set; } = false;
    }
}