using Io.Cucumber.Messages.Types;
using Reqnroll.Formatters.PayloadProcessing.Cucumber;
using Reqnroll.Events;
using System;
using System.Threading.Tasks;
using Reqnroll.Formatters.PubSub;

namespace Reqnroll.Formatters.ExecutionTracking;

/// <summary>
/// Tracks the logging of an output message. The <see cref="TestCaseStartedId"/> and <see cref="TestCaseStepId"/> are optional,
/// they are only filled out if the attachment is added in a scope of a scenario or step/hook execution.
/// </summary>
public class OutputMessageTracker 
{
    private readonly ICucumberMessageFactory _messageFactory;

    public string TestRunStartedId { get; }
    public string TestCaseStartedId { get; }
    public string TestCaseStepId { get; }
    public string TestRunHookStartedId { get; }

    private readonly IMessagePublisher _publisher;

    public string Text { get; private set; }
    public DateTime Timestamp { get; private set; }

    internal OutputMessageTracker(string testRunStartedId, string testCaseStartedId, string testCaseStepId, string outputIssuedByHookStartedId, ICucumberMessageFactory messageFactory, IMessagePublisher publisher)
    {
        _messageFactory = messageFactory;

        TestRunStartedId = testRunStartedId;
        TestCaseStartedId = testCaseStartedId;
        TestCaseStepId = testCaseStepId;
        TestRunHookStartedId = outputIssuedByHookStartedId;
        _publisher = publisher;
    }

    public async Task ProcessEvent(OutputAddedEvent outputAddedEvent)
    {
        Text = outputAddedEvent.Text;
        Timestamp = outputAddedEvent.Timestamp;

        await _publisher.PublishAsync(Envelope.Create(_messageFactory.ToAttachment(this)));
    }
}