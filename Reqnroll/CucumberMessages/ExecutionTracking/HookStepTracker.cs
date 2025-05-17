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
        internal string HookBindingSignature { get; private set; }
        internal HookBindingFinishedEvent HookBindingFinishedEvent { get; private set; }
        internal HookStepTracker(ITestCaseTracker tracker, TestCaseExecutionRecord testCaseExecutionRecord, ICucumberMessageFactory messageFactory) : base(tracker, testCaseExecutionRecord, messageFactory)
        {
        }

        internal void ProcessEvent(HookBindingStartedEvent hookBindingStartedEvent)
        {
            StepStarted = hookBindingStartedEvent.Timestamp;
            if (ParentTestCase.AttemptCount == 0)
            {
                HookBindingSignature = _messageFactory.CanonicalizeHookBinding(hookBindingStartedEvent.HookBinding);
                var hookId = ParentTestCase.StepDefinitionsByMethodSignature[HookBindingSignature];
                var testStepId = ParentTestCase.IDGenerator.GetNewId();
                Definition = new HookStepDefinition(testStepId, hookId, ParentTestCase.TestCaseDefinition, _messageFactory);
                ParentTestCase.TestCaseDefinition.AddStepDefinition(Definition);
            }
            else
            {
                HookBindingSignature = _messageFactory.CanonicalizeHookBinding(hookBindingStartedEvent.HookBinding);
                var hookId = ParentTestCase.StepDefinitionsByMethodSignature[HookBindingSignature];
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
                HookBindingStartedEvent started => [Envelope.Create(_messageFactory.ToTestStepStarted(this))],
                HookBindingFinishedEvent finished => [Envelope.Create(_messageFactory.ToTestStepFinished(this))],
                _ => Enumerable.Empty<Envelope>()
            };
        }
    }


}