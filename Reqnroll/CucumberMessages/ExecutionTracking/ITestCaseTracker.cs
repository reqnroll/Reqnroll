using Io.Cucumber.Messages.Types;
using Reqnroll.Events;
using System;
using System.Collections.Generic;

namespace Reqnroll.CucumberMessages.ExecutionTracking
{
    public interface ITestCaseTracker
    {
        public bool Finished { get; }
        public IEnumerable<Envelope> RuntimeGeneratedMessages { get; }
        public ScenarioExecutionStatus ScenarioExecutionStatus { get; }
        public DateTime TestCaseStartedTimeStamp { get; }

        public void ProcessEvent(ExecutionEvent anEvent);
    }
}