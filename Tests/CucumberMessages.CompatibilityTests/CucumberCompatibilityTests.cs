
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
        public void CCKPassingScenarios(string scenarioName)
        {
            AddCucumberMessagePlugIn();

            AddFeatureFileFromResource($"{scenarioName}/{scenarioName}.feature", "CucumberMessages.CompatibilityTests.CCK", Assembly.GetExecutingAssembly());
            AddBindingClassFromResource($"{scenarioName}/{scenarioName}.cs", "CucumberMessages.CompatibilityTests.CCK", Assembly.GetExecutingAssembly());

            ExecuteTests();

            ShouldAllScenariosPass();
        }

        [TestMethod]
        [DataRow("pending")]
        public void CCKPendingScenarios(string scenarioName)
        {
            AddCucumberMessagePlugIn();

            AddFeatureFileFromResource($"{scenarioName}/{scenarioName}.feature", "CucumberMessages.CompatibilityTests.CCK", Assembly.GetExecutingAssembly());
            AddBindingClassFromResource($"{scenarioName}/{scenarioName}.cs", "CucumberMessages.CompatibilityTests.CCK", Assembly.GetExecutingAssembly());

            ExecuteTests();

            ShouldAllScenariosPend();
        }

    }
}