using Gherkin.CucumberMessages;
using Io.Cucumber.Messages.Types;
using Reqnroll.Formatters.PayloadProcessing.Cucumber;
using Reqnroll.Events;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;

namespace Reqnroll.Formatters.ExecutionTracking;

/// <summary>
/// Tracks the execution lifecycle and state of a single Gherkin scenario (Pickle/TestCase).
/// </summary>
/// <remarks>
/// <para>
/// <b>Responsibilities:</b>
/// <list type="bullet">
///   <item>Maintains scenario-level and feature-level execution data for a single test case.</item>
///   <item>Records each execution attempt (including retries) as a <see cref="TestCaseExecutionTracker"/>.</item>
///   <item>Processes and responds to all relevant execution events (scenario, step, hook, attachment, output).</item>
///   <item>Generates Cucumber messages.</item>
///   <item>Tracks step definitions and their mapping to execution steps within the scenario.</item>
/// </list>
/// </para>
/// <para>
/// There is one <c>PickleExecutionTracker</c> instance per scenario (Pickle) in a feature.
/// </para>
/// </remarks>
public class PickleExecutionTracker : IPickleExecutionTracker
{
    private readonly ICucumberMessageFactory _messageFactory;
    private readonly List<TestCaseExecutionTracker> _executionHistory = new();
    private TestCaseExecutionTracker _currentTestCaseExecutionTracker;

    // Feature FeatureName and Pickle ID make up a unique identifier for tracking execution of Test Cases
    public string FeatureName { get; }
    public string TestRunStartedId { get; }
    public string PickleId { get; }
    public DateTime TestCaseStartedTimeStamp { get; }
    internal bool Enabled { get; } // This will be false if the feature could not be pickled
    public IIdGenerator IdGenerator { get; }

    public string TestCaseId { get; }
    public TestCaseTracker TestCaseTracker { get; }

    // This dictionary tracks the StepDefinitions(ID) by their method signature
    // used during TestCase creation to map from a Step Definition binding to its ID
    public ConcurrentDictionary<string, string> StepDefinitionsByMethodSignature { get; }

    public int AttemptCount { get; private set; }
    public bool Finished { get; private set; }

    private bool HasCurrentTestCaseExecution => Enabled && _currentTestCaseExecutionTracker != null;

    public ScenarioExecutionStatus ScenarioExecutionStatus => _executionHistory.Last().ScenarioExecutionStatus;

    /// <summary>
    /// Initializes a new instance of the <see cref="PickleExecutionTracker"/> class for a specific scenario (Pickle).
    /// </summary>
    /// <param name="pickleId">The unique identifier of the scenario (Pickle) being tracked.</param>
    /// <param name="testRunStartedId">The identifier of the test run this scenario belongs to.</param>
    /// <param name="featureName">The name of the feature containing this scenario.</param>
    /// <param name="enabled">Indicates whether this test case is enabled for execution.</param>
    /// <param name="idGenerator"> The ID generator used to create unique IDs for test case messages and related entities.</param>
    /// <param name="stepDefinitionsByMethodSignature">
    /// A thread-safe dictionary mapping step definition patterns to their unique IDs, 
    /// used to resolve step bindings during execution.
    /// </param>
    /// <param name="instant">The timestamp marking when the test case started execution.</param>
    /// <param name="messageFactory">The factory responsible for creating Cucumber message objects.</param>
    public PickleExecutionTracker(string pickleId, string testRunStartedId, string featureName, bool enabled, IIdGenerator idGenerator, ConcurrentDictionary<string, string> stepDefinitionsByMethodSignature, DateTime instant, ICucumberMessageFactory messageFactory)
    {
        TestRunStartedId = testRunStartedId;
        PickleId = pickleId;
        FeatureName = featureName;
        Enabled = enabled;
        IdGenerator = idGenerator;
        StepDefinitionsByMethodSignature = stepDefinitionsByMethodSignature;
        AttemptCount = -1;
        TestCaseStartedTimeStamp = instant;
        _messageFactory = messageFactory;

        TestCaseId = IdGenerator.GetNewId();
        TestCaseTracker = new TestCaseTracker(TestCaseId, PickleId, this, messageFactory);
    }

    private void SetExecutionRecordAsCurrentlyExecuting(TestCaseExecutionTracker testCaseExecutionTracker)
    {
        _currentTestCaseExecutionTracker = testCaseExecutionTracker;
        if (!_executionHistory.Contains(testCaseExecutionTracker))
            _executionHistory.Add(testCaseExecutionTracker);
    }

