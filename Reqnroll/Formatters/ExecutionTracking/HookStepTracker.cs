namespace Reqnroll.Formatters.ExecutionTracking;

/// <summary>
/// Tracks the information needed for a Cucumber Messages "hook step", that is a hook with binding information.
/// The hook step needs to be built upon the first execution attempt of a pickle.
/// </summary>
public class HookStepTracker(string testStepId, string hookId) : StepTrackerBase(testStepId)
{
    public string HookId { get; } = hookId;
}