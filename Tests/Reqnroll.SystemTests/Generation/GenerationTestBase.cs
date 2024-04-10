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
        AddScenarios(
            """
            Scenario: Sample Scenario
                When something happens

            Scenario: Scenario with DataTable
            When something happens with
            	| who          | when     |
            	| me           | today    |
            	| someone else | tomorrow |

            Scenario Outline: Scenario outline with examples
            When something happens to <person>
            Examples:
            	| person |
            	| me     |
            	| you    |
            """);
        AddPassingStepBinding();

        ExecuteTests();

        ShouldAllScenariosPass();
    }

    //test different outcomes: success, failure, pending, undefined, ignored (scenario & scenario outline)
    [TestMethod]
    public void Failing_scenarios_are_counted_as_failures()
    {
        AddScenarios(
            """
            Scenario: Sample Scenario
                When something happens

            Scenario Outline: Scenario outline with examples
            When something happens to <person>
            Examples:
            	| person |
            	| me     |
            	| you    |
            """);

        AddFailingStepBinding();

        ExecuteTests();

        ShouldAllScenariosFail();
    }

    [TestMethod]
    public void Pending_scenarios_are_counted_as_pending()
    {
        AddScenarios(
            """
            Scenario: Sample Scenario
                When something happens

            Scenario Outline: Scenario outline with examples
            When something happens to <person>
            Examples:
            	| person |
            	| me     |
            	| you    |
            """);

        AddPendingStepBinding();

        ExecuteTests();

        ShouldAllScenariosPend();
    }

    [TestMethod]
    public void Ignored_scenarios_are_counted_as_ignored()
    {
        AddScenarios(
            """
            @ignore
            Scenario: Sample Scenario
                When something happens

            @ignore
            Scenario Outline: Scenario outline with examples
            When something happens to <person>
            Examples:
            	| person |
            	| me     |
            	| you    |
            """);

        AddPassingStepBinding();
        _configFileDriver.SetIsRowTestsAllowed( false); //This is necessary as MSTest and Xunit count the number of physical Test methods.
        ExecuteTests();

        ShouldAllScenariosBeIgnored(3); 
    }

    [TestMethod]
    public void Undefined_scenarios_are_not_executed()
    {
        AddScenarios(
            """
            Scenario: Sample Scenario
                When something happens

            Scenario Outline: Scenario outline with examples
            When something happens to <person>
            Examples:
            	| person |
            	| me     |
            	| you    |
            """);

        ExecuteTests();

        ShouldAllUndefinedScenariosNotBeExecuted();
    }


    //test async steps (async steps are executed in order)
    [TestMethod]
    public void Async_steps_are_executed_in_order()
    {
        AddScenarios(
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
    //TODO: Consider adding a AddHookBinding method to SystemTestBase 
    [TestMethod]
    public void TestRun_Feature_and_Scenario_hooks_are_executed_in_right_order()
    {
        AddScenarios(
            """
            Scenario: Sample Scenario
                When something happens

            Scenario Outline: Scenario outline with examples
            When something happens to <person>
            Examples:
            	| person |
            	| me     |
            	| you    |
            """);
        AddPassingStepBinding();
        _projectsDriver.AddHookBinding("BeforeTestRun", code: "global::Log.LogHook();");
        _projectsDriver.AddHookBinding("AfterTestRun", code: "global::Log.LogHook();");
        _projectsDriver.AddHookBinding("BeforeFeature", code: "global::Log.LogHook();");
        _projectsDriver.AddHookBinding("AfterFeature", code: "global::Log.LogHook();");
        _projectsDriver.AddHookBinding("BeforeScenario", code: "global::Log.LogHook();");
        _projectsDriver.AddHookBinding("AfterScenario", code: "global::Log.LogHook();");

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
