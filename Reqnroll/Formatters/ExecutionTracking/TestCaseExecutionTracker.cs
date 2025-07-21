using Reqnroll.Events;
using System;
using System.Linq;
using System.Collections.Generic;
using Io.Cucumber.Messages.Types;
using Reqnroll.Formatters.PayloadProcessing.Cucumber;

namespace Reqnroll.Formatters.ExecutionTracking;

/// <summary>
/// Track the execution of a single TestCase (i.e. an execution of a Pickle)
/// There will be multiple of these for a given TestCase if it is retried.
/// </summary>
public class TestCaseExecutionTracker : IGenerateMessage
{
    private readonly ICucumberMessageFactory _messageFactory;
    private readonly TestCaseTracker _testCaseTracker;
    private readonly List<StepExecutionTrackerBase> _stepExecutionTrackers;
    // This queue holds ExecutionEvents that will be processed in stage 2
    private readonly Queue<(IGenerateMessage, ExecutionEvent)> _events = new();

    public IPickleExecutionTracker ParentTracker { get; }

    public int AttemptId { get; }
    public string TestCaseId { get; }

    /// <summary>
    /// The ID of this particular execution of this Test Case
    /// </summary>
    public string TestCaseStartedId { get; }

    public DateTime TestCaseStartedTimestamp { get; private set; }
    public DateTime TestCaseFinishedTimestamp { get; private set; }
    public ScenarioExecutionStatus ScenarioExecutionStatus { get; private set; }

    private StepExecutionTrackerBase CurrentStep => _stepExecutionTrackers.Last();

    public bool IsFirstAttempt => AttemptId == 0;

    public TestCaseExecutionTracker(IPickleExecutionTracker parentTracker, ICucumberMessageFactory messageFactory, int attemptId, string testCaseStartedId, string testCaseId, TestCaseTracker testCaseTracker)
    {
        _messageFactory = messageFactory;
        _stepExecutionTrackers = new();
        _testCaseTracker = testCaseTracker;
        ParentTracker = parentTracker;
        AttemptId = attemptId;
        TestCaseStartedId = testCaseStartedId;
        TestCaseId = testCaseId;
    }

    public IEnumerable<Envelope> RuntimeMessages
    {
        get
        {
            while (_events.Count > 0)
            {
                var (generator, execEvent) = _events.Dequeue();
                foreach (var e in generator.GenerateFrom(execEvent))
                {
                    yield return e;
                }
            }
        }
    }

    private void StoreMessageGenerator(IGenerateMessage generator, ExecutionEvent executionEvent)
    {
        _events.Enqueue((generator, executionEvent));
    }

    IEnumerable<Envelope> IGenerateMessage.GenerateFrom(ExecutionEvent executionEvent)
    {
        switch (executionEvent)
        {
            case ScenarioStartedEvent:
                // On the first execution of this TestCase we emit a TestCase Message.
                // On subsequent retries we do not.
                if (AttemptId == 0)
                {
                    var testCase = _messageFactory.ToTestCase(_testCaseTracker);
                    yield return Envelope.Create(testCase);
                }
                var testCaseStarted = _messageFactory.ToTestCaseStarted(this, TestCaseId);
                yield return Envelope.Create(testCaseStarted);
                break;
            case ScenarioFinishedEvent:
                yield return Envelope.Create(_messageFactory.ToTestCaseFinished(this));
                break;
        }
    }

    public void ProcessEvent(ScenarioStartedEvent scenarioStartedEvent)
    {
        TestCaseStartedTimestamp = scenarioStartedEvent.Timestamp;
        StoreMessageGenerator(this, scenarioStartedEvent);
    }

    public void ProcessEvent(ScenarioFinishedEvent scenarioFinishedEvent)
    {
        TestCaseFinishedTimestamp = scenarioFinishedEvent.Timestamp;
        ScenarioExecutionStatus = scenarioFinishedEvent.ScenarioContext.ScenarioExecutionStatus;
        StoreMessageGenerator(this, scenarioFinishedEvent);
    }

    public void ProcessEvent(StepStartedEvent stepStartedEvent)
    {
        var testStepTracker = new TestStepExecutionTracker(this, _messageFactory);
        testStepTracker.ProcessEvent(stepStartedEvent);
        _stepExecutionTrackers.Add(testStepTracker);
        StoreMessageGenerator(testStepTracker, stepStartedEvent);
    }

    public void ProcessEvent(StepFinishedEvent stepFinishedEvent)
    {
        if (CurrentStep is TestStepExecutionTracker testStepTracker)
        {
            testStepTracker.ProcessEvent(stepFinishedEvent);
            StoreMessageGenerator(testStepTracker, stepFinishedEvent);
        }
    }

    public void ProcessEvent(HookBindingStartedEvent hookBindingStartedEvent)
    {
        var hookStepStateTracker = new HookStepExecutionTracker(this, _messageFactory);
        hookStepStateTracker.ProcessEvent(hookBindingStartedEvent);
        _stepExecutionTrackers.Add(hookStepStateTracker);
        StoreMessageGenerator(hookStepStateTracker, hookBindingStartedEvent);
    }

    public void ProcessEvent(HookBindingFinishedEvent hookBindingFinishedEvent)
    {
        if (CurrentStep is HookStepExecutionTracker hookStepTracker)
        {
            hookStepTracker.ProcessEvent(hookBindingFinishedEvent);
            StoreMessageGenerator(hookStepTracker, hookBindingFinishedEvent);
        }
    }

    public void ProcessEvent(AttachmentAddedEvent attachmentAddedEvent)
    {
        var attachmentTracker = new AttachmentTracker(
            ParentTracker.TestRunStartedId,
            TestCaseStartedId,
            CurrentStep.StepTracker.TestStepId,
            "", // TestRunHookStartedId is not applicable here
            _messageFactory);
        attachmentTracker.ProcessEvent(attachmentAddedEvent);
        StoreMessageGenerator(attachmentTracker, attachmentAddedEvent);
    }

    public void ProcessEvent(OutputAddedEvent outputAddedEvent)
    {
        var outputMessageTracker = new OutputMessageTracker(
            ParentTracker.TestRunStartedId,
            TestCaseStartedId,
            CurrentStep.StepTracker.TestStepId,
            "", // TestRunHookStartedId is not applicable here
            _messageFactory);
        outputMessageTracker.ProcessEvent(outputAddedEvent);
        StoreMessageGenerator(outputMessageTracker, outputAddedEvent);
    }
}