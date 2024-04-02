using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace Reqnroll.SystemTests.Generation;

[TestCategory("Generation")]
public class GenerationTestBase : SystemTestBase
{
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

        ExecuteTests();

        ShouldAllScenariosBeIgnored(2); 
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
            await Task.Run(() => _scenarioContext[nameof(AsyncsequenceStepDefinitions)] = new List<string>() );
        }

        [When(""Async Step {string} is called"")]
        public async Task WhenStepIsTaken(string p0)
        {
            await Task.Run(() => ((IList<string>)_scenarioContext[nameof(AsyncsequenceStepDefinitions)]).Add(p0) );
        }

        [Then(""async step order should be {string}"")]
        public async Task ThenStepSequenceIs( string p0)
        {
            await Task.Run( () => 
            {   
                if (p0 != String.Join("","", (IList<string>)_scenarioContext[nameof(AsyncsequenceStepDefinitions)]))
                {
                    throw new Exception(""Step sequence is not correct"");
                }
            }
            );
        }
    }
}
";
        AddBindingClass(_asyncBindingClassContent);

        ExecuteTests();
        ShouldAllScenariosPass();
    }
    //TODO: test hooks: before/after test run (require special handling by test frameworks)
    //TODO: test hooks: before/after test feature & scenario (require special handling by test frameworks)
    //TODO: test scenario outlines (nr of examples, params are available in ScenarioContext, allowRowTests=false, examples tags)
    //TODO: test parallel execution (details TBD) - maybe this should be in a separate test class
}
