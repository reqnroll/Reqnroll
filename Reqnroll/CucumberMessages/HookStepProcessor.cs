using Io.Cucumber.Messages.Types;
using Reqnroll.Events;
using System.Collections.Generic;
using System.Linq;

namespace Reqnroll.CucumberMessages
{
    public class HookStepProcessor : StepProcessorBase
    {
        public string HookBindingSignature { get; private set; }
        public HookBindingFinishedEvent HookBindingFinishedEvent { get; private set; }
        public HookStepProcessor(TestCaseCucumberMessageTracker tracker) : base(tracker)
        {
        }

        public void ProcessEvent(HookBindingStartedEvent hookBindingStartedEvent)
        {
            TestStepID = ParentTestCase.IDGenerator.GetNewId();
            HookBindingSignature = CucumberMessageFactory.CanonicalizeHookBinding(hookBindingStartedEvent.HookBinding);
        }

        public void ProcessEvent(HookBindingFinishedEvent hookFinishedEvent)
        {
            HookBindingFinishedEvent  = hookFinishedEvent;
            Exception = hookFinishedEvent.HookException;
            Status = Exception == null ? ScenarioExecutionStatus.OK : ScenarioExecutionStatus.TestError;
        }
    }

  
}