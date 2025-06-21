using Reqnroll.Bindings;
using Reqnroll.Formatters.PayloadProcessing.Cucumber;
using Reqnroll.Events;
using System.Collections.Generic;
using System.Linq;

namespace Reqnroll.Formatters.ExecutionTracking;

/// <summary>
/// Tracks the information needed for a Cucumber Messages "test case step", that is a step with binding information.
/// The test case step needs to be built upon the first execution attempt of a pickle.
/// </summary>
public class TestStepTracker(string testStepId, string pickleStepId, TestCaseTracker parentTracker)
    : StepTracker(testStepId)
{
    public TestCaseTracker ParentTracker { get; } = parentTracker;
    public string PickleStepId { get; } = pickleStepId;

    // Indicates whether the step was successfully bound to a Step Definition.
    public bool IsBound { get; private set; }
    // The Step Definition(s) that match this step of the Test Case. None for no match, 1 for a successful match, 2 or more for Ambiguous match.
    public List<string> StepDefinitionIds { get; private set; }
    public List<TestStepArgument> StepArguments { get; } = new();

    // List of method signatures that cause an ambiguous situation to arise
    public List<string> AmbiguousStepDefinitions { get; private set; } = new();
    public bool IsAmbiguous => AmbiguousStepDefinitions != null && AmbiguousStepDefinitions.Any();

    public void ProcessEvent(StepStartedEvent stepStartedEvent)
    {
    }

    // Once the StepFinishedAt event fires, we can finally capture which step binding was used and the arguments sent as parameters to the binding method
    public void ProcessEvent(StepFinishedEvent stepFinishedEvent)
    {
        var bindingMatch = stepFinishedEvent.StepContext?.StepInfo?.BindingMatch;
        IsBound = !(bindingMatch == null || bindingMatch == BindingMatch.NonMatching);

        if (IsBound)
        {

        }

        var stepDefinitionBinding = IsBound ? bindingMatch!.StepBinding : null;
        var StepDefinitionId = IsBound ? ParentTracker.FindStepDefinitionIdByBindingKey(stepDefinitionBinding) : null;

        var Status = stepFinishedEvent.StepContext.Status;

        if (Status == ScenarioExecutionStatus.TestError && stepFinishedEvent.ScenarioContext.TestError != null)
        {
            var Exception = stepFinishedEvent.ScenarioContext.TestError;
            if (Exception is AmbiguousBindingException)
            {
                AmbiguousStepDefinitions = new List<string>(((AmbiguousBindingException)Exception).Matches
                    .Select(m => ParentTracker.FindStepDefinitionIdByBindingKey(m.StepBinding)));
            }
        }

        StepDefinitionIds = IsAmbiguous ? AmbiguousStepDefinitions.ToList() : StepDefinitionId != null ? [StepDefinitionId] : [];

        var IsInputDataTableOrDocString = stepFinishedEvent.StepContext.StepInfo.Table != null || stepFinishedEvent.StepContext.StepInfo.MultilineText != null;
        var argumentValues = IsBound ? stepFinishedEvent.StepContext.StepInfo.BindingMatch.Arguments.Select(arg => arg.Value.ToString()).ToList() : new List<string>();
        var argumentStartOffsets = IsBound ? stepFinishedEvent.StepContext.StepInfo.BindingMatch.Arguments.Select(arg => arg.StartOffset).ToList() : new List<int?>();
        var argumentTypes = IsBound ? stepFinishedEvent.StepContext.StepInfo.BindingMatch.StepBinding.Method.Parameters.Select(p => p.Type.Name).ToList() : new List<string>();

        if (IsBound && !IsInputDataTableOrDocString)
        {
            for (int i = 0; i < argumentValues.Count; i++)
            {
                StepArguments.Add(new TestStepArgument { Value = argumentValues[i], StartOffset = argumentStartOffsets[i], Type = argumentTypes[i] });
            }
        }
    }
}