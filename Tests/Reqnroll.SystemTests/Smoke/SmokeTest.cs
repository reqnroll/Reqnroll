using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace Reqnroll.SystemTests.Smoke;

[TestClass]
[TestCategory("Smoke")]
public class SmokeTest : SystemTestBase
{
    [TestMethod]
    public void Handles_the_simplest_scenario()
    {
        AddSimpleScenario();
        _projectsDriver.AddPassingStepBinding();

        ExecuteTests();

        ShouldAllScenariosPass();
    }
}
