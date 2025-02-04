using Reqnroll.Events;
using Reqnroll.Tracing;

namespace Reqnroll.Infrastructure
{
    public class ReqnrollOutputHelper : IReqnrollOutputHelper
    {
        private readonly ITestThreadExecutionEventPublisher _testThreadExecutionEventPublisher;
        private readonly ITraceListener _traceListener;
        private readonly IReqnrollAttachmentHandler _reqnrollAttachmentHandler;
        private readonly IContextManager contextManager;

        public ReqnrollOutputHelper(ITestThreadExecutionEventPublisher testThreadExecutionEventPublisher, ITraceListener traceListener, IReqnrollAttachmentHandler reqnrollAttachmentHandler, IContextManager contextManager)
        {
            _testThreadExecutionEventPublisher = testThreadExecutionEventPublisher;
            _traceListener = traceListener;
            _reqnrollAttachmentHandler = reqnrollAttachmentHandler;
            this.contextManager = contextManager;
        }

        public void WriteLine(string message)
        {
            var featureInfo = contextManager.FeatureContext?.FeatureInfo;
            var scenarioInfo = contextManager.ScenarioContext?.ScenarioInfo;

            _testThreadExecutionEventPublisher.PublishEvent(new OutputAddedEvent(message, featureInfo, scenarioInfo));
            _traceListener.WriteToolOutput(message);
        }

        public void WriteLine(string format, params object[] args)
        {
            WriteLine(string.Format(format, args));
        }

        public void AddAttachment(string filePath)
        {
            var featureInfo = contextManager.FeatureContext.FeatureInfo;
            var scenarioInfo = contextManager.ScenarioContext?.ScenarioInfo;

            _testThreadExecutionEventPublisher.PublishEvent(new AttachmentAddedEvent(filePath, featureInfo, scenarioInfo));
            _reqnrollAttachmentHandler.AddAttachment(filePath);
        }
    }
}
