using Cucumber.Messages;
using Gherkin.CucumberMessages;
using Io.Cucumber.Messages.Types;
using Reqnroll.Bindings;
using Reqnroll.Events;
using System;
using System.Collections.Generic;
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

        internal IEnumerable<Envelope> ProcessEvent(HookBindingFinishedEvent hookBindingFinishedEvent)
        {
            // At this point we only care about hooks that wrap scenarios or steps
            if (hookBindingFinishedEvent.HookBinding.HookType == HookType.AfterFeature || hookBindingFinishedEvent.HookBinding.HookType == HookType.BeforeFeature
                || hookBindingFinishedEvent.HookBinding.HookType == HookType.BeforeTestRun || hookBindingFinishedEvent.HookBinding.HookType == HookType.AfterTestRun)
                return Enumerable.Empty<Envelope>();
            _events.Enqueue(hookBindingFinishedEvent);
            var step = new HookStepProcessor(this);
            step.ProcessEvent(hookBindingFinishedEvent);
            StepsByEvent.Add(hookBindingFinishedEvent, step);
            return Enumerable.Empty<Envelope>();
        }

        internal IEnumerable<Envelope> ProcessEvent(StepStartedEvent stepStartedEvent)
        {
            _events.Enqueue(stepStartedEvent);

            var stepState = new PickleStepProcessor(this);
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

        private PickleStepProcessor FindMatchingStepStartEvent(StepFinishedEvent stepFinishedEvent)
        {
            return StepsByEvent.Where(kvp => kvp.Key is StepStartedEvent).Where(kvp => ((StepStartedEvent) (kvp.Key)).StepContext == stepFinishedEvent.StepContext).OrderBy(kvp => ((StepStartedEvent)(kvp.Key)).Timestamp).Select(kvp => kvp.Value as PickleStepProcessor).LastOrDefault();
        }

        internal IEnumerable<Envelope> ProcessEvent(ScenarioFinishedEvent scenarioFinishedEvent)
        {
            _events.Enqueue(scenarioFinishedEvent);

            while (_events.Count > 0)
            {
                var executionEvent = _events.Dequeue();

                switch (executionEvent)
                {
                    case ScenarioStartedEvent scenarioStartedEvent:
                        yield return Envelope.Create(CucumberMessageFactory.ToTestCase(this, scenarioStartedEvent));
                        yield return Envelope.Create(CucumberMessageFactory.ToTestCaseStarted(this, scenarioStartedEvent));
                        break;
                    case ScenarioFinishedEvent scenarioFinished:
                        ScenarioExecutionStatus = scenarioFinished.ScenarioContext.ScenarioExecutionStatus;
                        yield return Envelope.Create(CucumberMessageFactory.ToTestCaseFinished(this, scenarioFinished));
                        break;
                    case StepStartedEvent stepStartedEvent:
                        var stepState = StepsByEvent[stepStartedEvent];
                        yield return Envelope.Create(CucumberMessageFactory.ToTestStepStarted(stepState as PickleStepProcessor, stepStartedEvent));
                        break;
                    case StepFinishedEvent stepFinishedEvent:
                        var stepFinishedState = StepsByEvent[stepFinishedEvent];
                        yield return Envelope.Create(CucumberMessageFactory.ToTestStepFinished(stepFinishedState as PickleStepProcessor, stepFinishedEvent));
                        break;
                    case HookBindingFinishedEvent hookBindingFinishedEvent:
                        var hookStepState = StepsByEvent[hookBindingFinishedEvent];
                        yield return Envelope.Create(CucumberMessageFactory.ToTestStepStarted(hookStepState as HookStepProcessor, hookBindingFinishedEvent));
                        yield return Envelope.Create(CucumberMessageFactory.ToTestStepFinished(hookStepState as HookStepProcessor, hookBindingFinishedEvent));
                        break;
                    // add more cases for other event types
                    default:
                        throw new ArgumentException($"Invalid event type: {executionEvent.GetType()}");
                }
            }

        }
    }
}