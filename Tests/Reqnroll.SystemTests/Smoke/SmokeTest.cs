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

        var externalLib = _projectsDriver.CreateProject("ExternalStepsLibrary", "C#", ProjectType.Library)!;
        externalLib.IsReqnrollFeatureProject = false;
        externalLib.AddStepBinding("When", "something happens in external lib", "//pass", null);

        _projectsDriver.AddProjectReference(externalLib.ProjectName);
        _solutionDriver.DefaultProject.Configuration.BindingAssemblies.Add(new BindingAssembly(externalLib.ProjectName));

        AddScenario(
            """
            Scenario: Sample Scenario
                When something happens in external lib
            """);

        ExecuteTests();

        ShouldAllScenariosPass();
    }
}
