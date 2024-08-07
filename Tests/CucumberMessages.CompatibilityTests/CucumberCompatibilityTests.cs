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

            AddPassingStepBinding();

            ExecuteTests();

            ShouldAllScenariosPass();
        }
    }
}