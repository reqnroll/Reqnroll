using Gherkin.CucumberMessages;
using Reqnroll.Formatters.PayloadProcessing.Cucumber;
using Reqnroll.Formatters.PubSub;
using Reqnroll.Time;

namespace Reqnroll.Formatters.ExecutionTracking;

public class PickleExecutionTrackerFactory(IIdGenerator idGenerator, IClock clock, ICucumberMessageFactory messageFactory, ITestCaseExecutionTrackerFactory testCaseExecutionTrackerFactory, IMessagePublisher publisher) : IPickleExecutionTrackerFactory
{
    public IPickleExecutionTracker CreatePickleTracker(IFeatureExecutionTracker featureExecutionTracker, string pickleId)
    {
        return new PickleExecutionTracker(pickleId, 
            featureExecutionTracker.TestRunStartedId, 
            featureExecutionTracker.FeatureName, 
            featureExecutionTracker.Enabled, 
            idGenerator, 
            featureExecutionTracker.StepDefinitionsByBinding, 
            clock.GetNowDateAndTime(), 
            messageFactory, 
            testCaseExecutionTrackerFactory, 
            new PickleExecutionTracker.OrderFixingMessagePublisher(publisher));
    }
}
