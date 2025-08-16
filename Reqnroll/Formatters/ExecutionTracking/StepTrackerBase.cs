namespace Reqnroll.Formatters.ExecutionTracking;

public abstract class StepTrackerBase(string testStepId)
{
    public string TestStepId { get; } = testStepId;
}
