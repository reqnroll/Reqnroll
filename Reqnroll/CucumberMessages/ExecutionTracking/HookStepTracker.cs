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
    internal class HookStepTracker : StepExecutionTrackerBase, IGenerateMessage
    {
        internal string HookBindingSignature { get; private set; }
        internal HookBindingFinishedEvent HookBindingFinishedEvent { get; private set; }
        internal HookStepTracker(TestCaseTracker tracker, TestCaseExecutionRecord testCaseExecutionRecord) : base(tracker, testCaseExecutionRecord)
        {
        }

        internal void ProcessEvent(HookBindingStartedEvent hookBindingStartedEvent)
        {
            StepStarted = hookBindingStartedEvent.Timestamp;
            if (ParentTestCase.AttemptCount == 0)
            {
                HookBindingSignature = CucumberMessageFactory.CanonicalizeHookBinding(hookBindingStartedEvent.HookBinding);
                var hookId = ParentTestCase.StepDefinitionsByPattern[HookBindingSignature];
                var testStepId = ParentTestCase.IDGenerator.GetNewId();
                Definition = new HookStepDefinition(testStepId, hookId, ParentTestCase.TestCaseDefinition);
                ParentTestCase.TestCaseDefinition.AddStepDefinition(Definition);
            }
            else
            {
                HookBindingSignature = CucumberMessageFactory.CanonicalizeHookBinding(hookBindingStartedEvent.HookBinding);
                var hookId = ParentTestCase.StepDefinitionsByPattern[HookBindingSignature];
                Definition = ParentTestCase.TestCaseDefinition.FindHookStepDefByHookId(hookId);
            }
        }

        internal void ProcessEvent(HookBindingFinishedEvent hookFinishedEvent)
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