namespace Reqnroll.Formatters.ExecutionTracking;

public abstract class StepTrackerBase(string testStepId, int occurrence)
{
    public string TestStepId { get; } = testStepId;

    /// <summary>
    /// The 1-based index of this step/hook among occurrences of the <i>same identity</i>
    /// (PickleStepId for steps, HookId for hooks) within a single execution attempt.
    /// This keeps the ledger entry identity stable across retries even when an earlier
    /// attempt aborted before this occurrence ran.
    /// </summary>
    public int Occurrence { get; } = occurrence;
}
