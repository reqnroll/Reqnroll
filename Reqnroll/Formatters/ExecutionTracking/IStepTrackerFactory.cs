using Gherkin.CucumberMessages;
using Reqnroll.Formatters.PubSub;

namespace Reqnroll.Formatters.ExecutionTracking;

public interface IStepTrackerFactory
{
    TestStepExecutionTracker CreateTestStepExecutionTracker(TestCaseExecutionTracker parentTracker, IMessagePublisher picklePublisher = null);
    HookStepExecutionTracker CreateHookStepExecutionTracker(TestCaseExecutionTracker parentTracker, IMessagePublisher picklePublisher = null);
    AttachmentTracker CreateAttachmentTracker(string testRunStartedId, string testCaseStartedId, string testCaseStepId, string testRunHookStartedId, IMessagePublisher picklePublisher = null);
    OutputMessageTracker CreateOutputMessageTracker(string testRunStartedId, string testCaseStartedId, string testCaseStepId, string outputIssuedByHookStartedId, IMessagePublisher picklePublisher = null);
}
