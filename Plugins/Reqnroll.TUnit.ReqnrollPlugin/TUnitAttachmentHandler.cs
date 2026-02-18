using System;
using System.IO;
using Reqnroll.Infrastructure;
using Reqnroll.Tracing;

namespace Reqnroll.TUnit.ReqnrollPlugin;

/// <summary>
/// Handles test attachments for TUnit test framework.
/// </summary>
public class TUnitAttachmentHandler : ReqnrollAttachmentHandler
{
    /// <summary>
    /// Initializes a new instance of the <see cref="TUnitAttachmentHandler"/> class.
    /// </summary>
    /// <param name="traceListener">The trace listener for fallback output.</param>
    public TUnitAttachmentHandler(ITraceListener traceListener) : base(traceListener)
    {
    }

    /// <inheritdoc />
    public override void AddAttachment(string filePath)
    {
        try
        {
            var artifact = new Artifact
            {
                File = new FileInfo(filePath),
                DisplayName = filePath,
            };

            TestContext.Current?.Output.AttachArtifact(artifact);
        }
        catch (Exception)
        {
            base.AddAttachment(filePath);
        }
    }
}