    // Returns all the Cucumber Messages that result from execution of the Test Case (eg, TestCase, TestCaseStarted, TestCaseFinished, TestStepStarted/Finished)
    public IEnumerable<Envelope> RuntimeGeneratedMessages
    {
        get
        {
            // ask each execution record for its messages
            var tempListOfMessages = new List<Envelope>();
            foreach (var testCaseExecution in _executionHistory)
            {
                var aRunsWorth = testCaseExecution.RuntimeMessages;
                if (tempListOfMessages.Count > 0)
                {
                    // there has been a previous run of this scenario that was retried
                    // We will create a copy of the last TestRunFinished message, but with the 'willBeRetried' flag set to true
                    // and substitute the copy into the list
                    var lastRunTestCaseFinished = tempListOfMessages.Last();
                    var lastRunTestCaseMarkedAsToBeRetried = FixupWillBeRetried(lastRunTestCaseFinished);
                    tempListOfMessages.Remove(lastRunTestCaseFinished);
                    tempListOfMessages.Add(lastRunTestCaseMarkedAsToBeRetried);
                }
                tempListOfMessages.AddRange(aRunsWorth);
            }
            return tempListOfMessages;
        }
    }

    private Envelope FixupWillBeRetried(Envelope lastRunTestCaseFinishedEnvelope)
    {
        var testCaseFinished = lastRunTestCaseFinishedEnvelope.Content() as TestCaseFinished;
        if (testCaseFinished == null) throw new InvalidOperationException("Invalid TestCaseFinished envelope");
        return Envelope.Create(new TestCaseFinished(testCaseFinished.TestCaseStartedId, testCaseFinished.Timestamp, true));
    }

    public void ProcessEvent(ScenarioStartedEvent scenarioStartedEvent)
    {
        if (!Enabled) 
            return;

        AttemptCount++;
        scenarioStartedEvent.ScenarioContext.ScenarioInfo.PickleId = PickleId;
        // on the first time this Scenario is executed, create a TestCaseTracker
        if (AttemptCount == 0)
        {
            //TestCaseId = IdGenerator.GetNewId();
            //TestCaseTracker = new TestCaseTracker(TestCaseId, PickleId, this);
        }
        else
        {
            // Reset tracking
            Finished = false;
            _currentTestCaseExecutionTracker = null;
        }

        var testCaseExecutionRecord = new TestCaseExecutionTracker(this, _messageFactory, AttemptCount, IdGenerator.GetNewId(), TestCaseId, TestCaseTracker);
        SetExecutionRecordAsCurrentlyExecuting(testCaseExecutionRecord);
        testCaseExecutionRecord.ProcessEvent(scenarioStartedEvent);
    }

    public void ProcessEvent(ScenarioFinishedEvent scenarioFinishedEvent)
    {
        if (!HasCurrentTestCaseExecution)
            return;

        Finished = true;
        _currentTestCaseExecutionTracker.ProcessEvent(scenarioFinishedEvent);
    }

    public void ProcessEvent(StepStartedEvent stepStartedEvent)
    {
        if (!HasCurrentTestCaseExecution)
            return;

        _currentTestCaseExecutionTracker.ProcessEvent(stepStartedEvent);
    }

    public void ProcessEvent(StepFinishedEvent stepFinishedEvent)
    {
        if (!HasCurrentTestCaseExecution)
            return;

        _currentTestCaseExecutionTracker.ProcessEvent(stepFinishedEvent);
    }

    public void ProcessEvent(HookBindingStartedEvent hookBindingStartedEvent)
    {
        // At this point we only care about hooks that wrap scenarios or steps; Before/AfterTestRun hooks were processed earlier by the publisher
        if (hookBindingStartedEvent.HookBinding.HookType is Bindings.HookType.AfterTestRun or Bindings.HookType.BeforeTestRun)
            return;

        if (!HasCurrentTestCaseExecution)
            return;

        _currentTestCaseExecutionTracker.ProcessEvent(hookBindingStartedEvent);
    }

    public void ProcessEvent(HookBindingFinishedEvent hookBindingFinishedEvent)
    {
        // At this point we only care about hooks that wrap scenarios or steps; TestRunHooks were processed earlier by the Publisher
        if (hookBindingFinishedEvent.HookBinding.HookType == Bindings.HookType.AfterTestRun || hookBindingFinishedEvent.HookBinding.HookType == Bindings.HookType.BeforeTestRun)
            return;

        if (!HasCurrentTestCaseExecution)
            return;

        _currentTestCaseExecutionTracker.ProcessEvent(hookBindingFinishedEvent);
    }

    public void ProcessEvent(AttachmentAddedEvent attachmentAddedEvent)
    {
        if (!HasCurrentTestCaseExecution)
            return;

        _currentTestCaseExecutionTracker.ProcessEvent(attachmentAddedEvent);
    }

    public void ProcessEvent(OutputAddedEvent outputAddedEvent)
    {
        if (!HasCurrentTestCaseExecution)
            return;

        _currentTestCaseExecutionTracker.ProcessEvent(outputAddedEvent);
    }
}