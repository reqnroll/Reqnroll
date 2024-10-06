using Reqnroll.Events;

namespace Reqnroll.CucumberMessages.RuntimeSupport
{
    // This class acts as an addendum to AttachmentAddedEvent and provides the ability to convey which Pickle, TestCase, and TestStep were responsible for the Attachment being added.
    internal class AttachmentAddedEventWrapper : ExecutionEvent
    {
        public AttachmentAddedEventWrapper(AttachmentAddedEvent attachmentAddedEvent, string pickleStepId)
        {
            AttachmentAddedEvent = attachmentAddedEvent;
            PickleStepID = pickleStepId;
        }

        public AttachmentAddedEvent AttachmentAddedEvent { get; }
        public string PickleStepID { get; }
        public string TestCaseStartedID { get; set; }
        public string TestCaseStepID { get; set; }
    }
}