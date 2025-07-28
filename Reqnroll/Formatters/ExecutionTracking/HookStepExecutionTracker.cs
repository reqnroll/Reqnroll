using Io.Cucumber.Messages.Types;
using Reqnroll.Formatters.PayloadProcessing.Cucumber;
using Reqnroll.Events;
using System.Collections.Generic;
using System.Threading.Tasks;
using Reqnroll.Formatters.PubSub;

namespace Reqnroll.Formatters.ExecutionTracking;

/// <summary>
/// This class is used to track execution of hook steps.
/// </summary>
public class HookStepExecutionTracker(TestCaseExecutionTracker parentTracker, ICucumberMessageFactory messageFactory, IPublishMessage publisher) : 
    StepExecutionTrackerBase(parentTracker, messageFactory, publisher)
{
    public async Task ProcessEvent(HookBindingStartedEvent hookBindingStartedEvent)
    {
        StepStartedAt = hookBindingStartedEvent.Timestamp;

        var hookId = PickleExecutionTracker.StepDefinitionsByBinding[hookBindingStartedEvent.HookBinding];

        if (ParentTracker.IsFirstAttempt)
        {
            TestCaseTracker.ProcessEvent(hookBindingStartedEvent);
        }

        StepTracker = PickleExecutionTracker.TestCaseTracker.GetHookStepTrackerByHookId(hookId);

        await base._publisher.PublishAsync(Envelope.Create(MessageFactory.ToTestStepStarted(this)));
    }

    public async Task ProcessEvent(HookBindingFinishedEvent hookFinishedEvent)
    {
        StepFinishedAt = hookFinishedEvent.Timestamp;
        Exception = hookFinishedEvent.HookException;
        Status = Exception == null ? ScenarioExecutionStatus.OK : ScenarioExecutionStatus.TestError;

        await base._publisher.PublishAsync(Envelope.Create(MessageFactory.ToTestStepFinished(this)));
    }
}