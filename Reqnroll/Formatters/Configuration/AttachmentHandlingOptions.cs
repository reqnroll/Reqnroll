using System;
using System.Collections.Generic;
using System.Text;

namespace Reqnroll.Formatters.Configuration;

public record AttachmentHandlingOptions(
    AttachmentHandlingOption AttachmentHandlingOption,
    string ExternalAttachmentsStoragePath
);