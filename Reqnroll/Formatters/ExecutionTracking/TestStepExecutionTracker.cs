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

        // if this is the first time to execute this step for this test, generate the properties needed to Generate the TestStep Message (stored in a TestStepTracker)
        if (ParentTracker.IsFirstAttempt)
        {
            TestCaseTracker.ProcessEvent(stepStartedEvent);
        }

        StepTracker = TestCaseTracker.GetTestStepTrackerByPickleId(stepStartedEvent.StepContext.StepInfo.PickleStepId);
        await Publisher.PublishAsync(Envelope.Create(MessageFactory.ToTestStepStarted(this)));
    }

    public async Task ProcessEvent(StepFinishedEvent stepFinishedEvent)
    {
        if (ParentTracker.IsFirstAttempt)
        {
            var testStepTracker = TestCaseTracker.GetTestStepTrackerByPickleId(stepFinishedEvent.StepContext.StepInfo.PickleStepId);
            testStepTracker?.ProcessEvent(stepFinishedEvent);
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