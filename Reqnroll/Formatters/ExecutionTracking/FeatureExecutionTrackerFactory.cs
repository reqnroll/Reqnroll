using Gherkin.CucumberMessages;
using Reqnroll.Bindings;
using Reqnroll.Events;
using Reqnroll.Formatters.PayloadProcessing.Cucumber;
using Reqnroll.Formatters.PubSub;
using Reqnroll.Time;
using System;
using System.Collections.Generic;
using System.Text;

namespace Reqnroll.Formatters.ExecutionTracking;

public class FeatureExecutionTrackerFactory(IIdGenerator idGenerator, IClock clock, ICucumberMessageFactory messageFactory, IPickleExecutionTrackerFactory pickleExecutionTrackerFactory, IPublishMessage publisher) : IFeatureExecutionTrackerFactory
{
    public IFeatureExecutionTracker CreateFeatureTracker(FeatureStartedEvent featureStartedEvent, string testRunStartedId, IReadOnlyDictionary<IBinding, string> stepDefinitionIdsByMethod)
    {
        return new FeatureExecutionTracker(featureStartedEvent, testRunStartedId, stepDefinitionIdsByMethod, idGenerator, clock, messageFactory, pickleExecutionTrackerFactory, publisher);
    }
}
