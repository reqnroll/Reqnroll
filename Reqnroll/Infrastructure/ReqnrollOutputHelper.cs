using Reqnroll.Events;
using Reqnroll.Tracing;

namespace Reqnroll.Infrastructure
{
    /// <summary>
    /// Provides methods for writing test output and adding attachments during test execution.
    /// </summary>
    /// <remarks>This class is designed to facilitate the logging of test output and the management of
    /// test-related attachments. It integrates with the test execution event publisher, trace listener, and attachment
    /// handler to ensure that output and attachments are properly handled and recorded.
    /// 
    /// This class needs further refactoring to make it asynchronous so that the TestThreadExecutionEventPublisher can be
    /// invoked asynchronously.
    /// </remarks>
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
