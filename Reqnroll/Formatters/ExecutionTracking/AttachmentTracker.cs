using Io.Cucumber.Messages.Types;
using Reqnroll.Formatters.PayloadProcessing.Cucumber;
using Reqnroll.Events;
using System;
using System.Threading.Tasks;
using Reqnroll.Formatters.PubSub;

namespace Reqnroll.Formatters.ExecutionTracking;

/// <summary>
/// Tracks the addition of a particular attachment. The <see cref="TestCaseStartedId"/> and <see cref="TestCaseStepId"/> are optional,
/// they are only filled out if the attachment is added in a scope of a scenario or step/hook execution.
/// </summary>
public class AttachmentTracker 
{
    private readonly ICucumberMessageFactory _messageFactory;

    public string TestRunStartedId { get; }
    public string TestCaseStartedId { get; }
    public string TestCaseStepId { get; }
    public string TestRunHookStartedId { get; }

    private readonly IMessagePublisher _publisher;

    public string FilePath { get; private set; }
    public DateTime Timestamp { get; private set; }

    internal AttachmentTracker(string testRunStartedId, string testCaseStartedId, string testCaseStepId, string testRunHookStartedId, ICucumberMessageFactory messageFactory, IMessagePublisher publisher)
    {
        _messageFactory = messageFactory;
        TestRunStartedId = testRunStartedId;
        TestCaseStartedId = testCaseStartedId;
        TestCaseStepId = testCaseStepId;
        TestRunHookStartedId = testRunHookStartedId;
        _publisher = publisher;
    }

    public async Task ProcessEvent(AttachmentAddedEvent attachmentAddedEvent)
    {
        FilePath = attachmentAddedEvent.FilePath;
        Timestamp = attachmentAddedEvent.Timestamp;
        await _publisher.PublishAsync(Envelope.Create(_messageFactory.ToAttachment(this)));
    }
}