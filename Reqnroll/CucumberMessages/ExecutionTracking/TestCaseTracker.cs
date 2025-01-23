﻿using Gherkin.CucumberMessages;
using Io.Cucumber.Messages.Types;
using Reqnroll.Bindings;
using Reqnroll.CucumberMessages.PayloadProcessing.Cucumber;
using Reqnroll.CucumberMessages.RuntimeSupport;
using Reqnroll.Events;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Net.NetworkInformation;

namespace Reqnroll.CucumberMessages.ExecutionTracking
{
    /// <summary>
    /// This class is used to track the execution of Test Cases
    /// There will be one instance of this class per gherkin Pickle/TestCase. It will track info from both Feature-level and Scenario-level Execution Events for a single Test Case
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
            Attempt_Count = 0;
        }

        // Feature FeatureName and Pickle ID make up a unique identifier for tracking execution of Test Cases
        public string FeatureName { get; set; }
        public string TestRunStartedId { get; }
        public string PickleId { get; set; } = string.Empty;
        public string TestCaseId { get; set; }
        public string TestCaseStartedId { get; private set; }

        public int Attempt_Count { get; set; }

        private readonly Dictionary<string, string> PickleIdList;

        public bool Enabled { get; set; } //This will be false if the feature could not be pickled

        public bool Finished { get; set; }


        // ID Generator to use when generating IDs for TestCase messages and beyond
        // If gherkin feature was generated using integer IDs, then we will use an integer ID generator seeded with the last known integer ID
        // otherwise we'll use a GUID ID generator. We can't know ahead of time which type of ID generator to use, therefore this is not set by the constructor.
        public IIdGenerator IDGenerator { get; set; }

        // We keep two dictionaries to track the Test Steps and Hooks.
        // The first dictionary tracks the Test Steps by their ID, the second will have two entries for each Test Step - one for the Started event and one for the Finished event
        private Dictionary<string, StepExecutionTrackerBase> StepsById { get; set; } = new();
        private Dictionary<ExecutionEvent, StepExecutionTrackerBase> StepsByEvent { get; set; } = new();
        public List<StepExecutionTrackerBase> Steps
        {
            get
            {
                return StepsByEvent.Where(kvp => kvp.Key is StepStartedEvent || kvp.Key is HookBindingFinishedEvent).Select(kvp => kvp.Value).ToList();
            }
        }
        public ScenarioExecutionStatus ScenarioExecutionStatus { get; private set; }

        // Message generation is a two-stage process. We use these two stages with both the XXXStarted and XXXFinished Events.
        // In the first stage (PreProcess) we capture what information we can about the step/hook that provides enough information to tie everything together.
        // In the second stage (PostProcess) (run after all events have been executed for a TestCase) we generate the Cucumber Messages for the Test Case
        // This queue holds ExecutionEvents that will be processed in stage 2
        private Queue<ExecutionEvent> _events = new();

        internal IEnumerable<Envelope> StaticMessages;

        // During Post-Processing, this is used to track the most recent TestStepStarted event so that Attachments and Output events can be associated with it
        private TestStepStarted mostRecentTestStepStarted;

        // This dictionary tracks the StepDefintions(ID) by their method signature
        // used during TestCase creation to map from a Step Definition binding to its ID
        internal ConcurrentDictionary<string, string> StepDefinitionsByPattern;

        // Processing of events is handled in two stages.
        // Stage 1: As events are recieved, critical information needed right away is extracted and stored in the TestCaseTracker
        //          The event is then stored in a queue for processing in stage 2
        // Stage 2: When TestRunFinished is recieved, the messages are processed to generate Cucumber Messages and then sent in a single batch to the broker
        internal void ProcessEvent(ExecutionEvent anEvent)
        {
            _events.Enqueue(anEvent);
            if (Enabled) InvokePreProcessEvent(anEvent);
        }
        private void InvokePreProcessEvent(ExecutionEvent anEvent)
        {
            switch (anEvent)
            {
                case FeatureStartedEvent featureStartedEvent:
                    PreProcessEvent(featureStartedEvent);
                    break;
                case FeatureFinishedEvent featureFinishedEvent:
                    PreProcessEvent(featureFinishedEvent);
                    break;
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

        public IEnumerable<Envelope> TestCaseCucumberMessages()
        {
            if (!Enabled) return Enumerable.Empty<Envelope>();
            // Stage 2
            return _events.Select(e => InvokePostProcessEvent(e)).SelectMany(x => x);
        }

        private IEnumerable<Envelope> InvokePostProcessEvent(ExecutionEvent anEvent)
        {
            return anEvent switch
            {
                FeatureStartedEvent featureStartedEvent => PostProcessEvent(featureStartedEvent),
                FeatureFinishedEvent featureFinishedEvent => PostProcessEvent(featureFinishedEvent),
                ScenarioStartedEvent scenarioStartedEvent => PostProcessEvent(scenarioStartedEvent),
                ScenarioFinishedEvent scenarioFinishedEvent => PostProcessEvent(scenarioFinishedEvent),
                StepStartedEvent stepStartedEvent => PostProcessEvent(stepStartedEvent),
                StepFinishedEvent stepFinishedEvent => PostProcessEvent(stepFinishedEvent),
                HookBindingStartedEvent hookBindingStartedEvent => PostProcessEvent(hookBindingStartedEvent),
                HookBindingFinishedEvent hookBindingFinishedEvent => PostProcessEvent(hookBindingFinishedEvent),
                AttachmentAddedEvent attachmentAddedEvent => PostProcessEvent(attachmentAddedEvent),
                AttachmentAddedEventWrapper attachmentAddedEventWrapper => PostProcessEvent(attachmentAddedEventWrapper),
                OutputAddedEvent outputAddedEvent => PostProcessEvent(outputAddedEvent),
                OutputAddedEventWrapper outputAddedEventWrapper => PostProcessEvent(outputAddedEventWrapper),
                _ => throw new NotImplementedException($"Event type {anEvent.GetType().Name} is not supported."),
            };
        }
        internal void PreProcessEvent(FeatureStartedEvent featureStartedEvent)
        {
        }

        internal IEnumerable<Envelope> PostProcessEvent(FeatureStartedEvent featureStartedEvent)
        {
            return Enumerable.Empty<Envelope>();
        }

        internal void PreProcessEvent(FeatureFinishedEvent featureFinishedEvent)
        {
        }
        internal IEnumerable<Envelope> PostProcessEvent(FeatureFinishedEvent featureFinishedEvent)
        {
            return Enumerable.Empty<Envelope>();
        }

        internal void PreProcessEvent(ScenarioStartedEvent scenarioStartedEvent)
        {
            if (String.IsNullOrEmpty(TestCaseId))
            {
                scenarioStartedEvent.FeatureContext.FeatureInfo.CucumberMessages_PickleId = PickleId;
                TestCaseId = IDGenerator.GetNewId();
                TestCaseStartedId = IDGenerator.GetNewId();
            }
            else
            {
                ResetTrackerForRetry();
                _events.Enqueue(scenarioStartedEvent);
                TestCaseStartedId = IDGenerator.GetNewId();
                Attempt_Count++;
            }
        }

        // This method resets all the variables used to track execution state and status
        // so that a Retry can be executed
        private void ResetTrackerForRetry()
        {
            Finished = false;
            StepsById.Clear();
            StepsByEvent.Clear();
            ScenarioExecutionStatus = ScenarioExecutionStatus.OK;
            _events.Clear();
            mostRecentTestStepStarted = null;
        }

        internal IEnumerable<Envelope> PostProcessEvent(ScenarioStartedEvent scenarioStartedEvent)
        {
            // On the first execution of this TestCase we emit a TestCase Message.
            // On subsequent retries we do not.
            if (Attempt_Count == 0)
            {
                var TestCase = CucumberMessageFactory.ToTestCase(this, scenarioStartedEvent);
                yield return Envelope.Create(TestCase);
            }
            var TestCaseStarted = CucumberMessageFactory.ToTestCaseStarted(this, scenarioStartedEvent);
            yield return Envelope.Create(TestCaseStarted);
        }

        internal void PreProcessEvent(ScenarioFinishedEvent scenarioFinishedEvent)
        {
            Finished = true;
        }
        internal IEnumerable<Envelope> PostProcessEvent(ScenarioFinishedEvent scenarioFinishedEvent)
        {
            ScenarioExecutionStatus = scenarioFinishedEvent.ScenarioContext.ScenarioExecutionStatus;
            yield return Envelope.Create(CucumberMessageFactory.ToTestCaseFinished(this, scenarioFinishedEvent));
        }

        internal void PreProcessEvent(StepStartedEvent stepStartedEvent)
        {
            var stepState = new TestStepTracker(this);

            stepState.ProcessEvent(stepStartedEvent);
            StepsById.Add(stepState.PickleStepID, stepState);
            StepsByEvent.Add(stepStartedEvent, stepState);
        }
        internal IEnumerable<Envelope> PostProcessEvent(StepStartedEvent stepStartedEvent)
        {
            var stepState = StepsById[stepStartedEvent.StepContext.StepInfo.PickleStepId];
            var stepStarted = CucumberMessageFactory.ToTestStepStarted(stepState as TestStepTracker, stepStartedEvent);
            mostRecentTestStepStarted = stepStarted;
            yield return Envelope.Create(stepStarted);
        }
        internal void PreProcessEvent(StepFinishedEvent stepFinishedEvent)
        {
            var stepState = StepsById[stepFinishedEvent.StepContext.StepInfo.PickleStepId] as TestStepTracker;
            stepState.ProcessEvent(stepFinishedEvent);
            StepsByEvent.Add(stepFinishedEvent, stepState);
        }
        internal IEnumerable<Envelope> PostProcessEvent(StepFinishedEvent stepFinishedEvent)
        {
            var stepState = StepsById[stepFinishedEvent.StepContext.StepInfo.PickleStepId] as TestStepTracker;
            yield return Envelope.Create(CucumberMessageFactory.ToTestStepFinished(stepState as TestStepTracker, stepFinishedEvent));
        }

        internal void PreProcessEvent(HookBindingStartedEvent hookBindingStartedEvent)
        {
            // At this point we only care about hooks that wrap scenarios or steps; Before/AfterTestRun hooks were processed earlier by the Publisher
            if (hookBindingStartedEvent.HookBinding.HookType == Bindings.HookType.AfterFeature || hookBindingStartedEvent.HookBinding.HookType == Bindings.HookType.BeforeFeature)
                return;
            var step = new HookStepTracker(this);
            step.ProcessEvent(hookBindingStartedEvent);
            StepsByEvent.Add(hookBindingStartedEvent, step);

        }
        internal IEnumerable<Envelope> PostProcessEvent(HookBindingStartedEvent hookBindingStartedEvent)
        {
            var hookStepStartState = StepsByEvent[hookBindingStartedEvent];
            var hookStepStarted = CucumberMessageFactory.ToTestStepStarted(hookStepStartState as HookStepTracker, hookBindingStartedEvent);
            mostRecentTestStepStarted = hookStepStarted;
            yield return Envelope.Create(hookStepStarted);
        }

        internal void PreProcessEvent(HookBindingFinishedEvent hookBindingFinishedEvent)
        {
            // At this point we only care about hooks that wrap scenarios or steps; TestRunHooks were processed earlier by the Publisher
            if (hookBindingFinishedEvent.HookBinding.HookType == Bindings.HookType.AfterFeature || hookBindingFinishedEvent.HookBinding.HookType == Bindings.HookType.BeforeFeature)
                return;

            var step = FindMatchingHookStartedEvent(hookBindingFinishedEvent);
            step.ProcessEvent(hookBindingFinishedEvent);
            StepsByEvent.Add(hookBindingFinishedEvent, step);
        }
        internal IEnumerable<Envelope> PostProcessEvent(HookBindingFinishedEvent hookBindingFinishedEvent)
        {
            var hookStepProcessor = FindMatchingHookStartedEvent(hookBindingFinishedEvent);

            yield return Envelope.Create(CucumberMessageFactory.ToTestStepFinished(hookStepProcessor as HookStepTracker, hookBindingFinishedEvent));
        }
        internal void PreProcessEvent(AttachmentAddedEvent attachmentAddedEvent)
        {
            var attachmentExecutionEventWrapper = new AttachmentAddedEventWrapper(attachmentAddedEvent, "");
            _events.Enqueue(attachmentExecutionEventWrapper);
        }
        internal IEnumerable<Envelope> PostProcessEvent(AttachmentAddedEvent attachmentAddedEvent)
        {
            return Enumerable.Empty<Envelope>();
        }
        internal IEnumerable<Envelope> PostProcessEvent(AttachmentAddedEventWrapper attachmentAddedEventWrapper)
        {
            attachmentAddedEventWrapper.TestCaseStepID = mostRecentTestStepStarted.TestStepId;
            attachmentAddedEventWrapper.TestCaseStartedID = mostRecentTestStepStarted.TestCaseStartedId;
            yield return Envelope.Create(CucumberMessageFactory.ToAttachment(this, attachmentAddedEventWrapper));

        }
        internal void PreProcessEvent(OutputAddedEvent outputAddedEvent)
        {
            var pickleStepId = "";
            var outputExecutionEventWrapper = new OutputAddedEventWrapper(outputAddedEvent, pickleStepId);
            _events.Enqueue(outputExecutionEventWrapper);
        }

        internal IEnumerable<Envelope> PostProcessEvent(OutputAddedEvent outputAddedEvent)
        {
            return Enumerable.Empty<Envelope>();
        }

        internal IEnumerable<Envelope> PostProcessEvent(OutputAddedEventWrapper outputAddedEventWrapper)
        {
            outputAddedEventWrapper.TestCaseStepID = mostRecentTestStepStarted.TestStepId; ;
            outputAddedEventWrapper.TestCaseStartedID = mostRecentTestStepStarted.TestCaseStartedId;
            yield return Envelope.Create(CucumberMessageFactory.ToAttachment(this, outputAddedEventWrapper));
        }
        private HookStepTracker FindMatchingHookStartedEvent(HookBindingFinishedEvent hookBindingFinishedEvent)
        {
            return StepsByEvent.Where(kvp => kvp.Key is HookBindingStartedEvent && ((HookBindingStartedEvent)kvp.Key).HookBinding == hookBindingFinishedEvent.HookBinding).Select(kvp => kvp.Value as HookStepTracker).LastOrDefault();
        }

    }
}