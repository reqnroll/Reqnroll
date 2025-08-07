using Gherkin.CucumberMessages;
using Reqnroll.Bindings;
using Reqnroll.Events;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace Reqnroll.Formatters.ExecutionTracking;

public interface IFeatureExecutionTracker
{
    bool Enabled { get; }
    bool FeatureExecutionSuccess { get; }
    string FeatureName { get; }
    IIdGenerator IdGenerator { get; }
    IReadOnlyDictionary<IBinding, string> StepDefinitionsByBinding { get; }
    string TestRunStartedId { get; }

    Task FinalizeTracking();
    Task ProcessEvent(FeatureStartedEvent featureStartedEvent);
    Task ProcessEvent(AttachmentAddedEvent attachmentAddedEvent);
    Task ProcessEvent(HookBindingFinishedEvent hookBindingFinishedEvent);
    Task ProcessEvent(HookBindingStartedEvent hookBindingStartedEvent);
    Task ProcessEvent(OutputAddedEvent outputAddedEvent);
    Task ProcessEvent(ScenarioFinishedEvent scenarioFinishedEvent);
    Task ProcessEvent(ScenarioStartedEvent scenarioStartedEvent);
    Task ProcessEvent(StepFinishedEvent stepFinishedEvent);
    Task ProcessEvent(StepStartedEvent stepStartedEvent);
}