using Gherkin.CucumberMessages;
using Reqnroll.Bindings;
using Reqnroll.Events;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace Reqnroll.Formatters.ExecutionTracking;

public interface IPickleExecutionTracker
{
    public bool Finished { get; }
    public string TestRunStartedId { get; }
    public ScenarioExecutionStatus ScenarioExecutionStatus { get; }
    public DateTime TestCaseStartedTimeStamp { get; }
    public int AttemptCount { get; }
    public IIdGenerator IdGenerator { get; }
    public TestCaseTracker TestCaseTracker { get; }
    public IReadOnlyDictionary<IBinding, string> StepDefinitionsByBinding { get; }

    Task FinalizeTracking();
    Task ProcessEvent(ScenarioStartedEvent scenarioStartedEvent);
    Task ProcessEvent(ScenarioFinishedEvent scenarioFinishedEvent);
    Task ProcessEvent(StepStartedEvent stepStartedEvent);
    Task ProcessEvent(StepFinishedEvent stepFinishedEvent);
    Task ProcessEvent(HookBindingStartedEvent hookBindingStartedEvent);
    Task ProcessEvent(HookBindingFinishedEvent hookBindingFinishedEvent);
    Task ProcessEvent(AttachmentAddedEvent attachmentAddedEvent);
    Task ProcessEvent(OutputAddedEvent outputAddedEvent);
}