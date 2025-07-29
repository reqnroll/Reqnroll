using Reqnroll.Formatters.PayloadProcessing.Cucumber;
using Reqnroll.Formatters.PubSub;

namespace Reqnroll.Formatters.ExecutionTracking;

public class StepTrackerFactory : IStepTrackerFactory
{
    private readonly ICucumberMessageFactory _messageFactory;
    private readonly IPublishMessage _publisher;
    public StepTrackerFactory(ICucumberMessageFactory messageFactory, IPublishMessage publisher)
    {
        _messageFactory = messageFactory;
        _publisher = publisher;
    }
    public TestStepExecutionTracker CreateTestStepExecutionTracker(TestCaseExecutionTracker parentTracker, IPublishMessage picklePublisher = null)
    {
        return new TestStepExecutionTracker(parentTracker, _messageFactory, picklePublisher ?? _publisher);
    }
    public HookStepExecutionTracker CreateHookStepExecutionTracker(TestCaseExecutionTracker parentTracker, IPublishMessage picklePublisher = null)
    {
        return new HookStepExecutionTracker(parentTracker, _messageFactory, picklePublisher ?? _publisher);
    }
    public AttachmentTracker CreateAttachmentTracker(string testRunStartedId, string testCaseStartedId, string testCaseStepId, string testRunHookStartedId, IPublishMessage picklePublisher = null)
    {
        return new AttachmentTracker(testRunStartedId, testCaseStartedId, testCaseStepId, testRunHookStartedId, _messageFactory, picklePublisher ?? _publisher);
    }
    public OutputMessageTracker CreateOutputMessageTracker(string testRunStartedId, string testCaseStartedId, string testCaseStepId, string outputIssuedByHookStartedId, IPublishMessage picklePublisher = null)
    {
        return new OutputMessageTracker(testRunStartedId, testCaseStartedId, testCaseStepId, outputIssuedByHookStartedId, _messageFactory, picklePublisher ?? _publisher);
    }
}
