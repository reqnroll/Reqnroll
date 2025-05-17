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
    /// Tracks the execution lifecycle and state of a single Gherkin scenario (Pickle/TestCase).
    /// <para>
    /// <b>Responsibilities:</b>
    /// <list type="bullet">
    ///   <item>Maintains scenario-level and feature-level execution data for a single test case.</item>
    ///   <item>Records each execution attempt (including retries) as a <see cref="TestCaseExecutionRecord"/>.</item>
    ///   <item>Processes and responds to all relevant execution events (scenario, step, hook, attachment, output).</item>
    ///   <item>Generates Cucumber messages.</item>
    ///   <item>Tracks step definitions and their mapping to execution steps within the scenario.</item>
    /// </list>
    /// </para>
    /// <para>
    /// There is one <c>TestCaseTracker</c> instance per scenario (Pickle) in a feature. It is created and managed by <see cref="TestCaseTrackers"/>.
    /// </para>
    /// </summary>
    internal class TestCaseTracker : ITestCaseTracker
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="TestCaseTracker"/> class for a specific scenario (Pickle).
        /// </summary>
        /// <param name="pickleId">The unique identifier of the scenario (Pickle) being tracked.</param>
        /// <param name="testRunStartedId">The identifier of the test run this scenario belongs to.</param>
        /// <param name="featureName">The name of the feature containing this scenario.</param>
        /// <param name="enabled">Indicates whether this test case is enabled for execution.</param>
        /// <param name="idGenerator">
        /// The ID generator used to create unique IDs for test case messages and related entities.
        /// </param>
        /// <param name="stepDefinitionsByMethodSignature">
        /// A thread-safe dictionary mapping step definition patterns to their unique IDs, 
        /// used to resolve step bindings during execution.
        /// </param>
        /// <param name="instant">The timestamp marking when the test case started execution.</param>
        /// <param name="messageFactory">
        /// The factory responsible for creating Cucumber message objects.
        /// </param>

        public TestCaseTracker(string pickleId, string testRunStartedId, string featureName, bool enabled, IIdGenerator idGenerator, ConcurrentDictionary<string, string> stepDefinitionsByMethodSignature, DateTime instant, ICucumberMessageFactory messageFactory)
        {
            TestRunStartedId = testRunStartedId;
            PickleId = pickleId;
            FeatureName = featureName;
            Enabled = enabled;
            IDGenerator = idGenerator;
            StepDefinitionsByMethodSignature = stepDefinitionsByMethodSignature;
            AttemptCount = -1;
            TestCaseStartedTimeStamp = instant;
            _messageFactory = messageFactory;
        }

        // Feature FeatureName and Pickle ID make up a unique identifier for tracking execution of Test Cases
        internal string FeatureName { get; }
        public string TestRunStartedId { get; }
        internal string PickleId { get; } = string.Empty;
        internal string TestCaseId { get; private set; }
        public int AttemptCount { get; private set; }
        public DateTime TestCaseStartedTimeStamp { get; }

        internal ICucumberMessageFactory _messageFactory;

        internal bool Enabled { get; } //This will be false if the feature could not be pickled
        public bool Finished { get; private set; }
        public ScenarioExecutionStatus ScenarioExecutionStatus { get { return ExecutionHistory.Last().ScenarioExecutionStatus; } }

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
        public ConcurrentDictionary<string, string> StepDefinitionsByMethodSignature { get; private set; }

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
            var testCaseExec = new TestCaseExecutionRecord(_messageFactory, AttemptCount, IDGenerator.GetNewId(), TestCaseId, TestCaseDefinition);
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
            var stepState = new TestStepTracker(this, Current_Execution, _messageFactory);

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
            var hookStepStateTracker = new HookStepTracker(this, Current_Execution, _messageFactory);
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
                Current_Execution.CurrentStep.Definition.TestStepId, 
                _messageFactory);
            Current_Execution.StoreMessageGenerator(attachmentExecutionEventWrapper, attachmentAddedEvent);
        }
        private void ProcessEvent(OutputAddedEvent outputAddedEvent)
        {
            var outputExecutionEventWrapper = new OutputAddedEventWrapper(
                outputAddedEvent,
                TestRunStartedId,
                Current_Execution.TestCaseStartedId,
                Current_Execution.CurrentStep.Definition.TestStepId,
                _messageFactory);

            Current_Execution.StoreMessageGenerator(outputExecutionEventWrapper, outputAddedEvent);
        }
    }
}