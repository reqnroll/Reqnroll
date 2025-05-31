using Gherkin.CucumberMessages;
using Io.Cucumber.Messages.Types;
using Reqnroll.Events;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;

namespace Reqnroll.CucumberMessages.ExecutionTracking
{
    public interface ITestCaseTracker
    {
        public bool Finished { get; }
        public string TestRunStartedId { get; }
        public IEnumerable<Envelope> RuntimeGeneratedMessages { get; }
        public ScenarioExecutionStatus ScenarioExecutionStatus { get; }
        public DateTime TestCaseStartedTimeStamp { get; }
        public int AttemptCount { get; }
        public IIdGenerator IDGenerator { get; set; }
        public TestCaseDefinition TestCaseDefinition { get; }
        public ConcurrentDictionary<string, string> StepDefinitionsByMethodSignature { get; }
        public void ProcessEvent(ExecutionEvent anEvent);
    }
}