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
    internal class ScenarioState
    {
        private readonly IIdGenerator _idGenerator;

        public string TestCaseStartedID;
        public string Name { get; set; }
        public string TestCaseID { get; set; }
        public string PickleID { get; set; }
        public Pickle Pickle { get; set; }

        // we will hold all scenario and step events here until the scenario is finished, then use them to generate TestCase and TestStep messages
        private Queue<ExecutionEvent> _events = new();

        // this holds the step definitions bindings that were executed by each step in this scenario. this will be used to find the Cucumber stepDefinitions that were used
        private Queue<IStepDefinitionBinding> _stepBindingsAsUsed = new();

        public ScenarioState(IScenarioContext context, FeatureState featureState)
        {
            _idGenerator = featureState.IDGenerator;

            Name = context.ScenarioInfo.Title;
            TestCaseID = _idGenerator.GetNewId();
            Pickle = featureState.PicklesByScenarioName[Name];
            PickleID = featureState.PicklesByScenarioName[Name].Id;
        }

        internal IEnumerable<Envelope> ProcessEvent(ScenarioStartedEvent scenarioStartedEvent)
        {
            TestCaseStartedID = _idGenerator.GetNewId();
            _events.Enqueue(scenarioStartedEvent);
            return Enumerable.Empty<Envelope>();
        }

        internal IEnumerable<Envelope> ProcessEvent(StepStartedEvent stepStartedEvent)
        {
            _events.Enqueue(stepStartedEvent);
            return Enumerable.Empty<Envelope>();
        }

        internal IEnumerable<Envelope> ProcessEvent(StepFinishedEvent stepFinishedEvent)
        {
            _events.Enqueue(stepFinishedEvent);
            _stepBindingsAsUsed.Enqueue(stepFinishedEvent.StepContext.StepInfo.BindingMatch.StepBinding);

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
                        yield return Envelope.Create(CucumberMessageFactory.ToTestCaseFinished(this, scenarioFinished));
                        break;
                    // add more cases for other event types
                    default:
                        throw new ArgumentException($"Invalid event type: {executionEvent.GetType()}");
                }
            }

        }
    }
}