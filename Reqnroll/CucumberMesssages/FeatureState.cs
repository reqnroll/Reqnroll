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
    internal class FeatureState
    {
        public string Name { get; set; }
        public bool Enabled { get; set; } //This will be false if the feature could not be pickled

        // These two flags are used to avoid duplicate messages being sent when Scenarios within Features are run concurrently
        // and multiple FeatureStartedEvent and FeatureFinishedEvent events are fired
        public bool Started { get; set; }
        public bool Finished { get; set; }

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
                    var pattern = CanonicalizeStepDefinitionPattern(stepDefinition);
                    StepDefinitionsByPattern.Add(pattern, stepDefinition.Id);

                    yield return Envelope.Create(stepDefinition);
                }
            }

            yield return Envelope.Create(CucumberMessageFactory.ToTestRunStarted(this, featureStartedEvent));

        }
        private string ExtractLastID(List<Pickle> pickles)
        {
            return pickles.Last().Id;
        }

        private string CanonicalizeStepDefinitionPattern(StepDefinition stepDefinition)
        {
            var sr = stepDefinition.SourceReference;
            var signature = sr.JavaMethod != null ? String.Join(",", sr.JavaMethod.MethodParameterTypes) : "";

            return $"{stepDefinition.Pattern}({signature})";
        }



    }
}