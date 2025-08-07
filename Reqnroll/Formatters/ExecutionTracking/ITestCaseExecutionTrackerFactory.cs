using Reqnroll.Formatters.PubSub;

namespace Reqnroll.Formatters.ExecutionTracking;

public interface ITestCaseExecutionTrackerFactory
{
    TestCaseExecutionTracker CreateTestCaseExecutionTracker(
        IPickleExecutionTracker parentTracker,
        int attemptId,
        string testCaseId,
        TestCaseTracker testCaseTracker,
        IMessagePublisher picklePublisher);
}
