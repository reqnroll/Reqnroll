using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace Reqnroll.SystemTests.Smoke;

[TestClass]
[TestCategory("Smoke")]
public class ReqnrollNamespaceCollisionsAvoidedTest : SystemTestBase
{

    protected override void TestInitialize() {
        base.TestInitialize();
        _projectsDriver.CreateProject("CollidingNamespace.Reqnroll", "C#");

    }

    [TestMethod]
    public void Handles_a_simple_scenario_without_namespace_collisions()
    {
        AddScenario(
            """
            Scenario: Sample Scenario
                When something happens
            """);
        _projectsDriver.AddPassingStepBinding();

        ExecuteTests();

        ShouldAllScenariosPass();
    }

    [TestMethod]
    public void Handles_a_scenario_with_DataTable_without_namespace_collisions()
    {
        AddScenario(
            """
            Scenario: Scenario with DataTable
            When something happens with
            	| who          | when     |
            	| me           | today    |
            	| someone else | tomorrow |
            """);
        _projectsDriver.AddPassingStepBinding();

        ExecuteTests();

        ShouldAllScenariosPass();
    }

}
