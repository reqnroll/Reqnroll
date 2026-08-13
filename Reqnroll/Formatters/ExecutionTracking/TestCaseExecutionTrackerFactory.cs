using Gherkin.CucumberMessages;
using Reqnroll.Formatters.PayloadProcessing.Cucumber;
using Reqnroll.Formatters.PubSub;

namespace Reqnroll.Formatters.ExecutionTracking;

public class TestCaseExecutionTrackerFactory(IIdGenerator idGenerator, ICucumberMessageFactory messageFactory, IMessagePublisher publisher, IStepTrackerFactory stepTrackerFactory) : ITestCaseExecutionTrackerFactory
{
    public TestCaseExecutionTracker CreateTestCaseExecutionTracker(
        IPickleExecutionTracker parentTracker,
        int attemptId,
        string testCaseId,
        IMessagePublisher picklePublisher = null)
    {
        return new TestCaseExecutionTracker(
            parentTracker,
            attemptId,
            idGenerator.GetNewId(),
            testCaseId,
            messageFactory,
            picklePublisher ?? publisher,
            stepTrackerFactory);
    }
}