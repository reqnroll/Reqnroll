using Microsoft.VisualStudio.TestTools.UnitTesting;
using Reqnroll.TestProjectGenerator.Data;

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

    [TestMethod]
    public void Can_load_step_definitions_from_external_assembly()
    {
        _solutionDriver.EnsureDefaultProject();
        var externalLibraryName = "ExternalStepsLibrary";
        var externalLib = _projectsDriver.CreateProject(externalLibraryName, "C#", ProjectType.Library)!;
        externalLib.IsReqnrollFeatureProject = false;

        externalLib.AddStepBinding("When", "something happens in external lib", "//pass", null);
        AddScenario(
            """
            Scenario: Sample Scenario
                When something happens in external lib
            """);

        ExecuteTests();

        ShouldAllScenariosPass();
    }
}
