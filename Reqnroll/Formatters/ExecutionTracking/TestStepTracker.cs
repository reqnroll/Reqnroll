using Reqnroll.Bindings;
using Reqnroll.Events;
using System;
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

    // Each matching binding will have a list of arguments. This List<List<TestStepArguments>> will have 0 entries if no match, 1 inner list for a successful match
    // and 2 or more inner lists for ambiguous matches
    public List<List<TestStepArgument>> StepArgumentsLists { get; } = new();

    public bool IsAmbiguous { get; private set; }

    public void ProcessEvent(StepStartedEvent stepStartedEvent)
    {
    }

    // Once the StepFinishedAt event fires, we can finally capture which step binding was used and the arguments sent as parameters to the binding method
    public void ProcessEvent(StepFinishedEvent stepFinishedEvent)
    {
        DetectBindingStatus(stepFinishedEvent, out var isBound, out bool isAmbiguous, out List<string> stepDefinitionIds, out var bindingMatches);
        IsBound = isBound;
        IsAmbiguous = isAmbiguous;
        StepDefinitionIds = stepDefinitionIds;

        var hasInputDataTable = stepFinishedEvent.StepContext?.StepInfo?.Table != null;
        var hasInputDocString = stepFinishedEvent.StepContext?.StepInfo?.MultilineText != null;
        var hasEitherDataTableOrDocString = hasInputDataTable || hasInputDocString;

        if (isBound)
        {
            foreach (var bindingMatch in bindingMatches)
            {
                var listOfMatchArguments = new List<TestStepArgument>();
                var argumentValues = bindingMatch.Arguments.Select(arg => arg.Value.ToString()).ToList();
                var argumentStartOffsets = bindingMatch.Arguments.Select(arg => arg.StartOffset).ToList();
                var argumentTypes = bindingMatch.StepBinding.Method.Parameters.Select(p => p.Type.Name).ToList();

                // These might not be the same if the user does not properly match up the expressions with corresponding parameters in the method, 
                // we'll assume the first n of them match up...
                var argumentCount = Math.Min(argumentValues.Count, argumentTypes.Count); 
                if (argumentCount > 0 && hasEitherDataTableOrDocString)
                    argumentCount -= 1; // don't add the DataTable or DocString argument as a TestStepArgument

                for (int i = 0; i < argumentCount; i++)
                {
                    listOfMatchArguments.Add(new TestStepArgument { Value = argumentValues[i], StartOffset = argumentStartOffsets[i], Type = argumentTypes[i] });
                }
                StepArgumentsLists.Add(listOfMatchArguments);
            }
        }
    }

    private void DetectBindingStatus(StepFinishedEvent stepFinishedEvent, out bool isBound, out bool isAmbiguous, out List<string> stepDefinitionIds, out List<BindingMatch> bindingMatches)
    {
        var stepStatus = stepFinishedEvent.StepContext?.Status ?? ScenarioExecutionStatus.Skipped;

        if (stepStatus == ScenarioExecutionStatus.BindingError &&
            stepFinishedEvent.ScenarioContext.TestError is AmbiguousBindingException ambiguousBindingException)
        {
            isBound = true;
            isAmbiguous = true;
            stepDefinitionIds = ambiguousBindingException.Matches
                .Select(m => ParentTracker.FindStepDefinitionIdByBindingKey(m.StepBinding))
                .ToList();
            bindingMatches = ambiguousBindingException.Matches.ToList();
        }
        else
        {
            var bindingMatch = stepFinishedEvent.StepContext?.StepInfo?.BindingMatch;
            isBound = bindingMatch != null && bindingMatch != BindingMatch.NonMatching;
            isAmbiguous = false;
            var stepDefinitionId = isBound ? ParentTracker.FindStepDefinitionIdByBindingKey(bindingMatch!.StepBinding) : null;
            stepDefinitionIds = stepDefinitionId != null ? [stepDefinitionId] : [];
            bindingMatches = [bindingMatch];
        }
    }
}