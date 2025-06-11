using System;

namespace Reqnroll.Formatters.ExecutionTracking
{
    /// <summary>
    /// Base class for tracking execution of steps (StepDefinitionBinding Methods and Hooks)
    /// </summary>
    public class StepExecutionTrackerBase
    {
        internal string TestCaseStartedID => ParentExecutionRecord.TestCaseStartedId;
        internal ScenarioExecutionStatus Status { get; set; }

        internal DateTime StepStarted { get; set; }
        internal DateTime StepFinished { get; set; }
        internal TimeSpan Duration { get => StepFinished - StepStarted; }
        internal Exception Exception { get; set; }

        internal TestStepDefinition Definition { get; set; }

        internal ITestCaseTracker ParentTestCase { get; }

        internal TestCaseExecutionRecord ParentExecutionRecord { get; }

        internal ICucumberMessageFactory _messageFactory;

        internal StepExecutionTrackerBase(ITestCaseTracker parentScenario, TestCaseExecutionRecord parentExecutionRecord, ICucumberMessageFactory messageFactory)
        {
            ParentTestCase = parentScenario;
            ParentExecutionRecord = parentExecutionRecord;
            _messageFactory = messageFactory;
        }
    }
}