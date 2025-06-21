using Io.Cucumber.Messages.Types;
using Reqnroll.Events;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;

namespace Reqnroll.Formatters.ExecutionTracking;

/// <summary>
/// Tracks output events, like added attachments and output messages.
/// </summary>
public class OutputEventsTracker
{
    private readonly List<ExecutionEvent> _attachments = new();

    public void ProcessEvent(AttachmentAddedEvent attachmentEvent)
    {
        _attachments.Add(attachmentEvent);
    }

    public void ProcessEvent(OutputAddedEvent outputAddedEvent)
    {
        _attachments.Add(outputAddedEvent);
    }

    public DateTime GetTimestampForMatchingAttachment(Attachment a)
    {
        //TODO: find a better way to get timestamp for an Attachment see also https://github.com/cucumber/messages/issues/304

        ExecutionEvent foundEvent;
        if (a.FileName == null) // ... meaning this is an Output attachment, not a file attachment
        {
            foundEvent = _attachments.OfType<OutputAddedEvent>().FirstOrDefault(e => e.Text == a.Body);
        }
        else
        {
            foundEvent = _attachments.OfType<AttachmentAddedEvent>().FirstOrDefault(e => a.FileName == Path.GetFileName(e.FilePath));
        }

        if (foundEvent != null)
        {
            _attachments.Remove(foundEvent);
            return foundEvent.Timestamp;
        }

        return default;
    }
}