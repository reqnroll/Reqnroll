using Gherkin.CucumberMessages;
using Io.Cucumber.Messages.Types;
using Reqnroll.Bindings;
using Reqnroll.CucumberMessages.PayloadProcessing.Cucumber;
using Reqnroll.CucumberMessages.RuntimeSupport;
using Reqnroll.Events;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text.RegularExpressions;

namespace Reqnroll.CucumberMessages.ExecutionTracking
{
    /// <summary>
    /// FeatureTracker is responsible for tracking the execution of a Feature.
    /// There will be one instance of this class per gherkin Feature.
    /// </summary>
    public class FeatureTracker
    {
        // Static Messages are those generated during code generation (Source, GherkinDocument & Pickles)
        // and the StepTransformations, StepDefinitions and Hook messages which are global to the entire Solution.
        internal IEnumerable<Envelope> StaticMessages => _staticMessages.Value;
        private Lazy<IEnumerable<Envelope>> _staticMessages; 
        // ID Generator to use when generating IDs for TestCase messages and beyond
        // If gherkin feature was generated using integer IDs, then we will use an integer ID generator seeded with the last known integer ID
        // otherwise we'll use a GUID ID generator. We can't know ahead of time which type of ID generator to use, therefore this is not set by the constructor.
        public IIdGenerator IDGenerator { get; set; }

        // This dictionary tracks the StepDefintions(ID) by their method signature
        // used during TestCase creation to map from a Step Definition binding to its ID
        internal Dictionary<string, string> StepDefinitionsByPattern = new();
        public string FeatureName { get; set; }
        public bool Enabled { get; private set; }

        // This dictionary maps from (string) PickleIDIndex to (string) PickleID
        public Dictionary<string, string> PickleIds { get; } = new();


        // This constructor is used by the Publisher when it sees a Feature (by name) for the first time
        public FeatureTracker(FeatureStartedEvent featureStartedEvent)
        {
            FeatureName = featureStartedEvent.FeatureContext.FeatureInfo.Title;
            var featureHasCucumberMessages = featureStartedEvent.FeatureContext.FeatureInfo.FeatureCucumberMessages != null;
            Enabled = featureHasCucumberMessages && featureStartedEvent.FeatureContext.FeatureInfo.FeatureCucumberMessages.Pickles != null ? true : false;
            PreProcessEvent(featureStartedEvent);
        }
        internal void PreProcessEvent(FeatureStartedEvent featureStartedEvent)
        {
            if (!Enabled) return;
            // This has side-effects needed for proper execution of subsequent events; eg, the Ids of the static messages get generated and then subsequent events generate Ids that follow
            _staticMessages = new Lazy<IEnumerable<Envelope>>(() => GenerateStaticMessages(featureStartedEvent));
        }
        private IEnumerable<Envelope> GenerateStaticMessages(FeatureStartedEvent featureStartedEvent)
        {
            yield return CucumberMessageFactory.ToMeta(featureStartedEvent);

            //Gherkin.CucumberMessages.Types.Source gherkinSource = featureStartedEvent.FeatureContext.FeatureInfo.FeatureCucumberMessages.Source;
            //Source messageSource = CucumberMessageTransformer.ToSource(gherkinSource);
            yield return Envelope.Create(featureStartedEvent.FeatureContext.FeatureInfo.FeatureCucumberMessages.Source);


            //Gherkin.CucumberMessages.Types.GherkinDocument gherkinDoc = featureStartedEvent.FeatureContext.FeatureInfo.FeatureCucumberMessages.GherkinDocument;
            //GherkinDocument gherkinDocument = CucumberMessageTransformer.ToGherkinDocument(gherkinDoc);
            yield return Envelope.Create(featureStartedEvent.FeatureContext.FeatureInfo.FeatureCucumberMessages.GherkinDocument);


            var pickles = featureStartedEvent.FeatureContext.FeatureInfo.FeatureCucumberMessages.Pickles.ToList();
            //var pickles = CucumberMessageTransformer.ToPickles(gherkinPickles);

            string lastID = ExtractLastID(pickles);
            IDGenerator = IdGeneratorFactory.Create(lastID);

            for(int i = 0; i < pickles.Count; i++)
            {
                PickleIds.Add(i.ToString(), pickles[i].Id);
            }

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
        }

        // This method is used to identify the last ID generated from the set generated during code gen. 
        // It takes advantage of the design assumption that Pickles are generated last, and that PickleSteps are generated before the ID of the Pickle itself.
        // Therefore, the ID of the last Pickle is last ID generated.
        // Subsequent Messages can be assigned IDs starting from that one (assuming incrementing integer IDs).
        // 
        // Note: Should the method of assigning IDs ever change (or their sequence of assignment) in the code generator, then this method may need to change as well.
        private string ExtractLastID(List<Pickle> pickles)
        {
            return pickles.Last().Id;
        }

    }
}