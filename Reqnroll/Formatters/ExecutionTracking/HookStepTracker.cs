namespace Reqnroll.Formatters.ExecutionTracking;

/// <summary>
/// Tracks the information needed for a Cucumber Messages "hook step", that is a hook with binding information.
/// Hook step ledger entries are created lazily across execution attempts and are keyed by (hookId, occurrence).
/// </summary>
public class HookStepTracker(string testStepId, string hookId, int occurrence) : StepTrackerBase(testStepId, occurrence)
{
    public string HookId { get; } = hookId;
}