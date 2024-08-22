using Io.Cucumber.Messages.Types;
using Reqnroll.Events;
using System.Collections.Generic;
using System.Linq;

namespace Reqnroll.CucumberMesssages
{
    public class HookStepProcessor : StepProcessorBase
    {
        public HookBindingFinishedEvent HookBindingFinishedEvent { get; private set; }
        public HookStepProcessor(ScenarioEventProcessor parentScenarioState) : base(parentScenarioState)
        {
        }

        public IEnumerable<Envelope> ProcessEvent(HookBindingFinishedEvent hookFinishedEvent)
        {
            HookBindingFinishedEvent  = hookFinishedEvent;
            TestStepID = parentScenario.IdGenerator.GetNewId();
            return Enumerable.Empty<Envelope>();
        }
    }

  
}