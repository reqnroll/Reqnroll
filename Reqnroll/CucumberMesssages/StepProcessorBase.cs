using System;

namespace Reqnroll.CucumberMesssages
{
    public class StepProcessorBase : IStepProcessor
    {
        public string TestStepID { get; set; }
        public string TestCaseStartedID => parentScenario.TestCaseStartedID;
        public ScenarioExecutionStatus Status { get; set; }
        public TimeSpan Duration { get; set; }



        public ScenarioEventProcessor parentScenario;

        public StepProcessorBase(ScenarioEventProcessor parentScenario)
        {
            this.parentScenario = parentScenario;
        }
    }

  
}