using System;
using System.IO;
using Reqnroll.Infrastructure;
using Reqnroll.Tracing;

namespace Reqnroll.TUnit.ReqnrollPlugin;

public class TUnitAttachmentHandler : ReqnrollAttachmentHandler
{
    public TUnitAttachmentHandler(ITraceListener traceListener) : base(traceListener)
    {
    }

    public override void AddAttachment(string filePath)
    {
        try
        {
            var artifact = new Artifact
            {
                File = new FileInfo(filePath),
                DisplayName = filePath,
            };

            TestContext.Current?.AddArtifact(artifact);
        }
        catch (Exception)
        {
            base.AddAttachment(filePath);
        }
    }
}
