using Io.Cucumber.Messages.Types;
using Reqnroll.Formatters.PayloadProcessing.Cucumber;
using Reqnroll.Events;
using System.Threading.Tasks;
using Reqnroll.Formatters.PubSub;
using Gherkin.CucumberMessages;

namespace Reqnroll.Formatters.ExecutionTracking;

/// <summary>
/// This class is used to track the execution of test steps.
/// </summary>
public class TestStepExecutionTracker(TestCaseExecutionTracker parentTracker, ICucumberMessageFactory messageFactory, IMessagePublisher publisher, IIdGenerator idGenerator) :
    StepExecutionTrackerBase(parentTracker, messageFactory, publisher)
{
    public async Task ProcessEvent(StepStartedEvent stepStartedEvent)
    {
        StepStartedAt = stepStartedEvent.Timestamp;

        // Resolve (or, on first sight across all attempts, create) the ledger entry for this step.
        // We cannot trust AttemptCount here: an earlier attempt may have aborted before reaching this
        // step, so the entry might not yet exist even though this is not the first attempt.
        var pickleStepId = stepStartedEvent.StepContext.StepInfo.PickleStepId;
        var occurrence = ParentTracker.NextOccurrence(StepKind.TestStep, pickleStepId);
        StepTracker = TestCaseTracker.GetOrCreateTestStepTracker(pickleStepId, occurrence);

        await Publisher.PublishAsync(Envelope.Create(MessageFactory.ToTestStepStarted(this)));
    }

    public async Task ProcessEvent(StepFinishedEvent stepFinishedEvent)
    {
        // Reuse the ledger entry resolved at StepStarted; capture of binding details is idempotent
        // (guarded inside TestStepTracker) so re-execution on a retry does not duplicate it.
        if (StepTracker is TestStepTracker testStepTracker)
        {
            testStepTracker.ProcessEvent(stepFinishedEvent);
        }

        StepFinishedAt = stepFinishedEvent.Timestamp;
        Status = stepFinishedEvent.StepContext.Status;
        Exception = stepFinishedEvent.StepContext.StepError;

        if (Status == ScenarioExecutionStatus.UndefinedStep)
        {
            var programmingLanguage = stepFinishedEvent.FeatureContext.FeatureInfo.GenerationTargetLanguage.ToString();
            // retrieve skeleton code from the ScenarioContext (keyed by StepInstance)
            if (stepFinishedEvent.ScenarioContext is ScenarioContext scenarioContext)
            {
                if (scenarioContext.MissingSteps.TryGetValue(stepFinishedEvent.StepContext.StepInfo.StepInstance, out var skeletonMessage))
                {
                    await Publisher.PublishAsync(Envelope.Create(MessageFactory.ToSuggestion(this, programmingLanguage, skeletonMessage, idGenerator)));
                }
            }
        }

        await Publisher.PublishAsync(Envelope.Create(MessageFactory.ToTestStepFinished(this)));
    }
}