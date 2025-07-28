namespace Reqnroll.Formatters.ExecutionTracking;

public abstract class StepTracker(string testStepId)
{
    public string TestStepId { get; } = testStepId;
}
