using Gherkin.CucumberMessages;
using Io.Cucumber.Messages.Types;
using Reqnroll.CucumberMessages.PayloadProcessing.Cucumber;
using Reqnroll.CucumberMessages.RuntimeSupport;
using Reqnroll.Events;
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
    /// </summary>
    public class TestCaseTracker
    {
        public TestCaseTracker(FeatureTracker featureTracker, string pickleId)
        {
            TestRunStartedId = featureTracker.TestRunStartedId;
            PickleId = pickleId;
            FeatureName = featureTracker.FeatureName;
            Enabled = featureTracker.Enabled;
            IDGenerator = featureTracker.IDGenerator;
            StepDefinitionsByPattern = featureTracker.StepDefinitionsByPattern;
            PickleIdList = featureTracker.PickleIds;
            Attempt_Count = -1;
        }

        // Feature FeatureName and Pickle ID make up a unique identifier for tracking execution of Test Cases
        public string FeatureName { get; set; }
        public string TestRunStartedId { get; }
        public string PickleId { get; set; } = string.Empty;
        public string TestCaseId { get; set; }
        public int Attempt_Count { get; set; }

        // This dictionary maps from (string) PickleIDIndex to (string) PickleID (and is a reference back to this data held at the Feature level; here as a convenience)
        private readonly Dictionary<string, string> PickleIdList;

        public bool Enabled { get; set; } //This will be false if the feature could not be pickled

        public bool Finished { get; set; }
        public ScenarioExecutionStatus ScenarioExecutionStatus { get { return ExecutionHistory.Last().ScenarioExecutionStatus; } }


        // ID Generator to use when generating IDs for TestCase messages and beyond
        // If gherkin feature was generated using integer IDs, then we will use an integer ID generator seeded with the last known integer ID
        // otherwise we'll use a GUID ID generator. We can't know ahead of time which type of ID generator to use, therefore this is not set by the constructor.
        public IIdGenerator IDGenerator { get; set; }

        public TestCaseDefinition TestCaseDefinition { get; private set; }
        private List<TestCaseExecutionRecord> ExecutionHistory = new();
        private TestCaseExecutionRecord Current_Execution;
        private void SetExecutionRecordAsCurrentlyExecuting(TestCaseExecutionRecord executionRecord)
        {
            Current_Execution = executionRecord;
            if (!ExecutionHistory.Contains(executionRecord))
                ExecutionHistory.Add(executionRecord);
        }

        //// We keep two dictionaries to track the Test Steps and Hooks.
        //// The first dictionary tracks the Test Steps by their ID, the second will have two entries for each Test Step - one for the Started event and one for the Finished event
        //private Dictionary<string, StepExecutionTrackerBase> StepsById { get; set; } = new();
        //private Dictionary<ExecutionEvent, StepExecutionTrackerBase> StepsByEvent { get; set; } = new();
        //public List<StepExecutionTrackerBase> Steps
        //{
        //    get
        //    {
        //        return StepsByEvent.Where(kvp => kvp.Key is StepStartedEvent || kvp.Key is HookBindingFinishedEvent).Select(kvp => kvp.Value).ToList();
        //    }
        //}

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

        internal void ProcessEvent(ExecutionEvent anEvent)
        {
            if (Enabled) InvokeProcessEvent(anEvent);
        }
        private void InvokeProcessEvent(ExecutionEvent anEvent)
        {
            switch (anEvent)
            {
                case ScenarioStartedEvent scenarioStartedEvent:
                    PreProcessEvent(scenarioStartedEvent);
                    break;
                case ScenarioFinishedEvent scenarioFinishedEvent:
                    PreProcessEvent(scenarioFinishedEvent);
                    break;
                case StepStartedEvent stepStartedEvent:
                    PreProcessEvent(stepStartedEvent);
                    break;
                case StepFinishedEvent stepFinishedEvent:
                    PreProcessEvent(stepFinishedEvent);
                    break;
                case HookBindingStartedEvent hookBindingStartedEvent:
                    PreProcessEvent(hookBindingStartedEvent);
                    break;
                case HookBindingFinishedEvent hookBindingFinishedEvent:
                    PreProcessEvent(hookBindingFinishedEvent);
                    break;
                case AttachmentAddedEvent attachmentAddedEvent:
                    PreProcessEvent(attachmentAddedEvent);
                    break;
                case OutputAddedEvent outputAddedEvent:
                    PreProcessEvent(outputAddedEvent);
                    break;
                default:
                    throw new NotImplementedException($"Event type {anEvent.GetType().Name} is not supported.");
            }
        }
        internal void PreProcessEvent(ScenarioStartedEvent scenarioStartedEvent)
        {
            Attempt_Count++;
            scenarioStartedEvent.FeatureContext.FeatureInfo.CucumberMessages_PickleId = PickleId;
            // on the first time this Scenario is executed, create a TestCaseDefinition
            if (Attempt_Count == 0)
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
            var testCaseExec = new TestCaseExecutionRecord(Attempt_Count, IDGenerator.GetNewId(), this);
            SetExecutionRecordAsCurrentlyExecuting(testCaseExec);
            testCaseExec.RecordStart(scenarioStartedEvent);
        }


        internal void PreProcessEvent(ScenarioFinishedEvent scenarioFinishedEvent)
        {
            Finished = true;
            Current_Execution.RecordFinish(scenarioFinishedEvent);
        }

        internal void PreProcessEvent(StepStartedEvent stepStartedEvent)
        {
            var stepState = new TestStepTracker(this, Current_Execution);

            stepState.ProcessEvent(stepStartedEvent);
            Current_Execution.StepExecutionTrackers.Add(stepState);
            Current_Execution.StoreMessageGenerator(stepState, stepStartedEvent);
            //StepsById.Add(stepState.PickleStepID, stepState);
            //StepsByEvent.Add(stepStartedEvent, stepState);
        }
        internal void PreProcessEvent(StepFinishedEvent stepFinishedEvent)
        {
            //var stepState = StepsById[stepFinishedEvent.StepContext.StepInfo.PickleStepId] as TestStepTracker;
            var stepState = Current_Execution.CurrentStep as TestStepTracker;

            stepState.ProcessEvent(stepFinishedEvent);
            Current_Execution.StoreMessageGenerator(stepState, stepFinishedEvent);
            //StepsByEvent.Add(stepFinishedEvent, stepState);
        }

        internal void PreProcessEvent(HookBindingStartedEvent hookBindingStartedEvent)
        {
            // At this point we only care about hooks that wrap scenarios or steps; Before/AfterTestRun hooks were processed earlier by the Publisher
            if (hookBindingStartedEvent.HookBinding.HookType == Bindings.HookType.AfterFeature || hookBindingStartedEvent.HookBinding.HookType == Bindings.HookType.BeforeFeature)
                return;
            var hookStepStateTracker = new HookStepTracker(this, Current_Execution);
            hookStepStateTracker.ProcessEvent(hookBindingStartedEvent);
            Current_Execution.StepExecutionTrackers.Add(hookStepStateTracker);
            Current_Execution.StoreMessageGenerator(hookStepStateTracker, hookBindingStartedEvent);
            //StepsByEvent.Add(hookBindingStartedEvent, hookStepStateTracker);
        }

        internal void PreProcessEvent(HookBindingFinishedEvent hookBindingFinishedEvent)
        {
            // At this point we only care about hooks that wrap scenarios or steps; TestRunHooks were processed earlier by the Publisher
            if (hookBindingFinishedEvent.HookBinding.HookType == Bindings.HookType.AfterFeature || hookBindingFinishedEvent.HookBinding.HookType == Bindings.HookType.BeforeFeature)
                return;
            var step = Current_Execution.CurrentStep as HookStepTracker;
            //var step = FindMatchingHookStartedEvent(hookBindingFinishedEvent);
            step.ProcessEvent(hookBindingFinishedEvent);
            Current_Execution.StoreMessageGenerator(step, hookBindingFinishedEvent);
        }
        internal void PreProcessEvent(AttachmentAddedEvent attachmentAddedEvent)
        {
            var attachmentExecutionEventWrapper = new AttachmentAddedEventWrapper(
                attachmentAddedEvent,
                Current_Execution.TestCaseTracker.TestRunStartedId,
                Current_Execution.TestCaseStartedId,
                Current_Execution.CurrentStep.Definition.TestStepId);
            Current_Execution.StoreMessageGenerator(attachmentExecutionEventWrapper, attachmentAddedEvent);
        }
        internal void PreProcessEvent(OutputAddedEvent outputAddedEvent)
        {
            var outputExecutionEventWrapper = new OutputAddedEventWrapper(
                outputAddedEvent,
                Current_Execution.TestCaseTracker.TestRunStartedId,
                Current_Execution.TestCaseStartedId,
                Current_Execution.CurrentStep.Definition.TestStepId);

            Current_Execution.StoreMessageGenerator(outputExecutionEventWrapper, outputAddedEvent);
        }
    }
}