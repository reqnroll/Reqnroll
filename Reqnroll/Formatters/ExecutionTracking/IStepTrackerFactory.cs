using Reqnroll.Formatters.PubSub;

namespace Reqnroll.Formatters.ExecutionTracking;

public interface IStepTrackerFactory
{
    TestStepExecutionTracker CreateTestStepExecutionTracker(TestCaseExecutionTracker parentTracker, IPublishMessage picklePublisher = null);
    HookStepExecutionTracker CreateHookStepExecutionTracker(TestCaseExecutionTracker parentTracker, IPublishMessage picklePublisher = null);
    AttachmentTracker CreateAttachmentTracker(string testRunStartedId, string testCaseStartedId, string testCaseStepId, string testRunHookStartedId, IPublishMessage picklePublisher = null);
    OutputMessageTracker CreateOutputMessageTracker(string testRunStartedId, string testCaseStartedId, string testCaseStepId, string outputIssuedByHookStartedId, IPublishMessage picklePublisher = null);
}
