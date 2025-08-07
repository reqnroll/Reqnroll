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
        TestCaseTracker testCaseTracker,
        IMessagePublisher picklePublisher = null)
    {
        return new TestCaseExecutionTracker(
            parentTracker,
            attemptId,
            idGenerator.GetNewId(),
            testCaseId,
            testCaseTracker,
            messageFactory,
            picklePublisher ?? publisher,
            stepTrackerFactory);
    }
}