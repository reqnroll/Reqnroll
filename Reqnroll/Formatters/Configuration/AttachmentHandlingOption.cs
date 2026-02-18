using System;
using System.Collections.Generic;
using System.Text;

namespace Reqnroll.Formatters.Configuration;

[Flags]
public enum AttachmentHandlingOption
{
    None = 1,       // Attachment events are ignored and not included in the Cucumber Messages output
    Embed = 2,      // Attachments are embedded directly into the Cucumber Messages output as base64-encoded data
    External = 4    // Attachments are referenced as external files in the Cucumber Messages output
}
