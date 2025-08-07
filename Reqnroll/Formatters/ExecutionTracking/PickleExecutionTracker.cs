using Gherkin.CucumberMessages;
using Io.Cucumber.Messages.Types;
using Reqnroll.Formatters.PayloadProcessing.Cucumber;
using Reqnroll.Events;
using System;
using System.Collections.Generic;
using System.Linq;
using Reqnroll.Bindings;
using Reqnroll.Formatters.PubSub;
using System.Threading.Tasks;

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
    /// <summary>
    /// Ensures correct message ordering for Cucumber messages by buffering messages until a <see cref="TestCase"/> message is published.
    /// <para>
    /// <b>Responsibilities:</b>
    /// <list type="bullet">
    ///   <item>Buffers all messages until a <see cref="TestCase"/> message is encountered.</item>
    ///   <item>Flushes buffered messages immediately after the <see cref="TestCase"/> message is published.</item>
    ///   <item>Passes through all subsequent messages directly to the underlying publisher.</item>
    /// </list>
    /// </para>
    /// <para>
    /// This is necessary because the <see cref="TestCase"/> message has IDs that are referenced in Pickle Execution messages.
    /// </para>
    /// </summary>
    internal class OrderFixingMessagePublisher : IMessagePublisher
    {
        private enum BufferingState
        {
            Buffering,
            PassThru
        }
        private BufferingState _state = BufferingState.Buffering;
        private readonly List<Envelope> _bufferedMessages = new();
        private readonly IMessagePublisher _publisher;

        public OrderFixingMessagePublisher(IMessagePublisher publisher)
        {
            _publisher = publisher ?? throw new ArgumentNullException(nameof(publisher));
        }

        public async Task PublishAsync(Envelope message)
        {
            if (message.Content() is TestCase)
            {
                await _publisher.PublishAsync(message);
                await FlushBuffer();
                _state = BufferingState.PassThru;
            }
            else if (_state == BufferingState.Buffering)
            {
                _bufferedMessages.Add(message);
            }
            else
            {
                await _publisher.PublishAsync(message);
            }
        }

        private async Task FlushBuffer()
        {
            foreach (var message in _bufferedMessages)
            {
                await _publisher.PublishAsync(message);
            }
            _bufferedMessages.Clear();
        }
    }

    private readonly ICucumberMessageFactory _messageFactory;
    private readonly ITestCaseExecutionTrackerFactory _testCaseExecutionTrackerFactory;
    private readonly IMessagePublisher _publisher;
    private readonly List<TestCaseExecutionTracker> _executionHistory = new();

    // Feature FeatureName and Pickle ID make up a unique identifier for tracking execution of Test Cases
    public string FeatureName { get; }
    public string TestRunStartedId { get; }
    public string PickleId { get; }
    public DateTime TestCaseStartedTimeStamp { get; }
    internal bool Enabled { get; } // This will be false if the feature could not be pickled
    public IIdGenerator IdGenerator { get; }

    public string TestCaseId { get; }

    public TestCaseTracker TestCaseTracker { get; }
    public TestCaseExecutionTracker CurrentTestCaseExecutionTracker { get; private set; }

    // This dictionary tracks the StepDefinitions(ID) by their method signature
    // used during TestCase creation to map from a Step Definition binding to its ID
    public IReadOnlyDictionary<IBinding, string> StepDefinitionsByBinding { get; internal set; }

    public int AttemptCount { get; private set; }
    public bool Finished { get; private set; }

    private bool HasCurrentTestCaseExecution => Enabled && CurrentTestCaseExecutionTracker != null;

    public ScenarioExecutionStatus ScenarioExecutionStatus => _executionHistory.Last().ScenarioExecutionStatus;
    public IEnumerable<TestCaseExecutionTracker> ExecutionHistory => _executionHistory;

    /// <summary>
    /// Initializes a new instance of the <see cref="PickleExecutionTracker"/> class for a specific scenario (Pickle).
    /// </summary>
    /// <param name="pickleId">The unique identifier of the scenario (Pickle) being tracked.</param>
    /// <param name="testRunStartedId">The identifier of the test run this scenario belongs to.</param>
    /// <param name="featureName">The name of the feature containing this scenario.</param>
    /// <param name="enabled">Indicates whether this test case is enabled for execution.</param>
    /// <param name="idGenerator"> The ID generator used to create unique IDs for test case messages and related entities.</param>
    /// <param name="stepDefinitionsByMethod">
    /// A thread-safe dictionary mapping step definition patterns to their unique IDs, 
    /// used to resolve step bindings during execution.
    /// </param>
    /// <param name="instant">The timestamp marking when the test case started execution.</param>
    /// <param name="messageFactory">The factory responsible for creating Cucumber message objects.</param>
    /// <param name="testCaseExecutionTrackerFactory">The factory for creating TestExecutionTracker objects.</param>
    /// <param name="publisher">The service that publishes Messages to Formatters</param>
    public PickleExecutionTracker(string pickleId, string testRunStartedId, string featureName, bool enabled, IIdGenerator idGenerator, IReadOnlyDictionary<IBinding, string> stepDefinitionsByMethod, DateTime instant, ICucumberMessageFactory messageFactory, ITestCaseExecutionTrackerFactory testCaseExecutionTrackerFactory, IMessagePublisher publisher)
    {
        TestRunStartedId = testRunStartedId;
        PickleId = pickleId;
        FeatureName = featureName;
        Enabled = enabled;
        IdGenerator = idGenerator;
        StepDefinitionsByBinding = stepDefinitionsByMethod;
        AttemptCount = -1;
        TestCaseStartedTimeStamp = instant;
        _messageFactory = messageFactory;
        _testCaseExecutionTrackerFactory = testCaseExecutionTrackerFactory;
        TestCaseId = IdGenerator.GetNewId();
        _publisher = publisher;
        TestCaseTracker = new TestCaseTracker(TestCaseId, PickleId, this);
    }

    private void SetExecutionRecordAsCurrentlyExecuting(TestCaseExecutionTracker testCaseExecutionTracker)
    {
        CurrentTestCaseExecutionTracker = testCaseExecutionTracker;
        if (!_executionHistory.Contains(testCaseExecutionTracker))
            _executionHistory.Add(testCaseExecutionTracker);
    }

    public async Task FinalizeTracking()
    {
        // The feature has finished (at least for the worker that is processing this scenario),
        // if our scenario has not yet published its TestCaseFinished message, we need to do it now
        if (!HasCurrentTestCaseExecution)
            return;

        await CurrentTestCaseExecutionTracker.FinalizeTracking();
    }

    public async Task ProcessEvent(ScenarioStartedEvent scenarioStartedEvent)
    {
        if (!Enabled) 
            return;

        AttemptCount++;
        scenarioStartedEvent.ScenarioContext.ScenarioInfo.PickleId = PickleId;

        if (AttemptCount > 0 && HasCurrentTestCaseExecution) // HasCurrentTestCaseExecution should be true, but for safety we check
        {
            // A ScenarioStartedEvent with an AttemptCount > 0 indicates a retry of the scenario.
            // We need to publish the TestCaseFinished message for the previous (failed) execution and set it's WillBeRetried property to true.
            var lastRunTestCaseFinishedEnvelope = Envelope.Create(
                _messageFactory.ToTestCaseFinished(CurrentTestCaseExecutionTracker, willBeRetried: true));
            await _publisher.PublishAsync(lastRunTestCaseFinishedEnvelope);
            // Reset tracking
            Finished = false;
            CurrentTestCaseExecutionTracker = null; // will be set again in SetExecutionRecordAsCurrentlyExecuting a few lines below
        }

        var testCaseExecutionTracker = _testCaseExecutionTrackerFactory.CreateTestCaseExecutionTracker(this, AttemptCount, TestCaseId, TestCaseTracker, _publisher);
        SetExecutionRecordAsCurrentlyExecuting(testCaseExecutionTracker);
        await testCaseExecutionTracker.ProcessEvent(scenarioStartedEvent);
    }

    public async Task ProcessEvent(ScenarioFinishedEvent scenarioFinishedEvent)
    {
        if (!HasCurrentTestCaseExecution)
            return;

        Finished = true;
        await CurrentTestCaseExecutionTracker.ProcessEvent(scenarioFinishedEvent);
    }

    public async Task ProcessEvent(StepStartedEvent stepStartedEvent)
    {
        if (!HasCurrentTestCaseExecution)
            return;

        await CurrentTestCaseExecutionTracker.ProcessEvent(stepStartedEvent);
    }

    public async Task ProcessEvent(StepFinishedEvent stepFinishedEvent)
    {
        if (!HasCurrentTestCaseExecution)
            return;

        await CurrentTestCaseExecutionTracker.ProcessEvent(stepFinishedEvent);
    }

    public async Task ProcessEvent(HookBindingStartedEvent hookBindingStartedEvent)
    {
        // At this point we only care about hooks that wrap scenarios or steps; Before/AfterTestRun hooks were processed earlier by the _publisher
        if (hookBindingStartedEvent.HookBinding.HookType is Bindings.HookType.AfterTestRun or Bindings.HookType.BeforeTestRun)
            return;

        if (!HasCurrentTestCaseExecution)
            return;

        await CurrentTestCaseExecutionTracker.ProcessEvent(hookBindingStartedEvent);
    }

    public async Task ProcessEvent(HookBindingFinishedEvent hookBindingFinishedEvent)
    {
        // At this point we only care about hooks that wrap scenarios or steps; TestRunHooks were processed earlier by the Publisher
        if (hookBindingFinishedEvent.HookBinding.HookType == Bindings.HookType.AfterTestRun || hookBindingFinishedEvent.HookBinding.HookType == Bindings.HookType.BeforeTestRun)
            return;

        if (!HasCurrentTestCaseExecution)
            return;

        await CurrentTestCaseExecutionTracker.ProcessEvent(hookBindingFinishedEvent);
    }

    public async Task ProcessEvent(AttachmentAddedEvent attachmentAddedEvent)
    {
        if (!HasCurrentTestCaseExecution)
            return;

        await CurrentTestCaseExecutionTracker.ProcessEvent(attachmentAddedEvent);
    }

    public async Task ProcessEvent(OutputAddedEvent outputAddedEvent)
    {
        if (!HasCurrentTestCaseExecution)
            return;

        await CurrentTestCaseExecutionTracker.ProcessEvent(outputAddedEvent);
    }
}