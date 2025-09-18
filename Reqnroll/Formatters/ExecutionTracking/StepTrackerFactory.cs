using Gherkin.CucumberMessages;
using Reqnroll.Formatters.PayloadProcessing.Cucumber;
using Reqnroll.Formatters.PubSub;
using System.Runtime.CompilerServices;

namespace Reqnroll.Formatters.ExecutionTracking;

public class StepTrackerFactory(ICucumberMessageFactory messageFactory, IMessagePublisher publisher, IIdGenerator idGenerator) : IStepTrackerFactory
{
    public TestStepExecutionTracker CreateTestStepExecutionTracker(TestCaseExecutionTracker parentTracker, IMessagePublisher picklePublisher = null)
    {
        return new TestStepExecutionTracker(parentTracker, messageFactory, picklePublisher ?? publisher, idGenerator);
    }
    public HookStepExecutionTracker CreateHookStepExecutionTracker(TestCaseExecutionTracker parentTracker, IMessagePublisher picklePublisher = null)
    {
        return new HookStepExecutionTracker(parentTracker, messageFactory, picklePublisher ?? publisher);
    }
    public AttachmentTracker CreateAttachmentTracker(string testRunStartedId, string testCaseStartedId, string testCaseStepId, string testRunHookStartedId, IMessagePublisher picklePublisher = null)
    {
        return new AttachmentTracker(testRunStartedId, testCaseStartedId, testCaseStepId, testRunHookStartedId, messageFactory, picklePublisher ?? publisher);
    }
    public OutputMessageTracker CreateOutputMessageTracker(string testRunStartedId, string testCaseStartedId, string testCaseStepId, string outputIssuedByHookStartedId, IMessagePublisher picklePublisher = null)
    {
        return new OutputMessageTracker(testRunStartedId, testCaseStartedId, testCaseStepId, outputIssuedByHookStartedId, messageFactory, picklePublisher ?? publisher);
    }
}
