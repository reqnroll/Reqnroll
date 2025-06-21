using Io.Cucumber.Messages.Types;
using Reqnroll.Formatters.PayloadProcessing.Cucumber;
using Reqnroll.Events;
using System.Collections.Generic;

namespace Reqnroll.Formatters.ExecutionTracking;

/// <summary>
/// Tracks the logging of an output message. The <see cref="TestCaseStartedId"/> and <see cref="TestCaseStepId"/> are optional,
/// they are only filled out if the attachment is added in a scope of a scenario or step/hook execution.
/// </summary>
public class OutputMessageTracker : IGenerateMessage
{
    private readonly ICucumberMessageFactory _messageFactory;

    public string TestRunStartedId { get; }
    public string TestCaseStartedId { get; }
    public string TestCaseStepId { get; }

    public string Text { get; private set; }

    internal OutputMessageTracker(string testRunStartedId, string testCaseStartedId, string testCaseStepId, ICucumberMessageFactory messageFactory)
    {
        _messageFactory = messageFactory;

        TestRunStartedId = testRunStartedId;
        TestCaseStartedId = testCaseStartedId;
        TestCaseStepId = testCaseStepId;
    }

    IEnumerable<Envelope> IGenerateMessage.GenerateFrom(ExecutionEvent executionEvent)
    {
        return [Envelope.Create(_messageFactory.ToAttachment(this))];
    }

    public void ProcessEvent(OutputAddedEvent outputAddedEvent)
    {
        Text = outputAddedEvent.Text;
    }
}