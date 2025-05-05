using Gherkin.CucumberMessages;
using Io.Cucumber.Messages.Types;
using Reqnroll.CucumberMessages.PayloadProcessing.Cucumber;
using Reqnroll.CucumberMessages.RuntimeSupport;
using Reqnroll.Events;
using Reqnroll.Time;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;

namespace Reqnroll.CucumberMessages.ExecutionTracking
{

    /// <summary>
    /// This class is used to track the execution of Test Cases
    /// There will be one instance of this class per gherkin Pickle/TestCase. 
    /// It will track info from both Feature-level and Scenario-level Execution Events for a single Test Case
    /// Individual executions will be recorded as a TestExecutionRecord.
    /// </summary>
    internal class TestCaseTracker : ITestCaseTracker
    {
        internal TestCaseTracker(string pickleId, string testRunStartedId, string featureName, bool enabled, IIdGenerator idGenerator, ConcurrentDictionary<string, string> stepDefinitionsByPattern, DateTime instant)
        {
            TestRunStartedId = testRunStartedId;
            PickleId = pickleId;
            FeatureName = featureName;
            Enabled = enabled;
            IDGenerator = idGenerator;
            StepDefinitionsByPattern = stepDefinitionsByPattern;
            AttemptCount = -1;
            TestCaseStartedTimeStamp = instant;
        }

        // Feature FeatureName and Pickle ID make up a unique identifier for tracking execution of Test Cases
        internal string FeatureName { get; }
        internal string TestRunStartedId { get; }
        internal string PickleId { get; } = string.Empty;
        internal string TestCaseId { get; private set; }
        internal int AttemptCount { get; private set; }
        public DateTime TestCaseStartedTimeStamp { get; }
        internal bool Enabled { get; } //This will be false if the feature could not be pickled
        public bool Finished { get; private set; }
        public ScenarioExecutionStatus ScenarioExecutionStatus { get { return ExecutionHistory.Last().ScenarioExecutionStatus; } }


        // ID Generator to use when generating IDs for TestCase messages and beyond
        // If gherkin feature was generated using integer IDs, then we will use an integer ID generator seeded with the last known integer ID
        // otherwise we'll use a GUID ID generator. We can't know ahead of time which type of ID generator to use, therefore this is not set by the constructor.
        internal IIdGenerator IDGenerator { get; set; }

        internal TestCaseDefinition TestCaseDefinition { get; private set; }
        private List<TestCaseExecutionRecord> ExecutionHistory = new();
        private TestCaseExecutionRecord Current_Execution;
        private void SetExecutionRecordAsCurrentlyExecuting(TestCaseExecutionRecord executionRecord)
        {
            Current_Execution = executionRecord;
            if (!ExecutionHistory.Contains(executionRecord))
                ExecutionHistory.Add(executionRecord);
        }

        // Returns all of the Cucumber Messages that result from execution of the Test Case (eg, TestCase, TestCaseStarted, TestCaseFinished, TestStepStarted/Finished)
        public IEnumerable<Envelope> RuntimeGeneratedMessages
        {
            get
            {
                // ask each execution record for its messages
                var tempListOfMessages = new List<Envelope>();
                foreach (var testCaseExecution in ExecutionHistory)
                {
                    var aRunsWorth = testCaseExecution.RuntimeMessages;
                    if (tempListOfMessages.Count > 0)
                    {
                        // there has been a previous run of this scenario that was retried
                        // We will create a copy of the last TestRunFinished message, but with the 'willBeRetried' flag set to true
                        // and substitute the copy into the list
                        var lastRunTestCaseFinished = tempListOfMessages.Last();
                        var lastRunTCMarkedAsToBeRetried = FixupWillBeRetried(lastRunTestCaseFinished);
                        tempListOfMessages.Remove(lastRunTestCaseFinished);
                        tempListOfMessages.Add(lastRunTCMarkedAsToBeRetried);
                    }
                    tempListOfMessages.AddRange(aRunsWorth);
                }
                return tempListOfMessages;
            }
        }

        private Envelope FixupWillBeRetried(Envelope lastRunTestCaseFinished)
        {
            TestCaseFinished prior = lastRunTestCaseFinished.Content() as TestCaseFinished;
            return Envelope.Create(new TestCaseFinished(prior.TestCaseStartedId, prior.Timestamp, true));
        }

        // This dictionary tracks the StepDefintions(ID) by their method signature
        // used during TestCase creation to map from a Step Definition binding to its ID
        internal ConcurrentDictionary<string, string> StepDefinitionsByPattern;

