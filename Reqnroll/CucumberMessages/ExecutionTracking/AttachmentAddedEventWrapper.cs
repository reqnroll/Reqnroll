using Io.Cucumber.Messages.Types;
using Reqnroll.CucumberMessages.PayloadProcessing.Cucumber;
using Reqnroll.Events;
using System.Collections.Generic;

namespace Reqnroll.CucumberMessages.ExecutionTracking
{
    // This class acts as an addendum to AttachmentAddedEvent and provides the ability to convey which Pickle, TestCase, and TestStep were responsible for the Attachment being added.
    public class AttachmentAddedEventWrapper : IGenerateMessage
    {
        public AttachmentAddedEventWrapper(AttachmentAddedEvent attachmentAddedEvent, string testRunStartedId, string testCaseStartedId, string testCaseStepId)
        {
            AttachmentAddedEvent = attachmentAddedEvent;
            TestRunStartedId = testRunStartedId;
            TestCaseStartedId = testCaseStartedId;
            TestCaseStepId = testCaseStepId;
        }

        public AttachmentAddedEvent AttachmentAddedEvent { get; }
        public string TestCaseStartedId { get; set; }
        public string TestCaseStepId { get; set; }
        public string TestRunStartedId {get; set; }

        public IEnumerable<Envelope> GenerateFrom(ExecutionEvent executionEvent)
        {
             return [ Envelope.Create(CucumberMessageFactory.ToAttachment(this))];
        }
    }
}