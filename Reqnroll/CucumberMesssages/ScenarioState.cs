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
    public class ScenarioState
    {
        internal readonly IIdGenerator IdGenerator;
        internal readonly FeatureState FeatureState;

        public string TestCaseStartedID;
        public string Name { get; set; }
        public string TestCaseID { get; set; }
        public string PickleID { get; set; }
        public Pickle Pickle { get; set; }

        // we will hold all scenario and step events here until the scenario is finished, then use them to generate TestCase and TestStep messages
        private Queue<ExecutionEvent> _events = new();

        public Dictionary<ExecutionEvent, StepState> StepsByEvent { get; set; } = new();
        public List<StepState> Steps
        {
            get
            {
                return StepsByEvent.Where(kvp => kvp.Key is StepStartedEvent).Select(kvp => kvp.Value).ToList();
            }
        }
        public ScenarioExecutionStatus ScenarioExecutionStatus { get; private set; }

        public ScenarioState(IScenarioContext context, FeatureState featureState)
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

        internal IEnumerable<Envelope> ProcessEvent(StepStartedEvent stepStartedEvent)
        {
            _events.Enqueue(stepStartedEvent);

            var stepState = new StepState(this, stepStartedEvent);
            StepsByEvent.Add(stepStartedEvent, stepState);
            stepState.ProcessEvent(stepStartedEvent);

            return Enumerable.Empty<Envelope>();
        }

        internal IEnumerable<Envelope> ProcessEvent(StepFinishedEvent stepFinishedEvent)
        {
            _events.Enqueue(stepFinishedEvent);
            var stepState = StepsByEvent.Values.Last();
            stepState.ProcessEvent(stepFinishedEvent);
            StepsByEvent.Add(stepFinishedEvent, stepState);

            return Enumerable.Empty<Envelope>();
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
                        yield return Envelope.Create(CucumberMessageFactory.ToTestStepStarted(stepState, stepStartedEvent));
                        break;
                    case StepFinishedEvent stepFinishedEvent:
                        var stepFinishedState = StepsByEvent[stepFinishedEvent];
                        yield return Envelope.Create(CucumberMessageFactory.ToTestStepFinished(stepFinishedState, stepFinishedEvent));
                        break;
                    // add more cases for other event types
                    default:
                        throw new ArgumentException($"Invalid event type: {executionEvent.GetType()}");
                }
            }

        }
    }
}