using Io.Cucumber.Messages.Types;
using Reqnroll.CucumberMessages.PayloadProcessing.Cucumber;
using Reqnroll.Events;
using System.Collections.Generic;
using System.Linq;

namespace Reqnroll.CucumberMessages.ExecutionTracking
{
    /// <summary>
    /// This class is used to track execution of Hook Steps
    /// 
    /// </summary>
    public class HookStepTracker : StepExecutionTrackerBase, IGenerateMessage
    {
        public string HookBindingSignature { get; private set; }
        public HookBindingFinishedEvent HookBindingFinishedEvent { get; private set; }
        public HookStepTracker(TestCaseTracker tracker, TestCaseExecutionRecord testCaseExecutionRecord) : base(tracker, testCaseExecutionRecord)
        {
        }

        public void ProcessEvent(HookBindingStartedEvent hookBindingStartedEvent)
        {
            StepStarted = hookBindingStartedEvent.Timestamp;
            if (ParentTestCase.Attempt_Count == 0)
            {
                HookBindingSignature = CucumberMessageFactory.CanonicalizeHookBinding(hookBindingStartedEvent.HookBinding);
                var hookId = ParentTestCase.StepDefinitionsByPattern[HookBindingSignature];
                var testStepId = ParentTestCase.IDGenerator.GetNewId();
                Definition = new HookStepDefinition(testStepId, hookId, ParentTestCase.TestCaseDefinition);
                ParentTestCase.TestCaseDefinition.StepDefinitions.Add(Definition);
            }
            else
            {
                HookBindingSignature = CucumberMessageFactory.CanonicalizeHookBinding(hookBindingStartedEvent.HookBinding);
                var hookId = ParentTestCase.StepDefinitionsByPattern[HookBindingSignature];
                Definition = ParentTestCase.TestCaseDefinition.StepDefinitions.OfType<HookStepDefinition>().Where(hd => hd.HookId == hookId).First();
            }
        }

        public void ProcessEvent(HookBindingFinishedEvent hookFinishedEvent)
        {
            StepFinished = hookFinishedEvent.Timestamp;
            HookBindingFinishedEvent = hookFinishedEvent;
            Exception = hookFinishedEvent.HookException;
            Status = Exception == null ? ScenarioExecutionStatus.OK : ScenarioExecutionStatus.TestError;
        }

        public IEnumerable<Envelope> GenerateFrom(ExecutionEvent executionEvent)
        {
            return executionEvent switch
            {
                HookBindingStartedEvent started => [Envelope.Create(CucumberMessageFactory.ToTestStepStarted(this))],
                HookBindingFinishedEvent finished => [Envelope.Create(CucumberMessageFactory.ToTestStepFinished(this))],
                _ => Enumerable.Empty<Envelope>()
            };
        }
    }


}