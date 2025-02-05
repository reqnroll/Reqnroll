using Reqnroll.Events;
using System;
using System.Collections.Generic;

namespace Reqnroll.CucumberMessages.ExecutionTracking
{
    /// <summary>
    /// Base class for tracking execution of steps (StepDefinitionBinding Methods and Hooks)
    /// </summary>
    internal class StepExecutionTrackerBase
    {
        internal string TestCaseStartedID => ParentExecutionRecord.TestCaseStartedId;
        internal ScenarioExecutionStatus Status { get; set; }

        internal DateTime StepStarted { get; set; }
        internal DateTime StepFinished { get; set; }
        internal TimeSpan Duration { get => StepFinished - StepStarted; }
        internal Exception Exception { get; set; }

        internal TestStepDefinition Definition { get; set; }

        internal TestCaseTracker ParentTestCase { get; }

        internal TestCaseExecutionRecord ParentExecutionRecord { get; }

        internal StepExecutionTrackerBase(TestCaseTracker parentScenario, TestCaseExecutionRecord parentExecutionRecord)
        {
            ParentTestCase = parentScenario;
            ParentExecutionRecord = parentExecutionRecord;
        }
    }
}