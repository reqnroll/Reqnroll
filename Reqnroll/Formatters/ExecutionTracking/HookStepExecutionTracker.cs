using Io.Cucumber.Messages.Types;
using Reqnroll.Formatters.PayloadProcessing.Cucumber;
using Reqnroll.Events;
using System.Collections.Generic;

namespace Reqnroll.Formatters.ExecutionTracking;

/// <summary>
/// This class is used to track execution of hook steps.
/// </summary>
public class HookStepExecutionTracker(TestCaseExecutionTracker parentTracker, ICucumberMessageFactory messageFactory) : 
    StepExecutionTrackerBase(parentTracker, messageFactory), IGenerateMessage
{
    IEnumerable<Envelope> IGenerateMessage.GenerateFrom(ExecutionEvent executionEvent)
    {
        return executionEvent switch
        {
            HookBindingStartedEvent => [Envelope.Create(MessageFactory.ToTestStepStarted(this))],
            HookBindingFinishedEvent => [Envelope.Create(MessageFactory.ToTestStepFinished(this))],
            _ => []
        };
    }

    public void ProcessEvent(HookBindingStartedEvent hookBindingStartedEvent)
    {
        StepStarted = hookBindingStartedEvent.Timestamp;

        var hookId = PickleExecutionTracker.StepDefinitionsByBinding[hookBindingStartedEvent.HookBinding];

        if (ParentTracker.IsFirstAttempt)
        {
            TestCaseTracker.ProcessEvent(hookBindingStartedEvent);
        }

        StepTracker = PickleExecutionTracker.TestCaseTracker.GetHookStepTrackerByHookId(hookId);
    }

    public void ProcessEvent(HookBindingFinishedEvent hookFinishedEvent)
    {
        StepFinished = hookFinishedEvent.Timestamp;
        Exception = hookFinishedEvent.HookException;
        Status = Exception == null ? ScenarioExecutionStatus.OK : ScenarioExecutionStatus.TestError;
    }
}