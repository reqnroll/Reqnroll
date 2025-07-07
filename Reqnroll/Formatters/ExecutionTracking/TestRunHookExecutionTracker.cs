using Io.Cucumber.Messages.Types;
using Reqnroll.Formatters.PayloadProcessing.Cucumber;
using Reqnroll.Events;
using System;
using System.Collections.Generic;

namespace Reqnroll.Formatters.ExecutionTracking;

/// <summary>
/// Captures information about TestRun Hooks (Before/After TestRun and Before/After Feature)
/// </summary>
public class TestRunHookExecutionTracker(string hookStartedId, string hookId, string testRunId, ICucumberMessageFactory messageFactory)
    : IGenerateMessage
{
    public string TestRunId { get; } = testRunId;
    public string HookId { get; } = hookId;
    public string HookStartedId { get; } = hookStartedId;

    public DateTime HookStarted { get; private set; }
    public DateTime HookFinished { get; private set; }
    public TimeSpan Duration { get; private set; }
    public System.Exception Exception { get; private set; }
    public bool IsActive => HookStarted != default && HookFinished == default;
    public ScenarioExecutionStatus Status => Exception == null ? ScenarioExecutionStatus.OK : ScenarioExecutionStatus.TestError;

    IEnumerable<Envelope> IGenerateMessage.GenerateFrom(ExecutionEvent executionEvent)
    {
        return executionEvent switch
        {
            HookBindingStartedEvent => new List<Envelope> { Envelope.Create(messageFactory.ToTestRunHookStarted(this)) },
            HookBindingFinishedEvent => new List<Envelope> { Envelope.Create(messageFactory.ToTestRunHookFinished(this)) },
            _ => throw new ArgumentOutOfRangeException(nameof(executionEvent), executionEvent, null),
        };
    }

    public void ProcessEvent(HookBindingStartedEvent hookBindingStartedEvent)
    {
        HookStarted = hookBindingStartedEvent.Timestamp;
    }

    public void ProcessEvent(HookBindingFinishedEvent hookBindingFinishedEvent)
    {
        Duration = hookBindingFinishedEvent.Duration;
        Exception = hookBindingFinishedEvent.HookException;
        HookFinished = hookBindingFinishedEvent.Timestamp;
    }
}