using Reqnroll.Bindings;
using Reqnroll.Events;
using System.Collections.Generic;

namespace Reqnroll.Formatters.ExecutionTracking;

public interface IFeatureExecutionTrackerFactory
{
    IFeatureExecutionTracker CreateFeatureTracker(FeatureStartedEvent featureStartedEvent, string testRunStartedId, IReadOnlyDictionary<IBinding, string> stepDefinitionIdsByMethod);
}