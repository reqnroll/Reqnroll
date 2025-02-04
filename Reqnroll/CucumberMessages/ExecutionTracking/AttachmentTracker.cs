using Io.Cucumber.Messages.Types;
using Reqnroll.Events;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;

namespace Reqnroll.CucumberMessages.ExecutionTracking
{
    public class AttachmentTracker
    {

        private List<ExecutionEvent> _attacheds = new();

        public void RecordAttachment(AttachmentAddedEvent attachmentEvent)
        {
            _attacheds.Add(attachmentEvent);
        }

        public void RecordOutput(OutputAddedEvent outputAddedEvent)
        {
            _attacheds.Add(outputAddedEvent);
        }

        public ExecutionEvent FindMatchingAttachment(Attachment a)
        {
            if (a.FileName == null) // ... meaning this is an Output attachment, not a file attachment
            {
                var o = _attacheds.OfType<OutputAddedEvent>().Where(e => e.Text == a.Body).First();
                _attacheds.Remove(o);
                return o;
            }
            else
            {
                var r = _attacheds.OfType<AttachmentAddedEvent>().Where(e => a.FileName == Path.GetFileName(e.FilePath)).First();
                _attacheds.Remove(r);
                return r;
            }
        }
    }
}
