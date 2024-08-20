
using System.Reflection;

namespace CucumberMessages.CompatibilityTests
{
    [TestClass]
    public class CucumberCompatibilitySmokeTest : CucumberCompatibilityTestBase
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
        [DataRow("minimal")]
        [DataRow("cdata")]
        [DataRow("pending")]
        [DataRow("examples-tables")]
        [DataRow("data-tables")]
        [DataRow("parameter-types")]
        [DataRow("skipped")]
        [DataRow("undefined")]
        [DataRow("unknown-parameter-type")]
        [DataRow("rules")]
        public void CCKPassingScenarios(string scenarioName)
        {
            AddCucumberMessagePlugIn();

            scenarioName = scenarioName.Replace("-", "_");

            AddFeatureFileFromResource($"{scenarioName}/{scenarioName}.feature", "CucumberMessages.CompatibilityTests.CCK", Assembly.GetExecutingAssembly());
            AddBindingClassFromResource($"{scenarioName}/{scenarioName}.cs", "CucumberMessages.CompatibilityTests.CCK", Assembly.GetExecutingAssembly());

            ExecuteTests();

            ConfirmAllTestsRan(null);
        }


    }
}