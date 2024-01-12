using System;
using NUnit.Framework;
using Reqnroll.Infrastructure;
using Reqnroll.Tracing;

namespace Reqnroll.NUnit.ReqnrollPlugin
{
    public class NUnitAttachmentHandler : ReqnrollAttachmentHandler
    {
        public NUnitAttachmentHandler(ITraceListener traceListener) : base(traceListener)
        {
        }

        public override void AddAttachment(string filePath)
        {
            try
            {
                TestContext.AddTestAttachment(filePath);
            }
            catch (Exception)
            {
                base.AddAttachment(filePath);
            }
        }
    }
}
