﻿using System.Linq;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using FluentAssertions;

namespace Reqnroll.SystemTests.Generation;

[TestCategory("Generation")]
public abstract class GenerationTestBase : SystemTestBase
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

        _bindingDriver.GetActualLogLines("tags").Should().BeEquivalentTo(
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

    //TODO: test parallel execution (details TBD) - maybe this should be in a separate test class
}
