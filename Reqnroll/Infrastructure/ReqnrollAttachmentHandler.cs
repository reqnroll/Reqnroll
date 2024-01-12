using Reqnroll.Tracing;

namespace Reqnroll.Infrastructure
{
    public class ReqnrollAttachmentHandler : IReqnrollAttachmentHandler
    {
        private readonly ITraceListener _traceListener;

        protected ReqnrollAttachmentHandler(ITraceListener traceListener)
        {
            _traceListener = traceListener;
        }

        public virtual void AddAttachment(string filePath)
        {
            _traceListener.WriteToolOutput($"Attachment '{filePath}' added (not forwarded to the test runner).");
        }
    }
}
