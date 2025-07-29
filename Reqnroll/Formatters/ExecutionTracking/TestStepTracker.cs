using Reqnroll.Bindings;
using Reqnroll.Events;
using System.Collections.Generic;
using System.Linq;

namespace Reqnroll.Formatters.ExecutionTracking;

/// <summary>
/// Tracks the information needed for a Cucumber Messages "test case step", that is a step with binding information.
/// The test case step needs to be built upon the first execution attempt of a pickle.
/// </summary>
public class TestStepTracker(string testStepId, string pickleStepId, TestCaseTracker parentTracker)
    : StepTrackerBase(testStepId)
{
    public TestCaseTracker ParentTracker { get; } = parentTracker;
    public string PickleStepId { get; } = pickleStepId;

    // Indicates whether the step was successfully bound to a Step Definition.
    public bool IsBound { get; private set; }
    // The Step Definition(s) that match this step of the Test Case. None for no match, 1 for a successful match, 2 or more for Ambiguous match.
    public List<string> StepDefinitionIds { get; private set; }
    public List<TestStepArgument> StepArguments { get; } = new();

    public bool IsAmbiguous { get; private set; }

    public void ProcessEvent(StepStartedEvent stepStartedEvent)
    {
    }

    // Once the StepFinishedAt event fires, we can finally capture which step binding was used and the arguments sent as parameters to the binding method
    public void ProcessEvent(StepFinishedEvent stepFinishedEvent)
    {
        var bindingMatch = stepFinishedEvent.StepContext?.StepInfo?.BindingMatch;
        // store isBound in a local variable to avoid null checks later
        var isBound = bindingMatch != null && bindingMatch != BindingMatch.NonMatching;
        IsBound = isBound;

        var ambiguousStepDefinitionIds = DetectAmbiguousStepDefinitionIds(stepFinishedEvent);
        IsAmbiguous = ambiguousStepDefinitionIds.Any();
        if (IsAmbiguous)
        {
            StepDefinitionIds = ambiguousStepDefinitionIds;
        }
        else
        {
            var stepDefinitionId = isBound ? ParentTracker.FindStepDefinitionIdByBindingKey(bindingMatch.StepBinding) : null;
            StepDefinitionIds = stepDefinitionId != null ? [stepDefinitionId] : [];
        }

        if (isBound)
        {
            var hasInputDataTable = stepFinishedEvent.StepContext?.StepInfo?.Table != null;
            var hasInputDocString = stepFinishedEvent.StepContext?.StepInfo?.MultilineText != null;
            var hasEitherDataTableOrDocString = hasInputDataTable || hasInputDocString;

            var argumentValues = bindingMatch.Arguments.Select(arg => arg.Value.ToString()).ToList();
            var argumentStartOffsets = bindingMatch.Arguments.Select(arg => arg.StartOffset).ToList();
            var argumentTypes = bindingMatch.StepBinding.Method.Parameters.Select(p => p.Type.Name).ToList();

            var argumentCount = argumentValues.Count;
            if (argumentCount > 0 && hasEitherDataTableOrDocString)
                argumentCount -= 1; // don't add the DataTable or DocString argument as a TestStepArgument
            for (int i = 0; i < argumentCount; i++)
            {
                StepArguments.Add(new TestStepArgument { Value = argumentValues[i], StartOffset = argumentStartOffsets[i], Type = argumentTypes[i] });
            }
        }
    }

    private List<string> DetectAmbiguousStepDefinitionIds(StepFinishedEvent stepFinishedEvent)
    {
        var stepStatus = stepFinishedEvent.StepContext?.Status ?? ScenarioExecutionStatus.Skipped;

        if (stepStatus == ScenarioExecutionStatus.TestError && 
            stepFinishedEvent.ScenarioContext.TestError is AmbiguousBindingException ambiguousBindingException)
        {
            return ambiguousBindingException.Matches
                .Select(m => ParentTracker.FindStepDefinitionIdByBindingKey(m.StepBinding))
                .ToList();
        }

        return [];
    }
}