using Gherkin.CucumberMessages;
using Io.Cucumber.Messages.Types;
using Reqnroll.Events;
using System.Collections.Generic;

namespace Reqnroll.CucumberMessages.ExecutionTracking
{
    public interface IFeatureTracker
    {
        bool Enabled { get; }
        bool FeatureExecutionSuccess { get; }
        string FeatureName { get; }
        IIdGenerator IDGenerator { get; set; }
        IEnumerable<Envelope> RuntimeGeneratedMessages { get; }
        IEnumerable<Envelope> StaticMessages { get; }
        string TestRunStartedId { get; }

        void ProcessEvent(AttachmentAddedEvent attachmentAddedEvent);
        void ProcessEvent(FeatureFinishedEvent featureFinishedEvent);
        void ProcessEvent(HookBindingFinishedEvent hookBindingFinishedEvent);
        void ProcessEvent(HookBindingStartedEvent hookBindingStartedEvent);
        void ProcessEvent(OutputAddedEvent outputAddedEvent);
        void ProcessEvent(ScenarioFinishedEvent scenarioFinishedEvent);
        void ProcessEvent(ScenarioStartedEvent scenarioStartedEvent);
        void ProcessEvent(StepFinishedEvent stepFinishedEvent);
        void ProcessEvent(StepStartedEvent stepStartedEvent);
    }
}