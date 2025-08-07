using Gherkin.CucumberMessages;
using Reqnroll.Bindings;
using Reqnroll.Events;
using Reqnroll.Formatters.PubSub;
using System.Collections.Generic;

namespace Reqnroll.Formatters.ExecutionTracking;

public class FeatureExecutionTrackerFactory(IIdGenerator idGenerator, IPickleExecutionTrackerFactory pickleExecutionTrackerFactory, IMessagePublisher publisher) : IFeatureExecutionTrackerFactory
{
    public IFeatureExecutionTracker CreateFeatureTracker(FeatureStartedEvent featureStartedEvent, string testRunStartedId, IReadOnlyDictionary<IBinding, string> stepDefinitionIdsByMethod)
    {
        return new FeatureExecutionTracker(featureStartedEvent, testRunStartedId, stepDefinitionIdsByMethod, idGenerator, pickleExecutionTrackerFactory, publisher);
    }
}
