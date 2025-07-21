using Gherkin.CucumberMessages;
using Io.Cucumber.Messages.Types;
using Reqnroll.Bindings;
using Reqnroll.Events;
using System;
using System.Collections.Generic;

namespace Reqnroll.Formatters.ExecutionTracking;

public interface IPickleExecutionTracker
{
    public bool Finished { get; }
    public string TestRunStartedId { get; }
    public IEnumerable<Envelope> RuntimeGeneratedMessages { get; }
    public ScenarioExecutionStatus ScenarioExecutionStatus { get; }
    public DateTime TestCaseStartedTimeStamp { get; }
    public int AttemptCount { get; }
    public IIdGenerator IdGenerator { get; }
    public TestCaseTracker TestCaseTracker { get; }
    public IReadOnlyDictionary<IBinding, string> StepDefinitionsByBinding { get; }

    void ProcessEvent(ScenarioStartedEvent scenarioStartedEvent);
    void ProcessEvent(ScenarioFinishedEvent scenarioFinishedEvent);
    void ProcessEvent(StepStartedEvent stepStartedEvent);
    void ProcessEvent(StepFinishedEvent stepFinishedEvent);
    void ProcessEvent(HookBindingStartedEvent hookBindingStartedEvent);
    void ProcessEvent(HookBindingFinishedEvent hookBindingFinishedEvent);
    void ProcessEvent(AttachmentAddedEvent attachmentAddedEvent);
    void ProcessEvent(OutputAddedEvent outputAddedEvent);
}