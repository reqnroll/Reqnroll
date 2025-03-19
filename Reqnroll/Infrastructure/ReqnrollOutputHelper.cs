using Reqnroll.Events;
using Reqnroll.Tracing;

namespace Reqnroll.Infrastructure
{
    public class ReqnrollOutputHelper : IReqnrollOutputHelper
    {
        private readonly ITestThreadExecutionEventPublisher _testThreadExecutionEventPublisher;
        private readonly ITraceListener _traceListener;
        private readonly IReqnrollAttachmentHandler _reqnrollAttachmentHandler;

        public ReqnrollOutputHelper(ITestThreadExecutionEventPublisher testThreadExecutionEventPublisher, ITraceListener traceListener, IReqnrollAttachmentHandler reqnrollAttachmentHandler)
        {
            _testThreadExecutionEventPublisher = testThreadExecutionEventPublisher;
            _traceListener = traceListener;
            _reqnrollAttachmentHandler = reqnrollAttachmentHandler;
        }

        public void WriteLine(string message)
        {
            _testThreadExecutionEventPublisher.PublishEvent(new OutputAddedEvent(message));
            _traceListener.WriteTestOutput(message);
        }

        public void WriteLine(string format, params object[] args)
        {
            WriteLine(string.Format(format, args));
        }

        public void AddAttachment(string filePath)
        {
            _testThreadExecutionEventPublisher.PublishEvent(new AttachmentAddedEvent(filePath));
            _reqnrollAttachmentHandler.AddAttachment(filePath);
        }
    }
}
