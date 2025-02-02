using Io.Cucumber.Messages.Types;
using Reqnroll.Assist;
using Reqnroll.Bindings;
using Reqnroll.CucumberMessages.PayloadProcessing.Cucumber;
using Reqnroll.Events;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Threading;

namespace Reqnroll.CucumberMessages.ExecutionTracking
{
    public class StepArgument
    {
        public string Value;
        public string Type;
    }

    /// <summary>
    /// This class is used to track the execution of Test StepDefinitionBinding Methods
    /// </summary>
    public class TestStepTracker : StepExecutionTrackerBase, IGenerateMessage
    {
        public TestStepTracker(TestCaseTracker parentTracker, TestCaseExecutionRecord parentExecutionRecord) : base(parentTracker, parentExecutionRecord)
        {
        }

        public IEnumerable<Envelope> GenerateFrom(ExecutionEvent executionEvent)
        {
            return executionEvent switch
            {
                StepStartedEvent started => [Envelope.Create(CucumberMessageFactory.ToTestStepStarted(this))],
                StepFinishedEvent finished => [Envelope.Create(CucumberMessageFactory.ToTestStepFinished(this))],
                _ => Enumerable.Empty<Envelope>()
            };
        }

        internal void ProcessEvent(StepStartedEvent stepStartedEvent)
        {
            StepStarted = stepStartedEvent.Timestamp;
            
            // if this is the first time to execute this step for this test, generate the properties needed to Generate the TestStep Message (stored in a TestStepDefinition)
            if (ParentTestCase.Attempt_Count == 0)
            {
                var testStepId = ParentTestCase.IDGenerator.GetNewId();
                var pickleStepID = stepStartedEvent.StepContext.StepInfo.PickleStepId;
                Definition = new(testStepId, pickleStepID, ParentTestCase.TestCaseDefinition);
                ParentTestCase.TestCaseDefinition.StepDefinitions.Add(Definition);
            }
            else
            {
                // On retries of the TestCase, find the Definition previously created.
                Definition = ParentTestCase.TestCaseDefinition.StepDefinitions.OfType<TestStepDefinition>().Where(sd => sd.PickleStepID == stepStartedEvent.StepContext.StepInfo.PickleStepId).First();
            }
        }

        internal void ProcessEvent(StepFinishedEvent stepFinishedEvent)
        {
            if (ParentTestCase.Attempt_Count == 0)
                Definition.PopulateStepDefinitionFromExecutionResult(stepFinishedEvent);
            StepFinished = stepFinishedEvent.Timestamp;

            Status = stepFinishedEvent.StepContext.Status;
        }
    }
}