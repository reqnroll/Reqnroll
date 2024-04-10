using Microsoft.VisualStudio.TestTools.UnitTesting;
using System;
using FluentAssertions;
using Reqnroll.TestProjectGenerator.Driver;
using Reqnroll.TestProjectGenerator;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using Microsoft.Extensions.DependencyInjection;

namespace Reqnroll.SystemTests.Generation;

[TestCategory("Generation")]
public class GenerationTestBase : SystemTestBase
{
    private HooksDriver _hookDriver = null!;
    private TestProjectGenerator.Driver.ConfigurationDriver _configDriver = null!;

    protected override void TestInitialize()
    {
        base.TestInitialize();
        _hookDriver = _testContainer.GetService<HooksDriver>();
        _configDriver = _testContainer.GetService<TestProjectGenerator.Driver.ConfigurationDriver>();
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

        AddScenario(
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
    public void FailingScenariosAreCountedAsFailures()
    {
        AddScenario(
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
    public void PendingScenariosAreCountedAsPending()
    {
        AddScenario(
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
    public void IgnoredScenariosAreCountedAsIgnored()
    {
        AddScenario(
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
        _configDriver.SetIsRowTestsAllowed( false); //This is necessary as MSTest and Xunit count the number of physical Test methods.
        ExecuteTests();

        ShouldAllScenariosBeIgnored(3); 
    }

    [TestMethod]
    public void UndefinedScenariosAreNotExecuted()
    {
        AddScenario(
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

        //AddPassingStepBinding();

        ExecuteTests();

        ShouldAllUndefinedScenariosNotBeExecuted();
    }


    //test async steps (async steps are executed in order)
    [TestMethod]
    public void AsyncStepsAreExecutedInOrder()
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

        var _asyncBindingClassContent =
            @"namespace AsyncSequence.StepDefinitions
{
    [Binding]
    public class AsyncsequenceStepDefinitions
    {
        private ScenarioContext _scenarioContext;

        public AsyncsequenceStepDefinitions(ScenarioContext scenarioContext) {
            _scenarioContext = scenarioContext;
        }
        [Given(""a list to hold step numbers"")]
        public async Task GivenAPlaceholder()
        {
            await Task.Run(() => global::Log.LogStep() );
        }

        [When(""Async Step {string} is called"")]
        public async Task WhenStepIsTaken(string p0)
        {
            await Task.Run(() => global::Log.LogStep() );
        }

        [Then(""async step order should be {string}"")]
        public async Task ThenStepSequenceIs( string p0)
        {
            await Task.Run( () => 
            {   
                global::Log.LogStep();
            }
            );
        }
    }
}
";
        AddBindingClass(_asyncBindingClassContent);

        ExecuteTests();
        CheckAreStepsExecutedInOrder(new string[] { "GivenAPlaceholder", "WhenStepIsTaken", "ThenStepSequenceIs" });

        ShouldAllScenariosPass();
    }
    //test hooks: before/after test run (require special handling by test frameworks)
    //TODO: Consider adding a AddHookBinding method to SystemTestBase 
    [TestMethod]
    public void BeforeAndAfterTestHooksRun()
    {
        AddScenario(
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

        _projectsDriver.AddHookBinding("BeforeTestRun", "BeforeTestRun", null, null, null, "global::Log.LogHook();");
        _projectsDriver.AddHookBinding("AfterTestRun", "AfterTestRun", null, null, null, "global::Log.LogHook();");

        ExecuteTests();

        _hookDriver?.CheckIsHookExecutedInOrder(new string[] { "BeforeTestRun", "AfterTestRun" });
        ShouldAllScenariosPass();

    }

    //test hooks: before/after test feature & scenario (require special handling by test frameworks)
    [TestMethod]
    public void BeforeAndAfterFeatureAndScenarioHooksRun()
    {
        AddScenario(
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

        _projectsDriver.AddHookBinding("BeforeFeature", "BeforeFeatureRun", null, null, null, "global::Log.LogHook();");
        _projectsDriver.AddHookBinding("AfterFeature", "AfterFeatureRun", null, null, null, "global::Log.LogHook();");
        _projectsDriver.AddHookBinding("BeforeScenario", "BeforeSenarioRun", null, null, null, "global::Log.LogHook();");
        _projectsDriver.AddHookBinding("AfterScenario", "AfterScenarioRun", null, null, null, "global::Log.LogHook();");

        ExecuteTests();

        _hookDriver.CheckIsHookExecutedInOrder(new string[] { "BeforeFeatureRun", "BeforeSenarioRun", "AfterScenarioRun", "BeforeSenarioRun", "AfterScenarioRun", "BeforeSenarioRun", "AfterScenarioRun", "AfterFeatureRun" });
        ShouldAllScenariosPass();

    }

    //TODO: test scenario outlines (nr of examples, params are available in ScenarioContext, allowRowTests=false, examples tags)
    //TODO: test parallel execution (details TBD) - maybe this should be in a separate test class


}
