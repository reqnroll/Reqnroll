using System;

namespace Reqnroll.CucumberMessages
{
    public class StepProcessorBase : IStepProcessor
    {
        public string TestStepID { get; set; }
        public string TestCaseStartedID => ParentTestCase.TestCaseStartedId;
        public ScenarioExecutionStatus Status { get; set; }
        public TimeSpan Duration { get; set; }
        public Exception Exception { get; set; }

        public TestCaseCucumberMessageTracker ParentTestCase;

        public StepProcessorBase(TestCaseCucumberMessageTracker parentScenario)
        {
            ParentTestCase = parentScenario;
        }
    }
}