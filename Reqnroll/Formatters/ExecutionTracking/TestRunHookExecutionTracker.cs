using Io.Cucumber.Messages.Types;
using Reqnroll.Formatters.PayloadProcessing.Cucumber;
using Reqnroll.Events;
using System;
using Reqnroll.Formatters.PubSub;
using System.Threading.Tasks;

namespace Reqnroll.Formatters.ExecutionTracking;

/// <summary>
/// Captures information about TestRun Hooks (Before/After TestRun and Before/After Feature)
/// </summary>
public class TestRunHookExecutionTracker(string hookStartedId, string testRunId, string hookId, ICucumberMessageFactory messageFactory, IMessagePublisher publisher)
{
    public string TestRunId { get; } = testRunId;
    public string HookId { get; } = hookId;
    public string HookStartedId { get; } = hookStartedId;

    public DateTime? HookStarted { get; private set; }
    public DateTime? HookFinished { get; private set; }
    public TimeSpan? Duration { get; private set; }
    public System.Exception Exception { get; private set; }
    public bool IsActive => HookStarted.HasValue && !HookFinished.HasValue;
    public ScenarioExecutionStatus Status => Exception == null ? ScenarioExecutionStatus.OK : ScenarioExecutionStatus.TestError;

    public async Task ProcessEvent(HookBindingStartedEvent hookBindingStartedEvent)
    {
        HookStarted = hookBindingStartedEvent.Timestamp;
        await publisher.PublishAsync(Envelope.Create(messageFactory.ToTestRunHookStarted(this)));
    }

    public async Task ProcessEvent(HookBindingFinishedEvent hookBindingFinishedEvent)
    {
        Duration = hookBindingFinishedEvent.Duration;
        Exception = hookBindingFinishedEvent.HookException;
        HookFinished = hookBindingFinishedEvent.Timestamp;
        await publisher.PublishAsync(Envelope.Create(messageFactory.ToTestRunHookFinished(this)));
    }
}