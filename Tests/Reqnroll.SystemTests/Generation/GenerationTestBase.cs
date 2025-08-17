using FluentAssertions;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Reqnroll.TestProjectGenerator;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.Json;

namespace Reqnroll.SystemTests.Generation;

[TestCategory("Generation")]
public abstract class GenerationTestBase : SystemTestBase
{
    [TestCategory("focus")]
    [TestMethod]
    public void GeneratorAllIn_sample_can_be_handled()
    {
        PrepareGeneratorAllInSamples();

        ExecuteTests();

        ShouldAllScenariosPass();

        ShouldFinishWithoutTestExecutionWarnings();
    }

    [TestMethod]
    public void GeneratorAllIn_sample_can_be_handled_with_VisualBasic()
    {
        if (_testRunConfiguration.UnitTestProvider == UnitTestProvider.TUnit)
        {
            Assert.Inconclusive("Issues with current TUnit Version (https://github.com/thomhurst/TUnit/issues/2905 and https://github.com/thomhurst/TUnit/issues/2906). Check again and remove inconclusive if a fix is available.");
        }

        _testRunConfiguration.ProgrammingLanguage = ProgrammingLanguage.VB;

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

    #region Test different outcomes: success, failure, pending, undefined, ignored (scenario & scenario outline)
    protected virtual string GetExpectedPendingOutcome() => "NotExecuted";
    protected virtual string GetExpectedUndefinedOutcome() => "NotExecuted";
    protected virtual string GetExpectedIgnoredOutcome() => "NotExecuted";

    [TestMethod]
    public void Handles_different_scenario_and_scenario_outline_outcomes()
    {
        AddFeatureFile(
            """
            Feature: Sample Feature
            
            Scenario: Passing scenario
            When the step passes        

            Scenario: Failing scenario
            When the step fails        

            Scenario: Pending scenario
            When the step is pending        

            Scenario: Undefined scenario
            When the step is undefined        

            @ignore
            Scenario: Ignored scenario
            When the step fails        

            Scenario Outline: SO
            When the step <result>
            Examples:
                | result       |
                | passes       |
                | fails        |
                | is pending   |
                | is undefined |
            @ignore
            Examples:
                | result       |
                | ignored      |
                
            @ignore
            Scenario Outline: ExampleIgnored
            When the step <result>
            Examples:
                | result |
                | fails  |

            @ignore
            Rule: Scenario in this Rule should be Ignored
             Scenario: Ruleignored scenario
             When the step passes
            """);
        _projectsDriver.AddPassingStepBinding(stepRegex: "the step passes");
        _projectsDriver.AddFailingStepBinding(stepRegex: "the step fails");
        _projectsDriver.AddStepBinding("StepDefinition", regex: "the step is pending", "throw new PendingStepException();", "Throw New PendingStepException()");
        ExecuteTests();

        // handles PASSED
        var expectedPassedOutcome = "Passed";
        _vsTestExecutionDriver.LastTestExecutionResult.LeafTestResults
                              .Should().ContainSingle(tr => tr.TestName.StartsWith("Passing"))
                              .Which.Outcome.Should().Be(expectedPassedOutcome);
        _vsTestExecutionDriver.LastTestExecutionResult.LeafTestResults
            .Should().ContainSingle(tr => tr.TestName.StartsWith("SO") && tr.TestName.Contains("passes"))
            .Which.Outcome.Should().Be(expectedPassedOutcome);

        // handles FAILED
        var expectedFailedOutcome = "Failed";
        _vsTestExecutionDriver.LastTestExecutionResult.LeafTestResults
                              .Should().ContainSingle(tr => tr.TestName.StartsWith("Failing"))
                              .Which.Outcome.Should().Be(expectedFailedOutcome);
        _vsTestExecutionDriver.LastTestExecutionResult.LeafTestResults
            .Should().ContainSingle(tr => tr.TestName.StartsWith("SO") && tr.TestName.Contains("fails"))
            .Which.Outcome.Should().Be(expectedFailedOutcome);

        // handles PENDING
        var expectedPendingOutcome = GetExpectedPendingOutcome();
        _vsTestExecutionDriver.LastTestExecutionResult.LeafTestResults
                              .Should().ContainSingle(tr => tr.TestName.StartsWith("Pending"))
                              .Which.Outcome.Should().Be(expectedPendingOutcome);
        _vsTestExecutionDriver.LastTestExecutionResult.LeafTestResults
            .Should().ContainSingle(tr => tr.TestName.StartsWith("SO") && tr.TestName.Contains("pending"))
            .Which.Outcome.Should().Be(expectedPendingOutcome);

        // handles UNDEFINED
        var expectedUndefinedOutcome = GetExpectedUndefinedOutcome();
        _vsTestExecutionDriver.LastTestExecutionResult.LeafTestResults
                              .Should().ContainSingle(tr => tr.TestName.StartsWith("Undefined"))
                              .Which.Outcome.Should().Be(expectedUndefinedOutcome);
        _vsTestExecutionDriver.LastTestExecutionResult.LeafTestResults
            .Should().ContainSingle(tr => tr.TestName.StartsWith("SO") && tr.TestName.Contains("undefined"))
            .Which.Outcome.Should().Be(expectedUndefinedOutcome);

        // handles IGNORED
        var expectedIgnoredOutcome = GetExpectedIgnoredOutcome();
        _vsTestExecutionDriver.LastTestExecutionResult.LeafTestResults
                              .Should().ContainSingle(tr => tr.TestName.StartsWith("Ignored"))
                              .Which.Outcome.Should().Be(expectedIgnoredOutcome);
        _vsTestExecutionDriver.LastTestExecutionResult.LeafTestResults
                              .Should().ContainSingle(tr => tr.TestName.StartsWith("ExampleIgnored"))
                              .Which.Outcome.Should().Be(expectedIgnoredOutcome);
        _vsTestExecutionDriver.LastTestExecutionResult.LeafTestResults
                              .Should().ContainSingle(tr => tr.TestName.StartsWith("Ruleignored")) 
                              .Which.Outcome.Should().Be(expectedIgnoredOutcome);
        AssertIgnoredScenarioOutlineExampleHandled();
    }

    protected virtual void AssertIgnoredScenarioOutlineExampleHandled()
    {
        // the scenario outline examples ignored by a tag on the examples block are not generated
        _vsTestExecutionDriver.LastTestExecutionResult.LeafTestResults
                              .Should().NotContain(tr => tr.TestName.StartsWith("SO") && tr.TestName.Contains("ignored"))
                              .And.Subject.Where(tr => tr.TestName.StartsWith("SO")).Should().HaveCount(4);

    }
    #endregion

    #region Test async steps (async steps are executed in order)
    [TestMethod]
    public void Async_steps_are_executed_in_order()
    {
        AddScenario(
            """
            Scenario: Async Scenario Steps
                When Async step 1 is called
                When Async step 2 is called
                When Async step 3 is called
            """);

        AddBindingClass(
            """
            namespace AsyncSequence.StepDefinitions
            {
                [Binding]
                public class AsyncSequenceStepDefinitions
                {
                    [When("Async step 1 is called")]
                    public async Task WhenStep1IsTaken()
                    {
                        await Task.Run(() => global::Log.LogStep() );
                    }
                    [When("Async step 2 is called")]
                    public async Task WhenStep2IsTaken()
                    {
                        await Task.Run(() => global::Log.LogStep() );
                    }
                    [When("Async step 3 is called")]
                    public async Task WhenStep3IsTaken()
                    {
                        await Task.Run(() => global::Log.LogStep() );
                    }
                }
            }
            """);

        ExecuteTests();
        _bindingDriver.AssertExecutedStepsEqual("WhenStep1IsTaken", "WhenStep2IsTaken", "WhenStep3IsTaken");

        ShouldAllScenariosPass();
    }
    #endregion

    #region Test hooks: before/after run, feature & scenario hook (require special handling by test frameworks)
    [TestMethod]
    public void TestRun_Feature_and_Scenario_hooks_are_executed_in_right_order()
    {
        var testsInFeatureFile = 3;
        AddSimpleScenario();
        AddSimpleScenarioOutline(testsInFeatureFile - 1);
        AddPassingStepBinding();
        AddHookBinding("BeforeTestRun");
        AddHookBinding("AfterTestRun");
        AddHookBinding("BeforeFeature");
        AddHookBinding("AfterFeature");
        AddHookBinding("BeforeScenario");
        AddHookBinding("AfterScenario");

        ExecuteTests();

        _bindingDriver.AssertExecutedHooksEqual(
            "BeforeTestRun",
            "BeforeFeature",
            "BeforeScenario",
            "AfterScenario",
            "BeforeScenario",
            "AfterScenario",
            "BeforeScenario",
            "AfterScenario",
            "AfterFeature",
            "AfterTestRun");
        ShouldAllScenariosPass();
    }

    [TestMethod]
    public void Async_TestRun_Feature_and_Scenario_hooks_are_executed_in_right_order()
    {
        var testsInFeatureFile = 3;
        AddSimpleScenario();
        AddSimpleScenarioOutline(testsInFeatureFile - 1);
        AddPassingStepBinding();
        const string waitCode = "await Task.Delay(TimeSpan.FromMilliseconds(50));";
        AddHookBinding("BeforeTestRun", code: waitCode, asyncHook: true);
        AddHookBinding("AfterTestRun", code: waitCode, asyncHook: true);
        AddHookBinding("BeforeFeature", code: waitCode, asyncHook: true);
        AddHookBinding("AfterFeature", code: waitCode, asyncHook: true);
        AddHookBinding("BeforeScenario", code: waitCode, asyncHook: true);
        AddHookBinding("AfterScenario", code: waitCode, asyncHook: true);

        ExecuteTests();

        _bindingDriver.AssertExecutedHooksEqual(
            "BeforeTestRun",
            "BeforeFeature",
            "BeforeScenario",
            "AfterScenario",
            "BeforeScenario",
            "AfterScenario",
            "BeforeScenario",
            "AfterScenario",
            "AfterFeature",
            "AfterTestRun");
        ShouldAllScenariosPass();
    }

    [TestMethod]
    public void Before_After_Feature_hooks_execute_only_once_on_sequential_run_even_with_failing_scenario_hooks()
    {
        // After a failure, the scenario execution should properly "release" the test runner, so that 
        // the next scenario can run in the same one.
        // We verify here that only one test runner is used by logging and checking _testRunnerManager.IsMultiThreaded
        // and that the BeforeFeature and AfterFeature hooks are executed only once.

        AddFeatureFile(
            """
            Feature: Feature 1
            
            @failBefore
            Scenario: Before scenario hook failing
            When the step passes
            
            @failAfter
            Scenario: After scenario hook failing
            When the step passes
            
            Scenario: Passing scenario
            When the step passes
            
            Scenario: Failing scenario
            When the step fails
            """);

        AddBindingClass(
            """
            namespace ReqnrollExecution.StepDefinitions
            {
                [Binding]
                public class ReqnrollExecutionBindings
                {
                    private readonly ITestRunnerManager _testRunnerManager;
                    
                    public ReqnrollExecutionBindings(ITestRunnerManager testRunnerManager)
                    {
                        _testRunnerManager = testRunnerManager;
                    }
                    
                    [When("the step passes")]
                    public void WhenTheStepPasses()
                    {
                        global::Log.LogStep();
                        Log.LogCustom("parallel", _testRunnerManager.IsMultiThreaded.ToString());
                    }
                    
                    [When("the step fails")]
                    public void WhenTheStepFails()
                    {
                        global::Log.LogStep();
                        Log.LogCustom("parallel", _testRunnerManager.IsMultiThreaded.ToString());
                        throw new System.Exception("simulated failure");
                    }
                    
                    [BeforeScenario("@failBefore")]
                    public void FailBefore()
                    {
                        Log.LogCustom("parallel", _testRunnerManager.IsMultiThreaded.ToString());
                        throw new System.Exception("Fail before");
                    }
                    
                    [AfterScenario("@failAfter")]
                    public void FailAfter()
                    {
                        Log.LogCustom("parallel", _testRunnerManager.IsMultiThreaded.ToString());
                        throw new System.Exception("Fail after");
                    }
                }
            }
            """);

        AddHookBinding("BeforeTestRun");
        AddHookBinding("AfterTestRun");
        AddHookBinding("BeforeFeature");
        AddHookBinding("AfterFeature");
        AddHookBinding("BeforeScenario");
        AddHookBinding("AfterScenario");

        ExecuteTests();
        ConfirmAllTestsRan();

        var parallelLogs = _bindingDriver.GetActualLogLines("parallel").ToList();
        parallelLogs.Should().NotBeEmpty("the scenarios should have parallel logs");
        parallelLogs.Should().AllSatisfy(log => log.Should().StartWith("-> parallel: False"));

        _bindingDriver.AssertExecutedHooksEqual(
            "BeforeTestRun",
            "BeforeFeature",
            "BeforeScenario",
            "AfterScenario",
            "BeforeScenario",
            "AfterScenario",
            "BeforeScenario",
            "AfterScenario",
            "BeforeScenario",
            "AfterScenario",
            "AfterFeature",
            "AfterTestRun");

        _vsTestExecutionDriver.LastTestExecutionResult.Succeeded.Should().Be(1, "only 'Passing scenario' should pass");
        _vsTestExecutionDriver.LastTestExecutionResult.Failed.Should().Be(3, "the three other scenario should fail");
    }

    [TestMethod]
    public void Before_After_Feature_hooks_execute_only_once_on_sequential_run_even_with_failing_feature_hooks()
    {
        _solutionDriver.DefaultProject.Configuration.Generator.Value.AllowDebugGeneratedFiles = true;

        // After a failure, the scenario execution should properly "release" the test runner, so that 
        // the next scenario can run in the same one.
        // We verify here that only one test runner is used by logging and checking _testRunnerManager.IsMultiThreaded
        // and that the BeforeFeature and AfterFeature hooks are executed only once.

        AddFeatureFile(
            """
            @failBefore
            Feature: Feature 1
            
            Scenario: Passing scenario 1
            When the step passes
            
            Scenario: Failing scenario 1
            When the step fails
            """);
        AddFeatureFile(
            """
            @failAfter
            Feature: Feature 2
            
            Scenario: Passing scenario 2
            When the step passes
            
            Scenario: Failing scenario 2
            When the step fails
            """);
        AddFeatureFile(
            """
            Feature: Feature 3
            
            Scenario: Passing scenario 3
            When the step passes
            
            Scenario: Failing scenario 3
            When the step fails
            """);

        AddBindingClass(
            """
            namespace ReqnrollExecution.StepDefinitions
            {
                [Binding]
                public class ReqnrollExecutionBindings
                {
                    private readonly ITestRunnerManager _testRunnerManager;
                    
                    public ReqnrollExecutionBindings(ITestRunnerManager testRunnerManager)
                    {
                        _testRunnerManager = testRunnerManager;
                    }
                    
                    [When("the step passes")]
                    public void WhenTheStepPasses()
                    {
                        global::Log.LogStep();
                        Log.LogCustom("parallel", _testRunnerManager.IsMultiThreaded.ToString());
                    }
                    
                    [When("the step fails")]
                    public void WhenTheStepFails()
                    {
                        global::Log.LogStep();
                        Log.LogCustom("parallel", _testRunnerManager.IsMultiThreaded.ToString());
                        throw new System.Exception("simulated failure");
                    }
                    
                    [BeforeFeature(Order = 0)]
                    public static void BeforeFeature(FeatureContext featureContext)
                    {
                        Log.LogCustom("hook", featureContext.FeatureInfo.Title);
                    }
                    
                    [AfterFeature(Order = 0)]
                    public static void AfterFeature(FeatureContext featureContext)
                    {
                        Log.LogCustom("hook", featureContext.FeatureInfo.Title);
                    }
                    
                    [BeforeScenario(Order = 0)]
                    public void BeforeScenario(FeatureContext featureContext, ScenarioContext scenarioContext)
                    {
                        Log.LogCustom("hook", featureContext.FeatureInfo.Title + "/" + scenarioContext.ScenarioInfo.Title);
                    }
                    
                    [AfterScenario(Order = 0)]
                    public void AfterScenario(FeatureContext featureContext, ScenarioContext scenarioContext)
                    {
                        Log.LogCustom("hook", featureContext.FeatureInfo.Title + "/" + scenarioContext.ScenarioInfo.Title);
                    }
                    
                    [BeforeFeature("@failBefore", Order = 10)]
                    public static void FailBefore()
                    {
                        throw new System.Exception("Fail before");
                    }
                    
                    [AfterFeature("@failAfter", Order = 10)]
                    public static void FailAfter()
                    {
                        throw new System.Exception("Fail after");
                    }
                }
            }
            """);

        AddHookBinding("BeforeTestRun", order: 0);
        AddHookBinding("AfterTestRun", order: 0);

        ExecuteTests();

        var hookLines = _bindingDriver.GetActualHookLines().ToList();
        TestContext.WriteLine(string.Join(Environment.NewLine, hookLines.Select((l,i) => $"#{i}: {l}")));

        var parallelLogs = _bindingDriver.GetActualLogLines("parallel").ToList();
        parallelLogs.Should().NotBeEmpty("the scenarios should have parallel logs");
        parallelLogs.Should().AllSatisfy(log => log.Should().StartWith("-> parallel: False"));

        var featureOrAboveHookLines = hookLines.Where(l => l.Contains("BeforeFeature") || l.Contains("AfterFeature") || l.Contains("BeforeTestRun") || l.Contains("AfterTestRun")).ToList();
        for (int i = 1; i <= 3; i++)
        {
            featureOrAboveHookLines.Should()
                                   .ContainInOrder(
                                       $"-> hook: Feature {i}:BeforeFeature",
                                       $"-> hook: Feature {i}:AfterFeature"
                                   );
        }

        if (_testRunConfiguration.UnitTestProvider != UnitTestProvider.TUnit)
        {
            featureOrAboveHookLines.Should().OnlyHaveUniqueItems();
        }

        hookLines.Should().HaveElementAt(0, "-> hook: BeforeTestRun", "The BeforeTestRun hook should be the first");
        hookLines.Should().HaveElementAt(hookLines.Count-1, "-> hook: AfterTestRun", "The AfterTestRun hook should be the last");

        // -> hook: Feature 2/Passing scenario 2:BeforeScenario
        hookLines.Should().NotContainMatch("-> hook: Feature 1/* scenario 1:*Scenario", "Scenarios of features with a failing before feature hook should not be executed.");

        _vsTestExecutionDriver.LastTestExecutionResult.Output.Should().NotContain("NullReferenceException");
        _vsTestExecutionDriver.LastTestExecutionResult.Output.Should().NotContain("The previous ScenarioContext was already disposed.");
    }

    [TestMethod]
    public void After_Feature_hooks_exception_handled_on_correct_time()
    {
        AddFeatureFile(
            """
            Feature: Feature 1
            
            Scenario: Passing scenario 1
            When the step passes
            """);
        AddFeatureFile(
            """
            @failAfter
            Feature: Feature 2
            
            Scenario: Failing scenario 2
            When the step passes
            """);
        AddFeatureFile(
            """
            Feature: Feature 3
            
            Scenario: Passing scenario 3
            When the step passes
            """);

        AddBindingClass(
            """
            namespace ReqnrollExecution.StepDefinitions
            {
                class MySampleFailAfterException : Exception
                { }

                [Binding]
                public class ReqnrollExecutionBindings
                {
                    private readonly ITestRunnerManager _testRunnerManager;
                    
                    public ReqnrollExecutionBindings(ITestRunnerManager testRunnerManager)
                    {
                        _testRunnerManager = testRunnerManager;
                    }
                    
                    [When("the step passes")]
                    public void WhenTheStepPasses()
                    {
                        global::Log.LogStep();
                        Log.LogCustom("parallel", _testRunnerManager.IsMultiThreaded.ToString());
                    }
                    
                    [AfterFeature(Order = 0)]
                    public static void AfterFeature(FeatureContext featureContext)
                    {
                        Log.LogCustom("hook", "AfterFeature");
                    }
                    
                    [AfterFeature("@failAfter", Order = 10)]
                    public static void FailAfter()
                    {
                        Log.LogCustom("hook", "FailAfter");
                        throw new MySampleFailAfterException();
                    }
                }
            }
            """);

        ExecuteTests();

        var parallelLogs = _bindingDriver.GetActualLogLines("parallel").ToList();
        parallelLogs.Should().NotBeEmpty("the scenarios should have parallel logs");
        parallelLogs.Should().AllSatisfy(log => log.Should().StartWith("-> parallel: False"));

        _bindingDriver.CheckIsHookExecuted("AfterFeature", 3);
        _bindingDriver.CheckIsHookExecuted("FailAfter", 1);
        _vsTestExecutionDriver.CheckOutputContainsText("MySampleFailAfterException");

        // Note: the used test frameworks handle exception in AfterFeature differently (e.g. xUnit creates another test run result), that's why the checks here are not 100% exact.
        _vsTestExecutionDriver.LastTestExecutionResult.Succeeded.Should().BeGreaterThanOrEqualTo(2);
        _vsTestExecutionDriver.LastTestExecutionResult.Failed.Should().BeLessThanOrEqualTo(1);
        _vsTestExecutionDriver.LastTestExecutionResult.LeafTestResults
            .Should().ContainSingle(tr => tr.TestName.StartsWith("Passing") && tr.TestName.EndsWith('1'))
            .Which.Outcome.Should().Be("Passed");
        _vsTestExecutionDriver.LastTestExecutionResult.LeafTestResults
            .Should().Contain(tr => tr.TestName.StartsWith("Failing") && tr.TestName.EndsWith('2'));
        _vsTestExecutionDriver.LastTestExecutionResult.LeafTestResults
            .Should().ContainSingle(tr => tr.TestName.StartsWith("Passing") && tr.TestName.EndsWith('3'))
            .Which.Outcome.Should().Be("Passed");
    }

    #endregion

    #region Test scenario outlines (nr of examples, params are available in ScenarioContext, allowRowTests=false, examples tags)

    [TestMethod]
    public void Scenario_outline_examples_gather_tags_and_parameters()
    {
        AddFeatureFile(
            """
            @feature_tag
            Feature: Sample Feature

            @rule_tag
            Rule: Sample Rule
            
            @so1_tag
            Scenario Outline: SO1
            When <what1> happens to <person>
            Examples:
                | what1     | person |
                | something | me     |
                | nothing   | you    |
            
            @so2_tag
            Scenario Outline: SO2
            When <what2> happens to <person>
            Examples:
                | what2     | person |
                | something | me     |
            @e3_tag
            Examples: E3
                | what2   | person |
                | nothing | you    |
            """);
        _projectsDriver.AddStepBinding("StepDefinition", regex: ".*",
            """
            global::Log.LogCustom("tags", string.Join(",", _scenarioContext.ScenarioInfo.CombinedTags));
            global::Log.LogCustom("parameters", string.Join(",", _scenarioContext.ScenarioInfo.Arguments.OfType<System.Collections.DictionaryEntry>().Select(kv => $"{kv.Key}={kv.Value}")));
            """);

        ExecuteTests();

        ShouldAllScenariosPass(4);

        _bindingDriver.GetActualLogLines("tags")
            .Should()
            .BeEquivalentTo(
                "-> tags: so1_tag,feature_tag,rule_tag:StepBinding",
                "-> tags: so1_tag,feature_tag,rule_tag:StepBinding",
                "-> tags: so2_tag,feature_tag,rule_tag:StepBinding",
                "-> tags: so2_tag,e3_tag,feature_tag,rule_tag:StepBinding");

        _bindingDriver.GetActualLogLines("parameters").Should().BeEquivalentTo(
                "-> parameters: what1=something,person=me:StepBinding",
                "-> parameters: what1=nothing,person=you:StepBinding",
                "-> parameters: what2=something,person=me:StepBinding",
                "-> parameters: what2=nothing,person=you:StepBinding");
    }

    #endregion

    #region Test background support (backgrounds steps are executed at the appropriate times)

    [TestMethod]
    public void Handles_scenario_with_backgrounds()
    {
        AddFeatureFile(
            """
            Feature: Sample Feature

            Background:
                Given background step 1 is called

            Scenario: Background Scenario Steps
                When scenario step is called

            Rule: Rule with background
                Background:
                    Given background step 2 is called
                    
                Scenario: Rule Background Scenario Steps
                    When scenario step is called

            """);

        AddBindingClass(
            """
            namespace Background.StepDefinitions
            {
                [Binding]
                public class BackgroundStepDefinitions
                {
                    [Given("background step 1 is called")]
                    public async Task GivenBackgroundStep1IsCalled()
                    {
                        await Task.Run(() => global::Log.LogStep() );
                    }
                    [Given("background step 2 is called")]
                    public async Task GivenBackgroundStep2IsCalled()
                    {
                        await Task.Run(() => global::Log.LogStep() );
                    }
                    [When("scenario step is called")]
                    public async Task WhenScenarioStepIsCalled()
                    {
                        await Task.Run(() => global::Log.LogStep() );
                    }
                }
            }
            """);

        ExecuteTests();

        var results = _vsTestExecutionDriver.LastTestExecutionResult.LeafTestResults;

        results.Should().ContainSingle(tr => tr.TestName == "Background Scenario Steps" || tr.TestName == "BackgroundScenarioSteps")
            .Which.Steps.Select(result => result.Step).Should().BeEquivalentTo(
            [
                "Given background step 1 is called",
                "When scenario step is called"
            ]);

        results.Should().ContainSingle(tr => tr.TestName == "Rule Background Scenario Steps" || tr.TestName == "RuleBackgroundScenarioSteps")
            .Which.Steps.Select(result => result.Step).Should().BeEquivalentTo(
            [
                "Given background step 1 is called",
                "Given background step 2 is called",
                "When scenario step is called"
            ]);

        ShouldAllScenariosPass();
    }

    #endregion

    #region Test tables arguments are processed

    [TestMethod]
    public void Table_arguments_are_passed_to_steps()
    {
        AddScenario(
            """
            Scenario: Using tables with steps
                When this table is processed
                | Example |
                | A       |
                | B       |
                | C       |
            """);

        AddBindingClass(
            """
            namespace TableArguments.StepDefinitions
            {
                [Binding]
                public class TableArgumentSteps
                {
                    [When("this table is processed")]
                    public async Task WhenThisTableIsProcessed(DataTable table)
                    {
                        var tableData = new
                        {
                            headings = table.Header.ToList(),
                            rows = table.Rows.Select(row => row.Select(kvp => kvp.Value)).ToList()
                        };

                        Log.LogCustom("argument", $"table = {System.Text.Json.JsonSerializer.Serialize(tableData)}");
                    }
                }
            }
            """);

        ExecuteTests();

        var arguments = _bindingDriver.GetActualLogLines("argument").ToList();

        arguments.Should().NotBeEmpty();

        arguments[0].Should().StartWith("-> argument: table = ");
        var tableSource = arguments[0];
        var tableJson = tableSource[tableSource.IndexOf('{')..(tableSource.LastIndexOf('}')+1)];
        var tableData = JsonSerializer.Deserialize<JsonElement>(tableJson);

        var actualHeadings = tableData
            .GetProperty("headings")
            .EnumerateArray()
            .Select(item => item.ToString());

        var actualRows = tableData
            .GetProperty("rows")
            .EnumerateArray()
            .Select(item => item.EnumerateArray().Select(data => data.ToString()).ToList());

        actualHeadings.Should().BeEquivalentTo(["Example"]);
        actualRows.Should().BeEquivalentTo(new List<List<string>> { new() { "A" }, new() { "B" }, new() { "C" } });
    }

    #endregion

    #region It is able to run tests parallel

    // We need to verify
    // Tests are run in parallel
    // Each scenario execution has a feature context and scenario context that is matching to the feature/scenario
    // For each scenario the before feature has been executed for that feature context

    [TestMethod]
    public void Tests_can_be_executed_parallel()
    {
        _projectsDriver.EnableTestParallelExecution();

        var scenarioTemplate = 
            """
            Scenario: {0}
              When executing '{0}' in '{1}'
            """;

        var scenarioOutlineTemplate = 
            """
            Scenario Outline: {0}
              When executing '{0}' in '{1}'
            Examples:
            	| Count |
            	| 1     |
            	| 2     |
                | 3     |
            """;

        const int scenariosPerFile = 3;
        const int scenarioOutlinesPerFile = 3;
        string[] features = ["A", "B"];
        int scenarioIdCounter = 0;

        foreach (string feature in features)
        {
            AddFeatureFile($"Feature: {feature}" + Environment.NewLine);
            for (int i = 0; i < scenariosPerFile; i++)
                AddScenario(string.Format(scenarioTemplate, $"S{++scenarioIdCounter}", feature));
            for (int i = 0; i < scenarioOutlinesPerFile; i++)
                AddScenario(string.Format(scenarioOutlineTemplate, $"S{++scenarioIdCounter}", feature));
        }

        AddBindingClass(
            """
            namespace ParallelExecution.StepDefinitions
            {
                [Binding]
                public class ParallelExecutionSteps
                {
                    public static int startIndex = 0;
                    public static int featureAStartIndex = 0;
                    public static int featureBStartIndex = 0;
                    
                    private readonly FeatureContext _featureContext;
                    private readonly ScenarioContext _scenarioContext;
                    
                    public ParallelExecutionSteps(FeatureContext featureContext, ScenarioContext scenarioContext)
                    {
                        _featureContext = featureContext;
                        _scenarioContext = scenarioContext;
                    }
                    
                    [BeforeFeature]
                    public static void BeforeFeature(FeatureContext featureContext)
                    {
                        featureContext.Set(true, "before_feature");
                    }
                
                    [When("executing {string} in {string}")]
                    public void WhenExecuting(string scenarioName, string featureName)
                    {
                        var currentStartIndex = System.Threading.Interlocked.Increment(ref startIndex);
                        var currentFeatureStartIndex = featureName == "A" 
                            ? System.Threading.Interlocked.Increment(ref featureAStartIndex)
                            : System.Threading.Interlocked.Increment(ref featureBStartIndex);
                        global::Log.LogStep();
                        if (_scenarioContext.ScenarioInfo.Title != scenarioName)
                            throw new System.Exception($"Invalid scenario context: {_scenarioContext.ScenarioInfo.Title} should be {scenarioName}");
                        if (_featureContext.FeatureInfo.Title != featureName)
                            throw new System.Exception($"Invalid scenario context: {_featureContext.FeatureInfo.Title} should be {featureName}");
                        if (!_featureContext.TryGetValue<bool>("before_feature", out var value) || !value)
                            throw new System.Exception($"BeforeFeature hook was not executed!");
                            
                        System.Threading.Thread.Sleep(10);
                        
                        var afterStartIndex = startIndex;
                        if (afterStartIndex != currentStartIndex)
                            Log.LogCustom("parallel", "true");
                        var afterFeatureStartIndex = featureName == "A" ? featureAStartIndex : featureBStartIndex;
                        if (afterFeatureStartIndex != currentFeatureStartIndex)
                            Log.LogCustom("scenario-parallel", "true");
                    }
                }
            }
            """);

        ExecuteTests();
        ShouldAllScenariosPass();

        var parallelLogs = _bindingDriver.GetActualLogLines("parallel").ToList();
        parallelLogs.Should().NotBeEmpty("the scenarios should have run parallel");

        AssertScenarioLevelParallelExecution();
    }

    protected virtual void AssertScenarioLevelParallelExecution()
    {
        var scenarioParallelLogs = _bindingDriver.GetActualLogLines("scenario-parallel").ToList();
        scenarioParallelLogs.Should().NotBeEmpty("the scenarios should have run parallel using scenario-level parallelization");
    }

    #endregion
}
