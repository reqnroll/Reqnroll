using Reqnroll.Events;
using System.Collections.Generic;
using System.Linq;
using Reqnroll.Formatters.PayloadProcessing.Cucumber;

namespace Reqnroll.Formatters.ExecutionTracking;

/// <summary>
/// Tracks the information needed for a Cucumber Messages "test case", that is a pickle with binding information,
/// so it captures for every step and hook the related step definitions.
/// The test case needs to be built upon the first execution attempt of a pickle.
/// </summary>
public class TestCaseTracker(string testCaseId, string pickleId, IPickleExecutionTracker parentTracker, ICucumberMessageFactory messageFactory)
{
    public IPickleExecutionTracker ParentTracker { get; } = parentTracker;
    public string PickleId { get; } = pickleId;
    public string TestCaseId { get; } = testCaseId;

    public List<StepTracker> Steps { get; } = new();

    internal string FindStepDefinitionIdByBingingKey(string canonicalizedStepPattern)
    {
        return ParentTracker.StepDefinitionsByMethodSignature[canonicalizedStepPattern];
    }

    public TestStepTracker GetTestStepTrackerByPickleId(string pickleId)
    {
        return Steps.OfType<TestStepTracker>().FirstOrDefault(sd => sd.PickleStepId == pickleId);
    }

    public HookStepTracker GetHookStepTrackerByHookId(string hookId)
    {
        return Steps.OfType<HookStepTracker>().First(sd => sd.HookId == hookId);
    }

    public void ProcessEvent(StepStartedEvent stepStartedEvent)
    {
        var pickleStepId = stepStartedEvent.StepContext.StepInfo.PickleStepId;
        var testStepId = ParentTracker.IdGenerator.GetNewId();
        var stepTracker = new TestStepTracker(testStepId, pickleStepId, this, messageFactory);
        Steps.Add(stepTracker);
        stepTracker.ProcessEvent(stepStartedEvent);
    }

    public void ProcessEvent(HookBindingStartedEvent hookBindingStartedEvent)
    {
        var hookBindingSignature = messageFactory.CanonicalizeHookBinding(hookBindingStartedEvent.HookBinding);
        var hookId = ParentTracker.StepDefinitionsByMethodSignature[hookBindingSignature];

        var testStepId = ParentTracker.IdGenerator.GetNewId();
        var hookStepTracker = new HookStepTracker(testStepId, hookId, this, messageFactory);
        Steps.Add(hookStepTracker);
    }
}