using Reqnroll.Events;
using System;
using System.Collections.Generic;
using Io.Cucumber.Messages.Types;
using Reqnroll.Formatters.PayloadProcessing.Cucumber;
using Reqnroll.Formatters.PubSub;
using System.Threading.Tasks;

namespace Reqnroll.Formatters.ExecutionTracking;

/// <summary>
/// Track the execution of a single TestCase (i.e. an execution of a Pickle)
/// There will be multiple of these for a given TestCase if it is retried.
/// </summary>
public class TestCaseExecutionTracker
{
    private readonly ICucumberMessageFactory _messageFactory;
    private readonly Stack<StepExecutionTrackerBase> _stepExecutionTrackers;

    // Per-attempt occurrence counters keyed on step/hook identity. Because a fresh
    // TestCaseExecutionTracker is created for each attempt, these counters are naturally
    // scoped to a single attempt and reset on every retry.
    private readonly Dictionary<(StepKind Kind, string Id), int> _occurrenceCounters = new();

    public IPickleExecutionTracker ParentTracker { get; }

    public int AttemptId { get; }
    public string TestCaseId { get; }

    private readonly IMessagePublisher _publisher;
    private readonly IStepTrackerFactory _stepTrackerFactory;
    private bool _testCaseFinishedHasBeenPublished = false;

    /// <summary>
    /// The ID of this particular execution of this Test Case
    /// </summary>
    public string TestCaseStartedId { get; }

    public DateTime TestCaseStartedTimestamp { get; private set; }
    public DateTime TestCaseFinishedTimestamp { get; private set; }
    public ScenarioExecutionStatus ScenarioExecutionStatus { get; private set; }

    private StepExecutionTrackerBase CurrentStep => _stepExecutionTrackers.Peek();

    public bool IsFirstAttempt => AttemptId == 0;

    public TestCaseExecutionTracker(IPickleExecutionTracker parentTracker, int attemptId, string testCaseStartedId, string testCaseId, ICucumberMessageFactory messageFactory, IMessagePublisher publisher, IStepTrackerFactory stepTrackerFactory)
    {
        _messageFactory = messageFactory;
        _stepExecutionTrackers = new();
        ParentTracker = parentTracker;
        AttemptId = attemptId;
        TestCaseStartedId = testCaseStartedId;
        TestCaseId = testCaseId;
        _publisher = publisher;
        _stepTrackerFactory = stepTrackerFactory;
    }

    /// <summary>
    /// Returns the next 1-based occurrence index for the given step/hook identity within this attempt.
    /// Called exactly once per StepStarted / HookBindingStarted event.
    /// </summary>
    internal int NextOccurrence(StepKind kind, string id)
    {
        var key = (kind, id);
        var next = _occurrenceCounters.TryGetValue(key, out var current) ? current + 1 : 1;
        _occurrenceCounters[key] = next;
        return next;
    }

    public async Task ProcessEvent(ScenarioStartedEvent scenarioStartedEvent)
    {
        TestCaseStartedTimestamp = scenarioStartedEvent.Timestamp;
        var testCaseStarted = _messageFactory.ToTestCaseStarted(this, TestCaseId);
        await _publisher.PublishAsync(Envelope.Create(testCaseStarted));
    }

    public async Task ProcessEvent(ScenarioFinishedEvent scenarioFinishedEvent)
    {
        // Note: publication of the TestCase (definition) message is owned by the PickleExecutionTracker,
        // which emits it once the ledger is known to be complete (a passing/skipped attempt, or at finalize).
        // It cannot be published here at attempt 0, because an aborted attempt 0 may not yet have reached
        // every step/hook; later attempts can still append newly-discovered entries to the ledger.
        TestCaseFinishedTimestamp = scenarioFinishedEvent.Timestamp;
        ScenarioExecutionStatus = scenarioFinishedEvent.ScenarioContext.ScenarioExecutionStatus;

        // We need to delay publishing the TestCaseFinished message to 'FinalizeTracking'
        // because we might have a retry of the scenario and need to set the 'willBeRetried' flag.
        if (ScenarioExecutionStatus != ScenarioExecutionStatus.TestError)
        {
            // If the scenario has not failed, we can publish the TestCaseFinished message
            // otherwise we will publish it later (if retried, or at the end of the Feature if not retried)
            await _publisher.PublishAsync(Envelope.Create(_messageFactory.ToTestCaseFinished(this)));
            _testCaseFinishedHasBeenPublished = true;
        }
    }

    public async Task FinalizeTracking()
    {
        // The Feature has finished, if our scenario has not yet published its TestCaseFinished message, we need to do it now
        if (!_testCaseFinishedHasBeenPublished)
        {
            await _publisher.PublishAsync(Envelope.Create(_messageFactory.ToTestCaseFinished(this)));
        }
    }

    public async Task ProcessEvent(StepStartedEvent stepStartedEvent)
    {
        var testStepExecutionTracker = _stepTrackerFactory.CreateTestStepExecutionTracker(this, _publisher);
        await testStepExecutionTracker.ProcessEvent(stepStartedEvent);
        _stepExecutionTrackers.Push(testStepExecutionTracker);
    }

    public async Task ProcessEvent(StepFinishedEvent stepFinishedEvent)
    {
        if (CurrentStep is TestStepExecutionTracker testStepExecutionTracker)
        {
            await testStepExecutionTracker.ProcessEvent(stepFinishedEvent);
            _stepExecutionTrackers.Pop();
        }
    }

    public async Task ProcessEvent(HookBindingStartedEvent hookBindingStartedEvent)
    {
        var hookStepExecutionTracker = _stepTrackerFactory.CreateHookStepExecutionTracker(this, _publisher);
        await hookStepExecutionTracker.ProcessEvent(hookBindingStartedEvent);
        _stepExecutionTrackers.Push(hookStepExecutionTracker);
    }

    public async Task ProcessEvent(HookBindingFinishedEvent hookBindingFinishedEvent)
    {
        if (CurrentStep is HookStepExecutionTracker hookStepExecutionTracker)
        {
            await hookStepExecutionTracker.ProcessEvent(hookBindingFinishedEvent);
            _stepExecutionTrackers.Pop();
        }
    }

    public async Task ProcessEvent(AttachmentAddedEvent attachmentAddedEvent)
    {
        var attachmentTracker = _stepTrackerFactory.CreateAttachmentTracker(
            ParentTracker.TestRunStartedId,
            TestCaseStartedId,
            CurrentStep.StepTracker.TestStepId,
            "",
            _publisher); // TestRunHookStartedId is not applicable here
        await attachmentTracker.ProcessEvent(attachmentAddedEvent);
    }

    public async Task ProcessEvent(OutputAddedEvent outputAddedEvent)
    {
        var outputMessageTracker = _stepTrackerFactory.CreateOutputMessageTracker(
            ParentTracker.TestRunStartedId,
            TestCaseStartedId,
            CurrentStep.StepTracker.TestStepId,
            "",
            _publisher); // TestRunHookStartedId is not applicable here
        await outputMessageTracker.ProcessEvent(outputAddedEvent);
    }
}
