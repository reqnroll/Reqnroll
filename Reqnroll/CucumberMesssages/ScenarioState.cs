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

        public string TestCaseStartedID;
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

            yield return Envelope.Create(CucumberMessageFactory.ToTestCase(this, scenarioStartedEvent));
            yield return Envelope.Create(CucumberMessageFactory.ToTestCaseStarted(this, scenarioStartedEvent));
        }

        internal IEnumerable<Envelope> ProcessEvent(ScenarioFinishedEvent scenarioFinishedEvent)
        {
            yield return Envelope.Create(CucumberMessageFactory.ToTestCaseFinished(this, scenarioFinishedEvent));
        }
    }
}