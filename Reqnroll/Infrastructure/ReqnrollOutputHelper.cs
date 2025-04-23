using Reqnroll.Events;
using Reqnroll.Tracing;

namespace Reqnroll.Infrastructure
{
    public class ReqnrollOutputHelper : IReqnrollOutputHelper
    {
        private readonly ITestThreadExecutionEventPublisher _testThreadExecutionEventPublisher;
        private readonly ITraceListener _traceListener;
        private readonly IReqnrollAttachmentHandler _reqnrollAttachmentHandler;
        private readonly IContextManager _contextManager;

        public ReqnrollOutputHelper(ITestThreadExecutionEventPublisher testThreadExecutionEventPublisher, ITraceListener traceListener, IReqnrollAttachmentHandler reqnrollAttachmentHandler, IContextManager contextManager)
        {
            _testThreadExecutionEventPublisher = testThreadExecutionEventPublisher;
            _traceListener = traceListener;
            _reqnrollAttachmentHandler = reqnrollAttachmentHandler;
            _contextManager = contextManager;
        }

        public void WriteLine(string message)
        {
            var featureInfo = _contextManager.FeatureContext?.FeatureInfo;
            var scenarioInfo = _contextManager.ScenarioContext?.ScenarioInfo;

            _testThreadExecutionEventPublisher.PublishEventAsync(new OutputAddedEvent(message, featureInfo, scenarioInfo));
            _traceListener.WriteTestOutput(message);
        }

        public void WriteLine(string format, params object[] args)
        {
            WriteLine(string.Format(format, args));
        }

        public void AddAttachment(string filePath)
        {
            var featureInfo = _contextManager.FeatureContext.FeatureInfo;
            var scenarioInfo = _contextManager.ScenarioContext?.ScenarioInfo;

            _testThreadExecutionEventPublisher.PublishEventAsync(new AttachmentAddedEvent(filePath, featureInfo, scenarioInfo));
            _reqnrollAttachmentHandler.AddAttachment(filePath);
        }
    }
}