        public void ProcessEvent(ExecutionEvent anEvent)
        {
            if (!Enabled) return;
            switch (anEvent)
            {
                case ScenarioStartedEvent scenarioStartedEvent:
                    ProcessEvent(scenarioStartedEvent);
                    break;
                case ScenarioFinishedEvent scenarioFinishedEvent:
                    ProcessEvent(scenarioFinishedEvent);
                    break;
                case StepStartedEvent stepStartedEvent:
                    ProcessEvent(stepStartedEvent);
                    break;
                case StepFinishedEvent stepFinishedEvent:
                    ProcessEvent(stepFinishedEvent);
                    break;
                case HookBindingStartedEvent hookBindingStartedEvent:
                    ProcessEvent(hookBindingStartedEvent);
                    break;
                case HookBindingFinishedEvent hookBindingFinishedEvent:
                    ProcessEvent(hookBindingFinishedEvent);
                    break;
                case AttachmentAddedEvent attachmentAddedEvent:
                    ProcessEvent(attachmentAddedEvent);
                    break;
                case OutputAddedEvent outputAddedEvent:
                    ProcessEvent(outputAddedEvent);
                    break;
                default:
                    throw new NotImplementedException($"Event type {anEvent.GetType().Name} is not supported.");
            }
        }
        private void ProcessEvent(ScenarioStartedEvent scenarioStartedEvent)
        {
            AttemptCount++;
            scenarioStartedEvent.FeatureContext.FeatureInfo.CucumberMessages_PickleId = PickleId;
            // on the first time this Scenario is executed, create a TestCaseDefinition
            if (AttemptCount == 0)
            {
                TestCaseId = IDGenerator.GetNewId();
                TestCaseDefinition = new TestCaseDefinition(TestCaseId, PickleId, this);
            }
            else
            {
                // Reset tracking
                Finished = false;
                Current_Execution = null;
            }
            var testCaseExec = new TestCaseExecutionRecord(AttemptCount, IDGenerator.GetNewId(), TestCaseId, TestCaseDefinition);
            SetExecutionRecordAsCurrentlyExecuting(testCaseExec);
            testCaseExec.RecordStart(scenarioStartedEvent);
        }


        private void ProcessEvent(ScenarioFinishedEvent scenarioFinishedEvent)
        {
            Finished = true;
            Current_Execution.RecordFinish(scenarioFinishedEvent);
        }

        private void ProcessEvent(StepStartedEvent stepStartedEvent)
        {
            var stepState = new TestStepTracker(this, Current_Execution);

            stepState.ProcessEvent(stepStartedEvent);
            Current_Execution.StepExecutionTrackers.Add(stepState);
            Current_Execution.StoreMessageGenerator(stepState, stepStartedEvent);
        }
        private void ProcessEvent(StepFinishedEvent stepFinishedEvent)
        {
            var stepState = Current_Execution.CurrentStep as TestStepTracker;

            stepState.ProcessEvent(stepFinishedEvent);
            Current_Execution.StoreMessageGenerator(stepState, stepFinishedEvent);
        }

        private void ProcessEvent(HookBindingStartedEvent hookBindingStartedEvent)
        {
            // At this point we only care about hooks that wrap scenarios or steps; Before/AfterTestRun hooks were processed earlier by the Publisher
            if (hookBindingStartedEvent.HookBinding.HookType == Bindings.HookType.AfterTestRun || hookBindingStartedEvent.HookBinding.HookType == Bindings.HookType.BeforeTestRun)
                return;
            var hookStepStateTracker = new HookStepTracker(this, Current_Execution);
            hookStepStateTracker.ProcessEvent(hookBindingStartedEvent);
            Current_Execution.StepExecutionTrackers.Add(hookStepStateTracker);
            Current_Execution.StoreMessageGenerator(hookStepStateTracker, hookBindingStartedEvent);
        }

        private void ProcessEvent(HookBindingFinishedEvent hookBindingFinishedEvent)
        {
            // At this point we only care about hooks that wrap scenarios or steps; TestRunHooks were processed earlier by the Publisher
            if (hookBindingFinishedEvent.HookBinding.HookType == Bindings.HookType.AfterTestRun || hookBindingFinishedEvent.HookBinding.HookType == Bindings.HookType.BeforeTestRun)
                return;
            var step = Current_Execution.CurrentStep as HookStepTracker;
            step.ProcessEvent(hookBindingFinishedEvent);
            Current_Execution.StoreMessageGenerator(step, hookBindingFinishedEvent);
        }
        private void ProcessEvent(AttachmentAddedEvent attachmentAddedEvent)
        {
            var attachmentExecutionEventWrapper = new AttachmentAddedEventWrapper(
                attachmentAddedEvent,
                TestRunStartedId,
                Current_Execution.TestCaseStartedId,
                Current_Execution.CurrentStep.Definition.TestStepId);
            Current_Execution.StoreMessageGenerator(attachmentExecutionEventWrapper, attachmentAddedEvent);
        }
        private void ProcessEvent(OutputAddedEvent outputAddedEvent)
        {
            var outputExecutionEventWrapper = new OutputAddedEventWrapper(
                outputAddedEvent,
                TestRunStartedId,
                Current_Execution.TestCaseStartedId,
                Current_Execution.CurrentStep.Definition.TestStepId);

            Current_Execution.StoreMessageGenerator(outputExecutionEventWrapper, outputAddedEvent);
        }
    }
}