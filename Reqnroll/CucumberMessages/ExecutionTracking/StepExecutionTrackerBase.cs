using System;

namespace Reqnroll.CucumberMessages.ExecutionTracking
{
    /// <summary>
    /// Base class for tracking execution of steps (StepDefinition Methods and Hooks)
    /// </summary>
    public class StepExecutionTrackerBase : IStepTracker
    {
        public string TestStepID { get; set; }
        public string TestCaseStartedID => ParentTestCase.TestCaseStartedId;
        public ScenarioExecutionStatus Status { get; set; }
        public TimeSpan Duration { get; set; }
        public Exception Exception { get; set; }

        public TestCaseCucumberMessageTracker ParentTestCase;

        public StepExecutionTrackerBase(TestCaseCucumberMessageTracker parentScenario)
        {
            ParentTestCase = parentScenario;
        }
    }
}