using Gherkin.CucumberMessages;
using Io.Cucumber.Messages.Types;
using Reqnroll.Bindings;
using Reqnroll.Events;
using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;

namespace Reqnroll.CucumberMessages
{
    public class FeatureTracker
    {
        internal IEnumerable<Envelope> StaticMessages;
        // ID Generator to use when generating IDs for TestCase messages and beyond
        // If gherkin feature was generated using integer IDs, then we will use an integer ID generator seeded with the last known integer ID
        // otherwise we'll use a GUID ID generator. We can't know ahead of time which type of ID generator to use, therefore this is not set by the constructor.
        public IIdGenerator IDGenerator { get; set; }
        // This dictionary tracks the StepDefintions(ID) by their method signature
        // used during TestCase creation to map from a Step Definition binding to its ID
        internal Dictionary<string, string> StepDefinitionsByPattern = new();
        public string FeatureName { get; set; }
        public bool Enabled { get; private set; }

        public FeatureTracker(FeatureStartedEvent featureStartedEvent)
        {
            FeatureName = featureStartedEvent.FeatureContext.FeatureInfo.Title;
            Enabled = featureStartedEvent.FeatureContext.FeatureInfo.FeatureCucumberMessages.Pickles == null ? false : true;
            PreProcessEvent(featureStartedEvent);
        }
        internal void PreProcessEvent(FeatureStartedEvent featureStartedEvent)
        {
            // This has side-effects needed for proper execution of subsequent events; eg, the Ids of the static messages get generated and then subsequent events generate Ids that follow
            StaticMessages = GenerateStaticMessages(featureStartedEvent).ToList();
        }
        private IEnumerable<Envelope> GenerateStaticMessages(FeatureStartedEvent featureStartedEvent)
        {
            yield return CucumberMessageFactory.ToMeta(featureStartedEvent);

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
                yield return Envelope.Create(pickle);
            }

            var bindingRegistry = featureStartedEvent.FeatureContext.FeatureContainer.Resolve<IBindingRegistry>();

            foreach (var stepTransform in bindingRegistry.GetStepTransformations())
            {
                var parameterType = CucumberMessageFactory.ToParameterType(stepTransform, IDGenerator);
                yield return Envelope.Create(parameterType);
            }

            foreach (var binding in bindingRegistry.GetStepDefinitions().Where(sd => !sd.IsValid))
            {
                var errmsg = binding.ErrorMessage;
                if (errmsg.Contains("Undefined parameter type"))
                {
                    var paramName = Regex.Match(errmsg, "Undefined parameter type '(.*)'").Groups[1].Value;
                    var undefinedParameterType = CucumberMessageFactory.ToUndefinedParameterType(binding.SourceExpression, paramName, IDGenerator);
                    yield return Envelope.Create(undefinedParameterType);
                }
            }

            foreach (var binding in bindingRegistry.GetStepDefinitions().Where(sd => sd.IsValid))
            {
                var stepDefinition = CucumberMessageFactory.ToStepDefinition(binding, IDGenerator);
                var pattern = CucumberMessageFactory.CanonicalizeStepDefinitionPattern(binding);
                if (!StepDefinitionsByPattern.ContainsKey(pattern)) StepDefinitionsByPattern.Add(pattern, stepDefinition.Id);

                yield return Envelope.Create(stepDefinition);
            }

            foreach (var hookBinding in bindingRegistry.GetHooks())
            {
                var hook = CucumberMessageFactory.ToHook(hookBinding, IDGenerator);
                var hookId = CucumberMessageFactory.CanonicalizeHookBinding(hookBinding);
                if (!StepDefinitionsByPattern.ContainsKey(hookId)) StepDefinitionsByPattern.Add(hookId, hook.Id);
                yield return Envelope.Create(hook);
            }

            yield return Envelope.Create(CucumberMessageFactory.ToTestRunStarted(featureStartedEvent));
        }
        private string ExtractLastID(List<Pickle> pickles)
        {
            return pickles.Last().Id;
        }

    }
}