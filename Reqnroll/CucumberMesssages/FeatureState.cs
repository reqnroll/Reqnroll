using Cucumber.Messages;
using Gherkin.CucumberMessages;
using Io.Cucumber.Messages.Types;
using Reqnroll.Bindings;
using Reqnroll.BoDi;
using Reqnroll.Events;
using System;
using System.Collections;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;

namespace Reqnroll.CucumberMesssages
{
    public class FeatureState
    {
        public string Name { get; set; }
        public bool Enabled { get; set; } //This will be false if the feature could not be pickled

        // These two flags are used to avoid duplicate messages being sent when Scenarios within Features are run concurrently
        // and multiple FeatureStartedEvent and FeatureFinishedEvent events are fired
        public bool Started { get; set; }
        public bool Finished { get; set; }

        public bool Success
        {
            get
            {
                return Enabled && Finished && ScenarioName2StateMap.Values.All(s => s.ScenarioExecutionStatus == ScenarioExecutionStatus.OK) ;
            }
        }

        //ID Generator to use when generating IDs for TestCase messages and beyond
        // If gherkin feature was generated using integer IDs, then we will use an integer ID generator seeded with the last known integer ID
        // otherwise we'll use a GUID ID generator
        public IIdGenerator IDGenerator { get; set; }

        //Lookup tables
        public Dictionary<string, string> StepDefinitionsByPattern = new();
        public Dictionary<string, Io.Cucumber.Messages.Types.Pickle> PicklesByScenarioName = new();

        public Dictionary<string, ScenarioState> ScenarioName2StateMap = new();

        public ConcurrentQueue<ReqnrollCucumberMessage> Messages = new();
        public ConcurrentStack<int> workerThreadMarkers = new();

        internal IEnumerable<Envelope> ProcessEvent(FeatureStartedEvent featureStartedEvent)
        {
            yield return Envelope.Create(new Meta(
                    Cucumber.Messages.ProtocolVersion.Version,
                    new Product("placeholder", "placeholder"),
                    new Product("placeholder", "placeholder"),
                    new Product("placeholder", "placeholder"),
                    new Product("placeholder", "placeholder"),
                    null));

            Gherkin.CucumberMessages.Types.Source gherkinSource = System.Text.Json.JsonSerializer.Deserialize<Gherkin.CucumberMessages.Types.Source>(featureStartedEvent.FeatureContext.FeatureInfo.FeatureCucumberMessages.Source);
            Io.Cucumber.Messages.Types.Source messageSource = CucumberMessageTransformer.ToSource(gherkinSource);
            yield return Envelope.Create(messageSource);


            Gherkin.CucumberMessages.Types.GherkinDocument gherkinDoc = System.Text.Json.JsonSerializer.Deserialize<Gherkin.CucumberMessages.Types.GherkinDocument>(featureStartedEvent.FeatureContext.FeatureInfo.FeatureCucumberMessages.GherkinDocument);
            GherkinDocument gherkinDocument = CucumberMessageTransformer.ToGherkinDocument(gherkinDoc);
            yield return Envelope.Create(gherkinDocument);


            var gherkinPickles = System.Text.Json.JsonSerializer.Deserialize<List<Gherkin.CucumberMessages.Types.Pickle>>(featureStartedEvent.FeatureContext.FeatureInfo.FeatureCucumberMessages.Pickles);
            var pickles = CucumberMessageTransformer.ToPickles(gherkinPickles);

            string lastID = ExtractLastID(pickles);
            IDGenerator = IdGeneratorFactory.Create(lastID);

            foreach (var pickle in pickles)
            {
                PicklesByScenarioName.Add(pickle.Name, pickle);
                yield return Envelope.Create(pickle);
            }

            var bindingRegistry = featureStartedEvent.FeatureContext.FeatureContainer.Resolve<IBindingRegistry>();
            if (bindingRegistry.IsValid)
            {
                foreach (var binding in bindingRegistry.GetStepDefinitions())
                {
                    var stepDefinition = CucumberMessageFactory.ToStepDefinition(binding, IDGenerator);
                    var pattern = CucumberMessageFactory.CanonicalizeStepDefinitionPattern(binding);
                    StepDefinitionsByPattern.Add(pattern, stepDefinition.Id);

                    yield return Envelope.Create(stepDefinition);
                }
            }

            yield return Envelope.Create(CucumberMessageFactory.ToTestRunStarted(this, featureStartedEvent));

        }


        internal IEnumerable<Envelope> ProcessEvent(FeatureFinishedEvent featureFinishedEvent)
        {
            yield return Envelope.Create(CucumberMessageFactory.ToTestRunFinished(this, featureFinishedEvent));
        }

        internal IEnumerable<Envelope> ProcessEvent(ScenarioStartedEvent scenarioStartedEvent)
        {
            var scenarioName = scenarioStartedEvent.ScenarioContext.ScenarioInfo.Title;
            var scenarioState = new ScenarioState(scenarioStartedEvent.ScenarioContext, this);
            ScenarioName2StateMap.Add(scenarioName, scenarioState);

            foreach (var e in scenarioState.ProcessEvent(scenarioStartedEvent))
            {
                yield return e;
            }
        }

        private string ExtractLastID(List<Pickle> pickles)
        {
            return pickles.Last().Id;
        }

        internal IEnumerable<Envelope> ProcessEvent(ScenarioFinishedEvent scenarioFinishedEvent)
        {
            var scenarioName = scenarioFinishedEvent.ScenarioContext.ScenarioInfo.Title;
            var scenarioState = ScenarioName2StateMap[scenarioName];

            foreach (var e in scenarioState.ProcessEvent(scenarioFinishedEvent))
            {
                yield return e;
            }
        }

        internal IEnumerable<Envelope> ProcessEvent(StepStartedEvent stepStartedEvent)
        {
            var scenarioName = stepStartedEvent.ScenarioContext.ScenarioInfo.Title;
            var scenarioState = ScenarioName2StateMap[scenarioName];

            foreach (var e in scenarioState.ProcessEvent(stepStartedEvent))
            {
                yield return e;
            }
        }

        internal IEnumerable<Envelope> ProcessEvent(StepFinishedEvent stepFinishedEvent)
        {
            var scenarioName = stepFinishedEvent.ScenarioContext.ScenarioInfo.Title;
            var scenarioState = ScenarioName2StateMap[scenarioName];

            foreach (var e in scenarioState.ProcessEvent(stepFinishedEvent))
            {
                yield return e;
            }
        }
    }
}