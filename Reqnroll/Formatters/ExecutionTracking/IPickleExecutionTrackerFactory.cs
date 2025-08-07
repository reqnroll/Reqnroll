namespace Reqnroll.Formatters.ExecutionTracking;

public interface IPickleExecutionTrackerFactory
{
    IPickleExecutionTracker CreatePickleTracker(IFeatureExecutionTracker featureExecutionTracker, string pickleId);
}