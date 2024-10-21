using Gherkin.CucumberMessages;
using Io.Cucumber.Messages.Types;
using Reqnroll.Bindings;
using Reqnroll.CucumberMessages.PayloadProcessing.Cucumber;
using Reqnroll.CucumberMessages.RuntimeSupport;
using Reqnroll.Events;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text.RegularExpressions;
using System.Threading.Tasks;

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
        // This dictionary is shared across all Features (via the Publisher)
        // The same is true of hte StepTransformations and StepDefinitionBindings used for Undefined Parameter Types
        internal ConcurrentDictionary<string, string> StepDefinitionsByPattern = new();
        private ConcurrentBag<IStepArgumentTransformationBinding> StepTransformRegistry;
        private ConcurrentBag<IStepDefinitionBinding> UndefinedParameterTypes;

        // This dictionary maps from (string) PickkleID to the TestCase tracker
        private ConcurrentDictionary<string, TestCaseTracker> testCaseTrackersById = new();

        public string FeatureName { get; set; }
        public bool Enabled { get; private set; }

        // This dictionary maps from (string) PickleIDIndex to (string) PickleID
        public Dictionary<string, string> PickleIds { get; } = new();
        public PickleJar PickleJar { get; set; }

        public bool FeatureExecutionSuccess { get; private set; }

        // This constructor is used by the Publisher when it sees a Feature (by name) for the first time
        public FeatureTracker(FeatureStartedEvent featureStartedEvent, IIdGenerator idGenerator, ConcurrentDictionary<string, string> stepDefinitionPatterns, ConcurrentBag<IStepArgumentTransformationBinding> stepTransformRegistry, ConcurrentBag<IStepDefinitionBinding> undefinedParameterTypes)
        {
            StepDefinitionsByPattern = stepDefinitionPatterns;
            StepTransformRegistry = stepTransformRegistry;
            UndefinedParameterTypes = undefinedParameterTypes;
            IDGenerator = idGenerator;
            FeatureName = featureStartedEvent.FeatureContext.FeatureInfo.Title;
            var featureHasCucumberMessages = featureStartedEvent.FeatureContext.FeatureInfo.FeatureCucumberMessages != null;
            Enabled = featureHasCucumberMessages && featureStartedEvent.FeatureContext.FeatureInfo.FeatureCucumberMessages.Pickles != null ? true : false;
            ProcessEvent(featureStartedEvent);
        }
        internal void ProcessEvent(FeatureStartedEvent featureStartedEvent)
        {
            if (!Enabled) return;
            // This has side-effects needed for proper execution of subsequent events; eg, the Ids of the static messages get generated and then subsequent events generate Ids that follow
            _staticMessages = new Lazy<IEnumerable<Envelope>>(() => GenerateStaticMessages(featureStartedEvent));
        }
        private IEnumerable<Envelope> GenerateStaticMessages(FeatureStartedEvent featureStartedEvent)
        {

            yield return Envelope.Create(featureStartedEvent.FeatureContext.FeatureInfo.FeatureCucumberMessages.Source());

            var gd = featureStartedEvent.FeatureContext.FeatureInfo.FeatureCucumberMessages.GherkinDocument();

            var pickles = featureStartedEvent.FeatureContext.FeatureInfo.FeatureCucumberMessages.Pickles().ToList();

            var idReWriter = new CucumberMessages.RuntimeSupport.IdReWriter();
            idReWriter.ReWriteIds(gd, pickles, IDGenerator, out var reWrittenGherkinDocument, out var reWrittenPickles);
            gd = reWrittenGherkinDocument;
            pickles = reWrittenPickles.ToList();

            for (int i = 0; i < pickles.Count; i++)
            {
                PickleIds.Add(i.ToString(), pickles[i].Id);
            }

            PickleJar = new PickleJar(pickles);

            yield return Envelope.Create(gd);
            foreach (var pickle in pickles)
            {
                yield return Envelope.Create(pickle);
            }

            var bindingRegistry = featureStartedEvent.FeatureContext.FeatureContainer.Resolve<IBindingRegistry>();

            foreach (var stepTransform in bindingRegistry.GetStepTransformations())
            {
                if (StepTransformRegistry.Contains(stepTransform))
                    continue;
                StepTransformRegistry.Add(stepTransform);
                var parameterType = CucumberMessageFactory.ToParameterType(stepTransform, IDGenerator);
                yield return Envelope.Create(parameterType);
            }

            foreach (var binding in bindingRegistry.GetStepDefinitions().Where(sd => !sd.IsValid))
            {
                var errmsg = binding.ErrorMessage;
                if (errmsg.Contains("Undefined parameter type"))
                {
                    var paramName = Regex.Match(errmsg, "Undefined parameter type '(.*)'").Groups[1].Value;
                    if (UndefinedParameterTypes.Contains(binding))
                        continue;
                    UndefinedParameterTypes.Add(binding);
                    var undefinedParameterType = CucumberMessageFactory.ToUndefinedParameterType(binding.SourceExpression, paramName, IDGenerator);
                    yield return Envelope.Create(undefinedParameterType);
                }
            }

            foreach (var binding in bindingRegistry.GetStepDefinitions().Where(sd => sd.IsValid))
            {
                var pattern = CucumberMessageFactory.CanonicalizeStepDefinitionPattern(binding);
                if (StepDefinitionsByPattern.ContainsKey(pattern))
                    continue;
                var stepDefinition = CucumberMessageFactory.ToStepDefinition(binding, IDGenerator);
                if (StepDefinitionsByPattern.TryAdd(pattern, stepDefinition.Id))
                {
                    yield return Envelope.Create(stepDefinition);
                }
            }

            foreach (var hookBinding in bindingRegistry.GetHooks())
            {
                var hookId = CucumberMessageFactory.CanonicalizeHookBinding(hookBinding);
                if (StepDefinitionsByPattern.ContainsKey(hookId))
                    continue;
                var hook = CucumberMessageFactory.ToHook(hookBinding, IDGenerator);
                if (StepDefinitionsByPattern.TryAdd(hookId, hook.Id))
                {
                    yield return Envelope.Create(hook);
                };
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

        // When the FeatureFinished event fires, we calculate the Feature-level Execution Status
        public void ProcessEvent(FeatureFinishedEvent featureFinishedEvent)
        {
            var testCases = testCaseTrackersById.Values.ToList();

            // Calculate the Feature-level Execution Status
            FeatureExecutionSuccess = testCases.All(tc => tc.Finished) switch
            {
                true => testCases.All(tc => tc.ScenarioExecutionStatus == ScenarioExecutionStatus.OK),
                _ => false
            };
        }

        public void ProcessEvent(ScenarioStartedEvent scenarioStartedEvent)
        {
            // as in the Publisher, we're using defensive coding here b/c some test setups might not have complete info
            var pickleIndex = scenarioStartedEvent.ScenarioContext?.ScenarioInfo?.PickleIdIndex;
            if (String.IsNullOrEmpty(pickleIndex)) return;

            if (PickleIds.TryGetValue(pickleIndex, out var pickleId))
            {
                // Fetch the PickleStepSequence for this Pickle and give to the ScenarioInfo
                var pickleStepSequence = PickleJar.PickleStepSequenceFor(pickleIndex);
                scenarioStartedEvent.ScenarioContext.ScenarioInfo.PickleStepSequence = pickleStepSequence; ;

                var tccmt = new TestCaseTracker(this, pickleId);
                tccmt.ProcessEvent(scenarioStartedEvent);
                testCaseTrackersById.TryAdd(pickleId, tccmt);
            }
        }

        public IEnumerable<Envelope> ProcessEvent(ScenarioFinishedEvent scenarioFinishedEvent)
        {
            var pickleIndex = scenarioFinishedEvent.ScenarioContext?.ScenarioInfo?.PickleIdIndex;
            if (String.IsNullOrEmpty(pickleIndex)) return Enumerable.Empty<Envelope>();

            if (PickleIds.TryGetValue(pickleIndex, out var pickleId))
            {
                if (testCaseTrackersById.TryGetValue(pickleId, out var tccmt))
                {
                    tccmt.ProcessEvent(scenarioFinishedEvent);

                    return tccmt.TestCaseCucumberMessages();
                }
            }
            return Enumerable.Empty<Envelope>();
        }

        public void ProcessEvent(StepStartedEvent stepStartedEvent)
        {
            var pickleIndex = stepStartedEvent.ScenarioContext?.ScenarioInfo?.PickleIdIndex;

            if (String.IsNullOrEmpty(pickleIndex)) return;

            if (PickleIds.TryGetValue(pickleIndex, out var pickleId))
            {
                if (testCaseTrackersById.TryGetValue(pickleId, out var tccmt))
                {
                    tccmt.ProcessEvent(stepStartedEvent);
                }
            }
        }

        public void ProcessEvent(StepFinishedEvent stepFinishedEvent)
        {
            var pickleIndex = stepFinishedEvent.ScenarioContext?.ScenarioInfo?.PickleIdIndex;

            if (String.IsNullOrEmpty(pickleIndex)) return;
            if (PickleIds.TryGetValue(pickleIndex, out var pickleId))
            {
                if (testCaseTrackersById.TryGetValue(pickleId, out var tccmt))
                {
                    tccmt.ProcessEvent(stepFinishedEvent);
                }
            }
        }

        public void ProcessEvent(HookBindingStartedEvent hookBindingStartedEvent)
        {
            var pickleIndex = hookBindingStartedEvent.ContextManager?.ScenarioContext?.ScenarioInfo?.PickleIdIndex;

            if (String.IsNullOrEmpty(pickleIndex)) return;

            if (PickleIds.TryGetValue(pickleIndex, out var pickleId))
            {
                if (testCaseTrackersById.TryGetValue(pickleId, out var tccmt))
                    tccmt.ProcessEvent(hookBindingStartedEvent);
            }
        }

        public void ProcessEvent(HookBindingFinishedEvent hookBindingFinishedEvent)
        {
            var pickleIndex = hookBindingFinishedEvent.ContextManager?.ScenarioContext?.ScenarioInfo?.PickleIdIndex;

            if (String.IsNullOrEmpty(pickleIndex)) return;

            if (PickleIds.TryGetValue(pickleIndex, out var pickleId))
            {
                if (testCaseTrackersById.TryGetValue(pickleId, out var tccmt))
                    tccmt.ProcessEvent(hookBindingFinishedEvent);
            }
        }

        public void ProcessEvent(AttachmentAddedEvent attachmentAddedEvent)
        {
            var pickleId = attachmentAddedEvent.FeatureInfo?.CucumberMessages_PickleId;

            if (String.IsNullOrEmpty(pickleId)) return;

            if (testCaseTrackersById.TryGetValue(pickleId, out var tccmt))
            {
                tccmt.ProcessEvent(attachmentAddedEvent);
            }
        }

        public void ProcessEvent(OutputAddedEvent outputAddedEvent)
        {
            var pickleId = outputAddedEvent.FeatureInfo?.CucumberMessages_PickleId;

            if (String.IsNullOrEmpty(pickleId)) return;

            if (testCaseTrackersById.TryGetValue(pickleId, out var tccmt))
            {
                tccmt.ProcessEvent(outputAddedEvent);
            }
        }
    }
}