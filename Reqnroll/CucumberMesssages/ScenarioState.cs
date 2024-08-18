using Cucumber.Messages;
using Gherkin.CucumberMessages;
using Io.Cucumber.Messages.Types;
using Reqnroll.Events;
using System.Collections.Generic;

namespace Reqnroll.CucumberMesssages
{
    internal class ScenarioState
    {
        private readonly IIdGenerator _idGenerator;
        private string TestCaseStartedID;

        public string Name { get; set; }
        public string TestCaseID { get; set; }
        public string PickleID { get; set; }

        public ScenarioState(IScenarioContext context, FeatureState featureState)
        {
            _idGenerator = featureState.IDGenerator;

            Name = context.ScenarioInfo.Title;
            TestCaseID = _idGenerator.GetNewId();
            PickleID = featureState.PicklesByScenarioName[Name].Id;
        }

        internal IEnumerable<Envelope> ProcessEvent(ScenarioStartedEvent scenarioStartedEvent)
        {
            TestCaseStartedID = _idGenerator.GetNewId();

            //TODO: move Message creation to the CucumberMessageFactory
            yield return Envelope.Create(new TestCase(TestCaseID, PickleID, new List<TestStep>()));
            yield return Envelope.Create(new TestCaseStarted(0, TestCaseStartedID, TestCaseID, null, Converters.ToTimestamp(scenarioStartedEvent.Timestamp)));
        }

        internal IEnumerable<Envelope> ProcessEvent(ScenarioFinishedEvent scenarioFinishedEvent)
        {
            //TODO: move Message creation to the CucumberMessageFactory

            yield return Envelope.Create(new TestCaseFinished(TestCaseStartedID, Converters.ToTimestamp(scenarioFinishedEvent.Timestamp), false));
        }
    }
}