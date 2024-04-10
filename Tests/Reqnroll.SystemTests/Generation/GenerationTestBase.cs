using Microsoft.VisualStudio.TestTools.UnitTesting;
using Reqnroll.TestProjectGenerator.Driver;
using Microsoft.Extensions.DependencyInjection;

namespace Reqnroll.SystemTests.Generation;

[TestCategory("Generation")]
public class GenerationTestBase : SystemTestBase
{
    private HooksDriver _hookDriver = null!;
    private ConfigurationFileDriver _configFileDriver = null!;

    protected override void TestInitialize()
    {
        base.TestInitialize();
        _hookDriver = _testContainer.GetService<HooksDriver>();
        _configFileDriver = _testContainer.GetService<ConfigurationFileDriver>();
    }

    [TestMethod]
    public void GeneratorAllIn_sample_can_be_handled()
    {
        PrepareGeneratorAllInSamples();

        ExecuteTests();

        ShouldAllScenariosPass();
    }

    [TestMethod]
    public void Handles_simple_scenarios_without_namespace_collisions()
    {
        _projectsDriver.CreateProject("CollidingNamespace.Reqnroll", "C#");
        AddSimpleScenarioAndOutline();
        AddScenario(
            """
            Scenario: Scenario with DataTable
            When something happens with
            	| who          | when     |
            	| me           | today    |
            	| someone else | tomorrow |
            """);
        AddPassingStepBinding();

        ExecuteTests();

        ShouldAllScenariosPass();
    }

    //test different outcomes: success, failure, pending, undefined, ignored (scenario & scenario outline)
    [TestMethod]
    public void Failing_scenarios_are_counted_as_failures()
    {
        AddSimpleScenarioAndOutline();
        AddFailingStepBinding();

        ExecuteTests();

        ShouldAllScenariosFail();
    }

    [TestMethod]
    public void Pending_scenarios_are_counted_as_pending()
    {
        AddSimpleScenarioAndOutline();
        AddPendingStepBinding();

        ExecuteTests();

        ShouldAllScenariosPend();
    }

    [TestMethod]
    public void Ignored_scenarios_are_counted_as_ignored()
    {
        AddScenario(
            """
            @ignore
            Scenario: Sample Scenario
                When something happens
            """);
        AddScenario(
            """
            @ignore
            Scenario Outline: Scenario outline with examples
                When something happens to <person>
            Examples:
            	| person |
            	| me     |
            	| you    |
            """);

        AddPassingStepBinding();
        _configFileDriver.SetIsRowTestsAllowed(false); //This is necessary as MSTest and Xunit count the number of physical Test methods.
        ExecuteTests();

        ShouldAllScenariosBeIgnored();
    }

    [TestMethod]
    public void Undefined_scenarios_are_not_executed()
    {
        AddSimpleScenarioAndOutline();

        ExecuteTests();

        ShouldAllUndefinedScenariosNotBeExecuted();
    }


    //test async steps (async steps are executed in order)
    [TestMethod]
    public void Async_steps_are_executed_in_order()
    {
        AddScenario(
            """
            Scenario: Async Scenario Steps
                Given a list to hold step numbers
                When Async Step '1' is called
                When Async Step '2' is called
                When Async Step '3' is called
                Then async step order should be '1,2,3'
            """);

        AddBindingClass(
            """
            namespace AsyncSequence.StepDefinitions
            {
                [Binding]
                public class AsyncSequenceStepDefinitions
                {
                    private ScenarioContext _scenarioContext;
            
                    public AsyncSequenceStepDefinitions(ScenarioContext scenarioContext)
                    {
                        _scenarioContext = scenarioContext;
                    }
                    
                    [Given("a list to hold step numbers")]
                    public async Task GivenAPlaceholder()
                    {
                        await Task.Run(() => global::Log.LogStep() );
                    }
            
                    [When("Async Step {string} is called")]
                    public async Task WhenStepIsTaken(string p0)
                    {
                        await Task.Run(() => global::Log.LogStep() );
                    }
            
                    [Then("async step order should be {string}")]
                    public async Task ThenStepSequenceIs(string p0)
                    {
                        await Task.Run(() =>
                        {
                            global::Log.LogStep();
                        });
                    }
                }
            }
            """);

        ExecuteTests();
        CheckAreStepsExecutedInOrder(new[] { "GivenAPlaceholder", "WhenStepIsTaken", "ThenStepSequenceIs" });

        ShouldAllScenariosPass();
    }

    //test hooks: before/after run, feature & scenario hook (require special handling by test frameworks)
    [TestMethod]
    public void TestRun_Feature_and_Scenario_hooks_are_executed_in_right_order()
    {
        var testsInFeatureFile1 = 3;
        AddSimpleScenario();
        AddSimpleScenarioOutline(testsInFeatureFile1 - 1);
        AddPassingStepBinding();
        AddHookBinding("BeforeTestRun");
        AddHookBinding("AfterTestRun");
        AddHookBinding("BeforeFeature");
        AddHookBinding("AfterFeature");
        AddHookBinding("BeforeScenario");
        AddHookBinding("AfterScenario");

        ExecuteTests();

        _hookDriver.CheckIsHookExecutedInOrder(new[]
        {
            "BeforeTestRun", 
            "BeforeFeature", 
            "BeforeScenario", 
            "AfterScenario", 
            "BeforeScenario", 
            "AfterScenario", 
            "BeforeScenario", 
            "AfterScenario", 
            "AfterFeature", 
            "AfterTestRun"
        });
        ShouldAllScenariosPass();
    }

    //TODO: test scenario outlines (nr of examples, params are available in ScenarioContext, allowRowTests=false, examples tags)
    //TODO: test parallel execution (details TBD) - maybe this should be in a separate test class


}
