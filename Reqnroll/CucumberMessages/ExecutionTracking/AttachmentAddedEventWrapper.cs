using Io.Cucumber.Messages.Types;
using Reqnroll.CucumberMessages.PayloadProcessing.Cucumber;
using Reqnroll.Events;
using System.Collections.Generic;

namespace Reqnroll.CucumberMessages.ExecutionTracking
{
    // This class acts as an addendum to AttachmentAddedEvent and provides the ability to convey which Pickle, TestCase, and TestStep were responsible for the Attachment being added.
    public class AttachmentAddedEventWrapper : IGenerateMessage
    {
        internal AttachmentAddedEventWrapper(AttachmentAddedEvent attachmentAddedEvent, string testRunStartedId, string testCaseStartedId, string testCaseStepId, ICucumberMessageFactory messageFactory)
        {
            AttachmentAddedEvent = attachmentAddedEvent;
            TestRunStartedId = testRunStartedId;
            TestCaseStartedId = testCaseStartedId;
            TestCaseStepId = testCaseStepId;
            _messageFactory = messageFactory;
        }

        internal AttachmentAddedEvent AttachmentAddedEvent { get; }
        internal string TestCaseStartedId { get; }
        internal string TestCaseStepId { get; }

        internal ICucumberMessageFactory _messageFactory;

        internal string TestRunStartedId { get; }

        public IEnumerable<Envelope> GenerateFrom(ExecutionEvent executionEvent)
        {
            return [Envelope.Create(_messageFactory.ToAttachment(this))];
        }
    }
}