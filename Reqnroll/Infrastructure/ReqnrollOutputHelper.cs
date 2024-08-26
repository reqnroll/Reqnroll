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
            var featureName = contextManager.FeatureContext.FeatureInfo?.Title;
            var scenarioName = contextManager.ScenarioContext.ScenarioInfo?.Title;
            var stepText = contextManager.StepContext.StepInfo?.Text;

            _testThreadExecutionEventPublisher.PublishEvent(new OutputAddedEvent(message, featureName, scenarioName, stepText));
            _traceListener.WriteToolOutput(message);
        }

        public void WriteLine(string format, params object[] args)
        {
            WriteLine(string.Format(format, args));
        }

        public void AddAttachment(string filePath)
        {
            var featureName = contextManager.FeatureContext.FeatureInfo?.Title;
            var scenarioName = contextManager.ScenarioContext.ScenarioInfo?.Title;
            var stepText = contextManager.StepContext.StepInfo?.Text;
            _testThreadExecutionEventPublisher.PublishEvent(new AttachmentAddedEvent(filePath, featureName, scenarioName, stepText));
            _reqnrollAttachmentHandler.AddAttachment(filePath);
        }
    }
}
