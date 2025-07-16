using Io.Cucumber.Messages.Types;
using Reqnroll.Formatters.PayloadProcessing.Cucumber;
using Reqnroll.Events;
using System.Collections.Generic;
using System;

namespace Reqnroll.Formatters.ExecutionTracking;

/// <summary>
/// Tracks the addition of a particular attachment. The <see cref="TestCaseStartedId"/> and <see cref="TestCaseStepId"/> are optional,
/// they are only filled out if the attachment is added in a scope of a scenario or step/hook execution.
/// </summary>
public class AttachmentTracker : IGenerateMessage
{
    private readonly ICucumberMessageFactory _messageFactory;

    public string TestRunStartedId { get; }
    public string TestCaseStartedId { get; }
    public string TestCaseStepId { get; }
    public string TestRunHookStartedId { get; }
    public string FilePath { get; private set; }
    public DateTime Timestamp { get; private set; }

    internal AttachmentTracker(string testRunStartedId, string testCaseStartedId, string testCaseStepId, string testRunHookStartedId, ICucumberMessageFactory messageFactory)
    {
        _messageFactory = messageFactory;
        TestRunStartedId = testRunStartedId;
        TestCaseStartedId = testCaseStartedId;
        TestCaseStepId = testCaseStepId;
        TestRunHookStartedId = testRunHookStartedId;
    }

    IEnumerable<Envelope> IGenerateMessage.GenerateFrom(ExecutionEvent executionEvent)
    {
        return [Envelope.Create(_messageFactory.ToAttachment(this))];
    }

    public void ProcessEvent(AttachmentAddedEvent attachmentAddedEvent)
    {
        FilePath = attachmentAddedEvent.FilePath;
        Timestamp = attachmentAddedEvent.Timestamp;
    }
}