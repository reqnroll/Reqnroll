
using Cucumber.Messages;
using Io.Cucumber.Messages.Types;
using Microsoft.VisualStudio.TestPlatform.PlatformAbstractions;
using System.Reflection;

namespace CucumberMessages.CompatibilityTests
{
    [TestClass]
    public class CucumberCompatibilityTests : CucumberCompatibilityTestBase
    {
        [TestMethod]
        public void SmokeTest()
        {
            AddCucumberMessagePlugIn();
            CucumberMessagesAddConfigurationFile("CucumberMessages.configuration.json");

            AddFeatureFile("""
                Feature: Cucumber Messages Smoke Test
                  Scenario: Log JSON
                     When the following string is attached as "application/json":
                       ```
                       {"message": "The <b>big</b> question", "foo": "bar"}
                       ```
                """);

            AddPassingStepBinding("When");

            ExecuteTests();

            ShouldAllScenariosPass();
        }

        [TestMethod]
        [DataRow("attachments")]
        [DataRow("minimal")]
        [DataRow("cdata")]
        [DataRow("pending")]
        [DataRow("examples-tables")]
        [DataRow("hooks")]
        [DataRow("data-tables")]
        [DataRow("parameter-types")]
        [DataRow("skipped")]
        [DataRow("undefined")]
        [DataRow("unknown-parameter-type")]
        [DataRow("rules")]
        public void CCKScenarios(string scenarioName)
        {
            AddCucumberMessagePlugIn();
            CucumberMessagesAddConfigurationFile("CucumberMessages.configuration.json");
            AddUtilClassWithFileSystemPath();

            scenarioName = scenarioName.Replace("-", "_");

            AddFeatureFileFromResource($"{scenarioName}/{scenarioName}.feature", "CucumberMessages.CompatibilityTests.CCK", Assembly.GetExecutingAssembly());
            AddBindingClassFromResource($"{scenarioName}/{scenarioName}.cs", "CucumberMessages.CompatibilityTests.CCK", Assembly.GetExecutingAssembly());
            AddBinaryFilesFromResource($"{scenarioName}", "CucumberMessages.CompatibilityTests.CCK", Assembly.GetExecutingAssembly());

            ExecuteTests();

            ConfirmAllTestsRan(null);
        }

        private void AddUtilClassWithFileSystemPath()
        {
            string location = AppContext.BaseDirectory;
            AddBindingClass(
                $"public class FileSystemPath {{  public static string GetFilePathForAttachments()  {{  return @\"{location}\\CCK\"; }}  }} ");
        }

        private IEnumerable<Envelope> GetExpectedResults(string scenarioName)
        {
            var workingDirectory = Assembly.GetExecutingAssembly().GetAssemblyLocation();
            var expectedJsonText = File.ReadAllLines(Path.Combine(workingDirectory, $"{scenarioName}.feature.ndjson"));

            foreach(var json in expectedJsonText) yield return NdjsonSerializer.Deserialize(json);
        }
    }
}