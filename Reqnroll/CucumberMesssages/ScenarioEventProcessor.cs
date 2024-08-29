using Cucumber.Messages;
using Gherkin.CucumberMessages;
using Io.Cucumber.Messages.Types;
using Reqnroll.Bindings;
using Reqnroll.Events;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;

namespace Reqnroll.CucumberMesssages
{

    public class ScenarioEventProcessor
    {
        internal readonly IIdGenerator IdGenerator;
        internal readonly FeatureEventProcessor FeatureState;

        public string TestCaseStartedID;
        public string Name { get; set; }
        public string TestCaseID { get; set; }
        public string PickleID { get; set; }
        public Pickle Pickle { get; set; }
        private TestCase TestCase;

        private TestCaseStarted TestCaseStarted;

        // we will hold all scenario and step events here until the scenario is finished, then use them to generate TestCase and TestStep messages
        private Queue<ExecutionEvent> _events = new();

        public Dictionary<ExecutionEvent, StepProcessorBase> StepsByEvent { get; private set; } = new();
        public List<StepProcessorBase> Steps
        {
            get
            {
                return StepsByEvent.Where(kvp => kvp.Key is StepStartedEvent || kvp.Key is HookBindingFinishedEvent).Select(kvp => kvp.Value ).ToList();
            }
        }
        public ScenarioExecutionStatus ScenarioExecutionStatus { get; private set; }

        public ScenarioEventProcessor(IScenarioContext context, FeatureEventProcessor featureState)
        {
            IdGenerator = featureState.IDGenerator;
            FeatureState = featureState;

            Name = context.ScenarioInfo.Title;
            TestCaseID = IdGenerator.GetNewId();
            Pickle = featureState.PicklesByScenarioName[Name];
            PickleID = featureState.PicklesByScenarioName[Name].Id;
        }

        internal IEnumerable<Envelope> ProcessEvent(ScenarioStartedEvent scenarioStartedEvent)
        {
            TestCaseStartedID = IdGenerator.GetNewId();
            _events.Enqueue(scenarioStartedEvent);
            return Enumerable.Empty<Envelope>();
        }

        internal IEnumerable<Envelope> ProcessEvent(HookBindingStartedEvent hookBindingStartedEvent)
        {
            // At this point we only care about hooks that wrap scenarios or steps
            if (hookBindingStartedEvent.HookBinding.HookType == HookType.AfterFeature || hookBindingStartedEvent.HookBinding.HookType == HookType.BeforeFeature
                || hookBindingStartedEvent.HookBinding.HookType == HookType.BeforeTestRun || hookBindingStartedEvent.HookBinding.HookType == HookType.AfterTestRun)
                return Enumerable.Empty<Envelope>();
            _events.Enqueue(hookBindingStartedEvent);
            var step = new HookStepProcessor(this);
            step.ProcessEvent(hookBindingStartedEvent);
            StepsByEvent.Add(hookBindingStartedEvent, step);
            return Enumerable.Empty<Envelope>();
        }

        internal IEnumerable<Envelope> ProcessEvent(HookBindingFinishedEvent hookBindingFinishedEvent)
        {

            // At this point we only care about hooks that wrap scenarios or steps
            if (hookBindingFinishedEvent.HookBinding.HookType == HookType.AfterFeature || hookBindingFinishedEvent.HookBinding.HookType == HookType.BeforeFeature
                || hookBindingFinishedEvent.HookBinding.HookType == HookType.BeforeTestRun || hookBindingFinishedEvent.HookBinding.HookType == HookType.AfterTestRun)
                return Enumerable.Empty<Envelope>();

            _events.Enqueue(hookBindingFinishedEvent);
            var step = FindMatchingHookStartedEvent(hookBindingFinishedEvent);
            step.ProcessEvent(hookBindingFinishedEvent);
            StepsByEvent.Add(hookBindingFinishedEvent, step);
            return Enumerable.Empty<Envelope>();
        }

        private HookStepProcessor FindMatchingHookStartedEvent(HookBindingFinishedEvent hookBindingFinishedEvent)
        {
            return StepsByEvent.Where(kvp => kvp.Key is HookBindingStartedEvent && ((HookBindingStartedEvent)kvp.Key).HookBinding == hookBindingFinishedEvent.HookBinding).Select(kvp => kvp.Value as HookStepProcessor).LastOrDefault();
        }

        internal IEnumerable<Envelope> ProcessEvent(StepStartedEvent stepStartedEvent)
        {
            _events.Enqueue(stepStartedEvent);

            var stepState = new ScenarioStepProcessor(this);
            StepsByEvent.Add(stepStartedEvent, stepState);
            stepState.ProcessEvent(stepStartedEvent);

            return Enumerable.Empty<Envelope>();
        }

        internal IEnumerable<Envelope> ProcessEvent(StepFinishedEvent stepFinishedEvent)
        {
            _events.Enqueue(stepFinishedEvent);
            var stepState = FindMatchingStepStartEvent(stepFinishedEvent);
            stepState.ProcessEvent(stepFinishedEvent);
            StepsByEvent.Add(stepFinishedEvent, stepState);

            return Enumerable.Empty<Envelope>();
        }
        internal IEnumerable<Envelope> ProcessEvent(AttachmentAddedEvent attachmentAddedEvent)
        {
            //var matchingPickleStep = Pickle.Steps.Where(st => st.Text == attachmentAddedEvent.StepText).ToList().LastOrDefault();
            var pickleStepId = "";
   
            var attachmentExecutionEventWrapper = new AttachmentAddedEventWrapper(attachmentAddedEvent, pickleStepId);
            _events.Enqueue(attachmentExecutionEventWrapper);

            return Enumerable.Empty<Envelope>();
        }

