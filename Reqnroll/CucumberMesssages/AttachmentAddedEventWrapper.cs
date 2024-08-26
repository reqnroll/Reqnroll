using Reqnroll.Events;

namespace Reqnroll.CucumberMesssages
{
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