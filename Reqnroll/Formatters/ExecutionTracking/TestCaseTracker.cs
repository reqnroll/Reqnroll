using System.Collections.Generic;
using System.Linq;
using Reqnroll.Bindings;

namespace Reqnroll.Formatters.ExecutionTracking;

/// <summary>
/// Tracks the information needed for a Cucumber Messages "test case", that is a pickle with binding information,
/// so it captures for every step and hook the related step definitions.
/// The ledger is populated lazily as steps/hooks are first seen across execution attempts; an entry for a
/// step/hook reached only on a later (retry) attempt is appended when it is first encountered.
/// </summary>
public class TestCaseTracker(string testCaseId, string pickleId, IPickleExecutionTracker parentTracker)
{
    public IPickleExecutionTracker ParentTracker { get; } = parentTracker;
    public string PickleId { get; } = pickleId;
    public string TestCaseId { get; } = testCaseId;

    public List<StepTrackerBase> Steps { get; } = new();

    internal string FindStepDefinitionIdByBindingKey(IBinding binding)
    {
        return ParentTracker.StepDefinitionsByBinding[binding];
    }

    /// <summary>
    /// Returns the ledger entry for the <paramref name="occurrence"/>-th execution of pickle step
    /// <paramref name="pickleStepId"/> within an attempt, creating and appending it on first sight.
    /// This is stable across retries: a step first reached on a later (less-truncated) attempt is
    /// simply appended, and a step already seen on an earlier attempt reuses its existing entry (and id).
    /// </summary>
    public TestStepTracker GetOrCreateTestStepTracker(string pickleStepId, int occurrence)
    {
        var existing = Steps.OfType<TestStepTracker>()
            .FirstOrDefault(sd => sd.PickleStepId == pickleStepId && sd.Occurrence == occurrence);
        if (existing != null)
            return existing;

        var stepTracker = new TestStepTracker(ParentTracker.IdGenerator.GetNewId(), pickleStepId, occurrence, this);
        Steps.Add(stepTracker);
        return stepTracker;
    }

    /// <summary>
    /// Hook counterpart of <see cref="GetOrCreateTestStepTracker"/>, keyed on (<paramref name="hookId"/>, <paramref name="occurrence"/>).
    /// The occurrence index distinguishes repeated firings of the same hook binding (e.g. a BeforeStep/AfterStep
    /// hook that runs once per step) so that each firing maps to its own test step.
    /// </summary>
    public HookStepTracker GetOrCreateHookStepTracker(string hookId, int occurrence)
    {
        var existing = Steps.OfType<HookStepTracker>()
            .FirstOrDefault(sd => sd.HookId == hookId && sd.Occurrence == occurrence);
        if (existing != null)
            return existing;

        var hookStepTracker = new HookStepTracker(ParentTracker.IdGenerator.GetNewId(), hookId, occurrence);
        Steps.Add(hookStepTracker);
        return hookStepTracker;
    }
}