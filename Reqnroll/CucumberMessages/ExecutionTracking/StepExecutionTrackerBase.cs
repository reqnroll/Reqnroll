using Reqnroll.Events;
using System;
using System.Collections.Generic;

namespace Reqnroll.CucumberMessages.ExecutionTracking
{
    /// <summary>
    /// Base class for tracking execution of steps (StepDefinitionBinding Methods and Hooks)
    /// </summary>
    public class StepExecutionTrackerBase 
    {
        public string TestCaseStartedID => ParentExecutionRecord.TestCaseStartedId;
        public ScenarioExecutionStatus Status { get; set; }

        public DateTime StepStarted { get; set; }
        public DateTime StepFinished { get; set; }
        public TimeSpan Duration { get => StepFinished - StepStarted; }
        public Exception Exception { get; set; }

        public TestStepDefinition Definition { get;  set; }

        public TestCaseTracker ParentTestCase;

        internal TestCaseExecutionRecord ParentExecutionRecord { get; }

        public StepExecutionTrackerBase(TestCaseTracker parentScenario, TestCaseExecutionRecord parentExecutionRecord)
        {
            ParentTestCase = parentScenario;
            ParentExecutionRecord = parentExecutionRecord;
        }
    }
}