        internal IEnumerable<Envelope> ProcessEvent(OutputAddedEvent outputAddedEvent)
        {
            //var matchingPickleStep = Pickle.Steps.Where(st => st.Text == outputAddedEvent.StepText).ToList().LastOrDefault();
            //var pickleStepId = matchingPickleStep.Id;

            var pickleStepId = "";
            var outputExecutionEventWrapper = new OutputAddedEventWrapper(outputAddedEvent, pickleStepId);
            _events.Enqueue(outputExecutionEventWrapper);

            return Enumerable.Empty<Envelope>();
        }

        private ScenarioStepProcessor FindMatchingStepStartEvent(StepFinishedEvent stepFinishedEvent)
        {
            return StepsByEvent.Where(kvp => kvp.Key is StepStartedEvent).Where(kvp => ((StepStartedEvent) (kvp.Key)).StepContext == stepFinishedEvent.StepContext).OrderBy(kvp => ((StepStartedEvent)(kvp.Key)).Timestamp).Select(kvp => kvp.Value as ScenarioStepProcessor).LastOrDefault();
        }

        internal IEnumerable<Envelope> ProcessEvent(ScenarioFinishedEvent scenarioFinishedEvent)
        {
            _events.Enqueue(scenarioFinishedEvent);
            TestStepStarted mostRecentTestStepStarted = null;

            while (_events.Count > 0)
            {
                var executionEvent = _events.Dequeue();

                switch (executionEvent)
                {
                    case ScenarioStartedEvent scenarioStartedEvent:
                        TestCase = CucumberMessageFactory.ToTestCase(this, scenarioStartedEvent);
                        TestCaseStarted = CucumberMessageFactory.ToTestCaseStarted(this, scenarioStartedEvent);
                        yield return Envelope.Create(TestCase);
                        yield return Envelope.Create(TestCaseStarted);
                        break;
                    case ScenarioFinishedEvent scenarioFinished:
                        ScenarioExecutionStatus = scenarioFinished.ScenarioContext.ScenarioExecutionStatus;
                        yield return Envelope.Create(CucumberMessageFactory.ToTestCaseFinished(this, scenarioFinished));
                        break;
                    case StepStartedEvent stepStartedEvent:
                        var stepState = StepsByEvent[stepStartedEvent];
                        var stepStarted = CucumberMessageFactory.ToTestStepStarted(stepState as ScenarioStepProcessor, stepStartedEvent);
                        mostRecentTestStepStarted = stepStarted;
                        yield return Envelope.Create(stepStarted);
                        break;
                    case StepFinishedEvent stepFinishedEvent:
                        var stepFinishedState = StepsByEvent[stepFinishedEvent];
                        yield return Envelope.Create(CucumberMessageFactory.ToTestStepFinished(stepFinishedState as ScenarioStepProcessor, stepFinishedEvent));
                        break;
                    //TODO: this isn't right; shouuld be one hook processor per hook that ran
                    case HookBindingStartedEvent hookBindingStartedEvent:
                        var hookStepStartState = StepsByEvent[hookBindingStartedEvent];
                        var hookStepStarted = CucumberMessageFactory.ToTestStepStarted(hookStepStartState as HookStepProcessor, hookBindingStartedEvent);
                        mostRecentTestStepStarted = hookStepStarted;
                        yield return Envelope.Create(hookStepStarted);
                        break;
                    case HookBindingFinishedEvent hookBindingFinishedEvent:
                        var hookStepFinishedState = StepsByEvent[hookBindingFinishedEvent];
                        yield return Envelope.Create(CucumberMessageFactory.ToTestStepFinished(hookStepFinishedState as HookStepProcessor, hookBindingFinishedEvent));
                        break;
                    case AttachmentAddedEventWrapper attachmentAddedEventWrapper:
                        // find the TestCaseStepId and testCaseStartedId
                        var testStepID = mostRecentTestStepStarted.TestStepId;
                        var testCaseStartedId = TestCaseStarted.Id;
                        attachmentAddedEventWrapper.TestCaseStepID = testStepID;
                        attachmentAddedEventWrapper.TestCaseStartedID = testCaseStartedId;
                        yield return Envelope.Create(CucumberMessageFactory.ToAttachment(this, attachmentAddedEventWrapper));
                        break;
                    case OutputAddedEventWrapper outputAddedEventWrapper:
                        // find the TestCaseStepId and testCaseStartedId
                        testStepID = mostRecentTestStepStarted.TestStepId;
                        testCaseStartedId = TestCaseStarted.Id;
                        outputAddedEventWrapper.TestCaseStepID = testStepID;
                        outputAddedEventWrapper.TestCaseStartedID = testCaseStartedId;
                        yield return Envelope.Create(CucumberMessageFactory.ToAttachment(this, outputAddedEventWrapper));
                        break;
                    // add more cases for other event types
                    default:
                        throw new ArgumentException($"Invalid event type: {executionEvent.GetType()}");
                }
            }

        }
    }
}