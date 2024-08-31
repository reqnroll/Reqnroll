using Cucumber.Messages;
using Gherkin.CucumberMessages;
using Io.Cucumber.Messages.Types;
using Reqnroll.Bindings;
using Reqnroll.BoDi;
using Reqnroll.Events;
using Reqnroll.Analytics;
using System;
using System.Collections;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Runtime.InteropServices;

namespace Reqnroll.CucumberMesssages
{
    public class FeatureEventProcessor
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
                return Enabled && Finished && ScenarioName2ScenarioProcessorMap.Values.All(s => s.ScenarioExecutionStatus == ScenarioExecutionStatus.OK);
            }
        }

        // ID Generator to use when generating IDs for TestCase messages and beyond
        // If gherkin feature was generated using integer IDs, then we will use an integer ID generator seeded with the last known integer ID
        // otherwise we'll use a GUID ID generator. We can't know ahead of time which type of ID generator to use, therefore this is not set by the constructor.
        public IIdGenerator IDGenerator { get; set; }

        //Lookup tables
        //
        // These three dictionaries hold the mapping of steps, hooks, and pickles to their IDs
        // These should only be produced by the first FeatureStartedEvent that this FeatureEventProcessor receives (it might receive multiple if the scenario is run concurrently)
        // therefore these are ConcurrentDictionary and we us the TryAdd method on them to only add each mapping once
        public ConcurrentDictionary<string, string> StepDefinitionsByPattern = new();
        public ConcurrentDictionary<string, string> HookDefinitionsByPattern = new();

        //TODO: fix this; there will be multiple  Pickles with the same scenario name when executing Example table rows 
        public ConcurrentDictionary<string, Io.Cucumber.Messages.Types.Pickle> PicklesByScenarioName = new();

        //TODO: Fix this for thread-safety; there will be multiple active Scenarios with the same name when executing Example table rows in parallel
        // Scenario event processors by scenario name; 
        public Dictionary<string, ScenarioEventProcessor> ScenarioName2ScenarioProcessorMap = new();

        // The list of Cucumber Messages that are ready to be sent to the broker for distribution to consumers
        public ConcurrentQueue<ReqnrollCucumberMessage> Messages = new();

        // A set of markers that represent the worker threads that are currently processing events for this feature.
        // Once the last worker thread marker is removed, the Messages are then sent to the broker
        public ConcurrentStack<int> workerThreadMarkers = new();

        internal void ProcessEvent(ExecutionEvent anEvent)
        {
            foreach (Envelope e in DispatchEvent(anEvent))
            {
                Messages.Enqueue(new ReqnrollCucumberMessage { CucumberMessageSource = Name, Envelope = e });
            }
        }
        private IEnumerable<Envelope> DispatchEvent(ExecutionEvent anEvent)
        {
            return anEvent switch
            {
                FeatureStartedEvent featureStartedEvent => ProcessEvent(featureStartedEvent),
                FeatureFinishedEvent featureFinishedEvent => ProcessEvent(featureFinishedEvent),
                ScenarioStartedEvent scenarioStartedEvent => ProcessEvent(scenarioStartedEvent),
                ScenarioFinishedEvent scenarioFinishedEvent => ProcessEvent(scenarioFinishedEvent),
                StepStartedEvent stepStartedEvent => ProcessEvent(stepStartedEvent),
                StepFinishedEvent stepFinishedEvent => ProcessEvent(stepFinishedEvent),
                HookBindingStartedEvent hookBindingStartedEvent => ProcessEvent(hookBindingStartedEvent),
                HookBindingFinishedEvent hookBindingFinishedEvent => ProcessEvent(hookBindingFinishedEvent),
                AttachmentAddedEvent attachmentAddedEvent => ProcessEvent(attachmentAddedEvent),
                OutputAddedEvent outputAddedEvent => ProcessEvent(outputAddedEvent),
                _ => throw new NotImplementedException(),
            };
        }

        internal IEnumerable<Envelope> ProcessEvent(FeatureStartedEvent featureStartedEvent)
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
                PicklesByScenarioName.TryAdd(pickle.Name, pickle);
                yield return Envelope.Create(pickle);
            }

            var bindingRegistry = featureStartedEvent.FeatureContext.FeatureContainer.Resolve<IBindingRegistry>();
            if (bindingRegistry.IsValid)
            {
                foreach (var stepTransform in bindingRegistry.GetStepTransformations())
                {
                    var parameterType = CucumberMessageFactory.ToParameterType(stepTransform, IDGenerator);
                    yield return Envelope.Create(parameterType);
                }
                foreach (var binding in bindingRegistry.GetStepDefinitions())
                {
                    var stepDefinition = CucumberMessageFactory.ToStepDefinition(binding, IDGenerator);
                    var pattern = CucumberMessageFactory.CanonicalizeStepDefinitionPattern(binding);
                    StepDefinitionsByPattern.TryAdd(pattern, stepDefinition.Id);

                    yield return Envelope.Create(stepDefinition);
                }

                foreach (var hookBinding in bindingRegistry.GetHooks())
                {
                    var hook = CucumberMessageFactory.ToHook(hookBinding, IDGenerator);
                    var hookId = CucumberMessageFactory.CanonicalizeHookBinding(hookBinding);
                    HookDefinitionsByPattern.TryAdd(hookId, hook.Id);
                    yield return Envelope.Create(hook);
                }
            }

            yield return Envelope.Create(CucumberMessageFactory.ToTestRunStarted(this, featureStartedEvent));

        }

        internal IEnumerable<Envelope> ProcessEvent(FeatureFinishedEvent featureFinishedEvent)
        {
            yield return Envelope.Create(CucumberMessageFactory.ToTestRunFinished(this, featureFinishedEvent));
        }

        private string GenerateScenarioKey(ScenarioInfo scenarioInfo)
        {
            var scenarioArguments = new List<string>();
            foreach (string v in scenarioInfo.Arguments.Values)
            {
                scenarioArguments.Add(v);
            }
            return scenarioInfo.Title
                 + scenarioArguments
                 + scenarioInfo.CombinedTags;
        }
        internal IEnumerable<Envelope> ProcessEvent(ScenarioStartedEvent scenarioStartedEvent)
        {
            var scenarioName = scenarioStartedEvent.ScenarioContext.ScenarioInfo.Title;
            var scenarioEP = new ScenarioEventProcessor(scenarioStartedEvent.ScenarioContext, this);
            ScenarioName2ScenarioProcessorMap.Add(scenarioName, scenarioEP);

            foreach (var e in scenarioEP.ProcessEvent(scenarioStartedEvent))
            {
                yield return e;
            }
        }

        internal IEnumerable<Envelope> ProcessEvent(ScenarioFinishedEvent scenarioFinishedEvent)
        {
            var scenarioName = scenarioFinishedEvent.ScenarioContext.ScenarioInfo.Title;
            var scenarioEP = ScenarioName2ScenarioProcessorMap[scenarioName];

            foreach (var e in scenarioEP.ProcessEvent(scenarioFinishedEvent))
            {
                yield return e;
            }
        }

        internal IEnumerable<Envelope> ProcessEvent(StepStartedEvent stepStartedEvent)
        {
            var scenarioName = stepStartedEvent.ScenarioContext.ScenarioInfo.Title;
            var scenarioEP = ScenarioName2ScenarioProcessorMap[scenarioName];

            foreach (var e in scenarioEP.ProcessEvent(stepStartedEvent))
            {
                yield return e;
            }
        }

        internal IEnumerable<Envelope> ProcessEvent(StepFinishedEvent stepFinishedEvent)
        {
            var scenarioName = stepFinishedEvent.ScenarioContext.ScenarioInfo.Title;
            var scenarioEP = ScenarioName2ScenarioProcessorMap[scenarioName];

            foreach (var e in scenarioEP.ProcessEvent(stepFinishedEvent))
            {
                yield return e;
            }
        }

        internal IEnumerable<Envelope> ProcessEvent(HookBindingStartedEvent hookStartedEvent)
        {
            var scenarioName = hookStartedEvent.ContextManager?.ScenarioContext?.ScenarioInfo?.Title;
            var scenarioEP = ScenarioName2ScenarioProcessorMap[scenarioName];

            foreach (var e in scenarioEP.ProcessEvent(hookStartedEvent))
            {
                yield return e;
            }
        }

        internal IEnumerable<Envelope> ProcessEvent(HookBindingFinishedEvent hookBindingFinishedEvent)
        {
            var scenarioName = hookBindingFinishedEvent.ContextManager?.ScenarioContext?.ScenarioInfo?.Title;
            var scenarioEP = ScenarioName2ScenarioProcessorMap[scenarioName];

            foreach (var e in scenarioEP.ProcessEvent(hookBindingFinishedEvent))
            {
                yield return e;
            }
        }

        internal IEnumerable<Envelope> ProcessEvent(AttachmentAddedEvent attachmentAddedEvent)
        {
            var scenarioName = attachmentAddedEvent.ScenarioName;
            var scenarioEP = ScenarioName2ScenarioProcessorMap[scenarioName];

            foreach (var e in scenarioEP.ProcessEvent(attachmentAddedEvent))
            {
                yield return e;
            }
        }

        internal IEnumerable<Envelope> ProcessEvent(OutputAddedEvent outputAddedEvent)
        {
            var scenarioName = outputAddedEvent.ScenarioName;
            var scenarioEP = ScenarioName2ScenarioProcessorMap[scenarioName];
            foreach (var e in scenarioEP.ProcessEvent(outputAddedEvent))
            {
                yield return e;
            }
        }

        private string ExtractLastID(List<Pickle> pickles)
        {
            return pickles.Last().Id;
        }

    }
}