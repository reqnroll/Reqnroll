@xUnit @NUnit3 @MSTest
Feature: MethodLevel Parallelisation

Background:
    Given there is a Reqnroll project
    And parallel execution is enabled
    And the following binding class 
        """
using Reqnroll;
using Reqnroll.Tracing;
using System.Collections.Concurrent;
using System.Diagnostics;
using FluentAssertions;

[Binding]
public class TraceSteps
{
    sealed class FeatureData
    {
        public Stopwatch Duration { get; } = Stopwatch.StartNew();
        public volatile int StepCount;
        public ConcurrentDictionary<ITestRunner, byte> TestRunners { get; } = new ConcurrentDictionary<ITestRunner, byte>();
        public ConcurrentDictionary<FeatureContext, byte> FeatureContexts { get; } = new ConcurrentDictionary<FeatureContext, byte>();
        public ConcurrentDictionary<ScenarioContext, byte> ScenarioContexts { get; } = new ConcurrentDictionary<ScenarioContext, byte>();
        public ConcurrentDictionary<TraceSteps, byte> BindingInstances { get; } = new ConcurrentDictionary<TraceSteps, byte>();
    }

    private readonly ITraceListener _traceListener;
    private readonly ITestRunner _testRunner;
    private volatile int _ScenarioLocalCounter;

    public TraceSteps(ITraceListener traceListener, ITestRunner testRunner)
    {
        _traceListener = traceListener;
        _testRunner = testRunner;

        Interlocked.Increment(ref _ScenarioLocalCounter);
    }

    [BeforeFeature]
    static void BeforeFeature(FeatureContext featureContext, ITestRunner testRunner)
    {
        testRunner.ScenarioContext.Should().BeNull();

        var featureData = new FeatureData();
        featureData.TestRunners.TryAdd(testRunner, 1);
        featureData.FeatureContexts.TryAdd(featureContext, 1);
        featureContext.Set(featureData);
    }

    static FeatureData GetFeatureData(FeatureContext featureContext) => featureContext.Get<FeatureData>();
    const int WaitTimeInMS = 1_000;

    [AfterFeature]
    static void AfterFeature(FeatureContext featureContext, ITestRunner testRunner)
    {
        testRunner.ScenarioContext.Should().BeNull();

        var featureData = GetFeatureData(featureContext);
        featureData.TestRunners.TryAdd(testRunner, 1).Should().BeFalse();
        featureData.Duration.Stop();
        featureData.TestRunners.Count.Should().Be(11, because: "One TestRunner for before/after hooks and one for each test is created");
        featureData.FeatureContexts.Count.Should().Be(1, because: "Only one FeatureContext should be created");
        featureData.ScenarioContexts.Count.Should().Be(10, because: "One ScenarioContext for each test is created");
        featureData.BindingInstances.Count.Should().Be(10, because: "One binding instance for each test is created");
        featureData.Duration.ElapsedMilliseconds.Should().BeLessThan(9 * WaitTimeInMS, because: "Test should be processed (parallel) in time");
    }

    [When(@"I do something in Scenario '(.*)'")]
    void WhenIDoSomething(string scenario)
    {
        _testRunner.ScenarioContext.Should().NotBeNull();
        _testRunner.ScenarioContext.ScenarioInfo.Title.Should().Be(scenario);

        Interlocked.Increment(ref _ScenarioLocalCounter);
        _ScenarioLocalCounter.Should().Be(2);

        var featureData = GetFeatureData(_testRunner.FeatureContext);
        featureData.TestRunners.TryAdd(_testRunner, 1);
        featureData.FeatureContexts.TryAdd(_testRunner.FeatureContext, 1);
        featureData.ScenarioContexts.TryAdd(_testRunner.ScenarioContext, 1);
        featureData.BindingInstances.TryAdd(this, 1);
        var currentStartIndex = Interlocked.Increment(ref featureData.StepCount);
        _traceListener.WriteTestOutput($"Start index: {currentStartIndex}, Worker: {_testRunner.TestWorkerId}");
        Thread.Sleep(WaitTimeInMS);
        var afterStartIndex = featureData.StepCount;
        if (afterStartIndex == currentStartIndex)
        {
            _traceListener.WriteTestOutput("Was not parallel");
        }
        else
        {
            _traceListener.WriteTestOutput("Was parallel");
        }
    }
}
        """

    And there is a feature file in the project as
        """
Feature: Feature 1
Scenario Outline: Simple Scenario Outline 1
	When I do something in Scenario 'Simple Scenario Outline 1'

Examples:
	| Count |
	| 1     |
    | 2     |
    | 3     |

Scenario Outline: Simple Scenario Outline 2
	When I do something in Scenario 'Simple Scenario Outline 2'

Examples:
	| Count |
	| 1     |
	| 2     |
    | 3     |

Scenario Outline: Simple Scenario Outline 3
	When I do something in Scenario 'Simple Scenario Outline 3'

Scenario Outline: Simple Scenario Outline 4
	When I do something in Scenario 'Simple Scenario Outline 4'

Scenario Outline: Simple Scenario Outline 5
	When I do something in Scenario 'Simple Scenario Outline 5'

Scenario Outline: Simple Scenario Outline 6
	When I do something in Scenario 'Simple Scenario Outline 6'
        """

Scenario: Precondition: Tests run parallel 
    When I execute the tests
    Then the execution log should contain text 'Was parallel'

Scenario: Tests should be processed parallel without failure
    When I execute the tests
    Then the execution log should contain text 'Was parallel'
    And the execution summary should contain
        | Total | Succeeded |
        | 10    | 10        |

Scenario Outline: Before/After TestRun hook should only be executed once
    Given a hook 'HookFor<event>' for '<event>'
    When I execute the tests
    Then the execution log should contain text 'Was parallel'
    And the hook 'HookFor<event>' is executed once

Examples:
    | event               |
    | BeforeTestRun       |
    | AfterTestRun        |

