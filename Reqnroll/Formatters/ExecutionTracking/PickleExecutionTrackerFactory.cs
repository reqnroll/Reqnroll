using Gherkin.CucumberMessages;
using Reqnroll.Bindings;
using Reqnroll.Formatters.PayloadProcessing.Cucumber;
using Reqnroll.Formatters.PubSub;
using Reqnroll.Time;
using System;
using System.Collections.Generic;
using System.Text;

namespace Reqnroll.Formatters.ExecutionTracking;

public class PickleExecutionTrackerFactory(IIdGenerator idGenerator, IClock clock, ICucumberMessageFactory messageFactory, ITestCaseExecutionTrackerFactory testCaseExecutionTrackerFactory, IPublishMessage publisher) : IPickleExecutionTrackerFactory
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
            new BufferedMessagePublisher(publisher));
    }
}
