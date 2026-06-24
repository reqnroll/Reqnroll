using Io.Cucumber.Messages.Types;
using Reqnroll.Formatters.PayloadProcessing.Cucumber;
using Reqnroll.Events;
using System.Threading.Tasks;
using Reqnroll.Formatters.PubSub;

namespace Reqnroll.Formatters.ExecutionTracking;

/// <summary>
/// This class is used to track execution of hook steps.
/// </summary>
public class HookStepExecutionTracker(TestCaseExecutionTracker parentTracker, ICucumberMessageFactory messageFactory, IMessagePublisher publisher) : 
    StepExecutionTrackerBase(parentTracker, messageFactory, publisher)
{
    public async Task ProcessEvent(HookBindingStartedEvent hookBindingStartedEvent)
    {
        StepStartedAt = hookBindingStartedEvent.Timestamp;

        var hookId = PickleExecutionTracker.StepDefinitionsByBinding[hookBindingStartedEvent.HookBinding];

        // Resolve (or create on first sight) the ledger entry for this hook firing. The occurrence index
        // distinguishes repeated firings of the same hook (e.g. BeforeStep/AfterStep) and stays stable
        // across retries, even if an earlier attempt aborted before this firing occurred.
        var occurrence = ParentTracker.NextOccurrence(StepKind.Hook, hookId);
        StepTracker = TestCaseTracker.GetOrCreateHookStepTracker(hookId, occurrence);

        await Publisher.PublishAsync(Envelope.Create(MessageFactory.ToTestStepStarted(this)));
    }

    public async Task ProcessEvent(HookBindingFinishedEvent hookFinishedEvent)
    {
        StepFinishedAt = hookFinishedEvent.Timestamp;
        Exception = hookFinishedEvent.HookException;
        Status = hookFinishedEvent.HookStatus;

        await Publisher.PublishAsync(Envelope.Create(MessageFactory.ToTestStepFinished(this)));
    }
}