using Io.Cucumber.Messages.Types;
using Reqnroll.Formatters.PayloadProcessing.Cucumber;
using Reqnroll.Events;
using System.Collections.Generic;

namespace Reqnroll.Formatters.ExecutionTracking;

/// <summary>
/// This class is used to track the execution of test steps.
/// </summary>
public class TestStepExecutionTracker(TestCaseExecutionTracker parentTracker, ICucumberMessageFactory messageFactory): 
    StepExecutionTrackerBase(parentTracker, messageFactory), IGenerateMessage
{
    IEnumerable<Envelope> IGenerateMessage.GenerateFrom(ExecutionEvent executionEvent)
    {
        return executionEvent switch
        {
            StepStartedEvent => [Envelope.Create(MessageFactory.ToTestStepStarted(this))],
            StepFinishedEvent => [Envelope.Create(MessageFactory.ToTestStepFinished(this))],
            _ => []
        };
    }

    public void ProcessEvent(StepStartedEvent stepStartedEvent)
    {
        StepStartedAt = stepStartedEvent.Timestamp;

        // if this is the first time to execute this step for this test, generate the properties needed to Generate the TestStep Message (stored in a TestStepTracker)
        if (ParentTracker.IsFirstAttempt)
        {
            TestCaseTracker.ProcessEvent(stepStartedEvent);
        }

        StepTracker = PickleExecutionTracker.TestCaseTracker.GetTestStepTrackerByPickleId(stepStartedEvent.StepContext.StepInfo.PickleStepId);
    }

    public void ProcessEvent(StepFinishedEvent stepFinishedEvent)
    {
        if (ParentTracker.IsFirstAttempt)
        {
            var testStepTracker = PickleExecutionTracker.TestCaseTracker.GetTestStepTrackerByPickleId(stepFinishedEvent.StepContext.StepInfo.PickleStepId);
            testStepTracker?.ProcessEvent(stepFinishedEvent);
        }

        Exception = stepFinishedEvent.ScenarioContext.TestError;
        StepFinishedAt = stepFinishedEvent.Timestamp;
        Status = stepFinishedEvent.StepContext.Status;
    }
}