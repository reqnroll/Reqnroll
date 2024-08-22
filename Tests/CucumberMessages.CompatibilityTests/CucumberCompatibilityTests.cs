
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

            AddFeatureFile("""
                Feature: Cucumber Messages Smoke Test
                Scenario: Smoke Test
                    Given I have a passing step
                """);

            AddPassingStepBinding("Given");

            ExecuteTests();

            ShouldAllScenariosPass();
        }

        [TestMethod]
        //[DataRow("attachments")]
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

            scenarioName = scenarioName.Replace("-", "_");

            AddFeatureFileFromResource($"{scenarioName}/{scenarioName}.feature", "CucumberMessages.CompatibilityTests.CCK", Assembly.GetExecutingAssembly());
            AddBindingClassFromResource($"{scenarioName}/{scenarioName}.cs", "CucumberMessages.CompatibilityTests.CCK", Assembly.GetExecutingAssembly());
            AddBinaryFilesFromResource($"{scenarioName}", "CucumberMessages.CompatibilityTests.CCK", Assembly.GetExecutingAssembly());

            ExecuteTests();

            ConfirmAllTestsRan(null);
            if (scenarioName == "attachments")
            {
                ShouldAllScenariosPass();
            }
        }

        private IEnumerable<Envelope> GetExpectedResults(string scenarioName)
        {
            var workingDirectory = Assembly.GetExecutingAssembly().GetAssemblyLocation();
            var expectedJsonText = File.ReadAllLines(Path.Combine(workingDirectory, $"{scenarioName}.feature.ndjson"));

            foreach(var json in expectedJsonText) yield return NdjsonSerializer.Deserialize(json);
        }
    }
}