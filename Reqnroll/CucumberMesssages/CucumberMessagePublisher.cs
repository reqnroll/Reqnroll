using Reqnroll.BoDi;
using Reqnroll.Events;
using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;
using Io.Cucumber.Messages;
using Io.Cucumber.Messages.Types;
using Reqnroll.Tracing;
using Reqnroll.Plugins;
using Reqnroll.UnitTestProvider;
using Reqnroll.Time;
using Cucumber.Messages;
using Reqnroll.Bindings;
using System.Reflection;
using System.Collections.Concurrent;
using System.Linq;

namespace Reqnroll.CucumberMesssages
{
    public class CucumberMessagePublisher : ICucumberMessagePublisher, IRuntimePlugin
    {
        private ICucumberMessageBroker broker;
        private IObjectContainer objectContainer;
        private ConcurrentDictionary<string, FeatureState> featureStatesByFeatureName = new();

        public CucumberMessagePublisher(ICucumberMessageBroker CucumberMessageBroker, IObjectContainer objectContainer)
        {
            broker = CucumberMessageBroker;
        }
        public void Initialize(RuntimePluginEvents runtimePluginEvents, RuntimePluginParameters runtimePluginParameters, UnitTestProviderConfiguration unitTestProviderConfiguration)
        {
            runtimePluginEvents.CustomizeFeatureDependencies += (sender, args) =>
            {
                objectContainer = args.ObjectContainer;
                var testThreadExecutionEventPublisher = args.ObjectContainer.Resolve<ITestThreadExecutionEventPublisher>();
                HookIntoTestThreadExecutionEventPublisher(testThreadExecutionEventPublisher);
            };
        }

        public void HookIntoTestThreadExecutionEventPublisher(ITestThreadExecutionEventPublisher testThreadEventPublisher)
        {

            var traceListener = objectContainer.Resolve<ITraceListener>();
            traceListener.WriteTestOutput("HookIntoTestThreadExecutionEventPublisher");

            testThreadEventPublisher.AddHandler<FeatureStartedEvent>(FeatureStartedEventHandler);
            testThreadEventPublisher.AddHandler<FeatureFinishedEvent>(FeatureFinishedEventHandler);
            testThreadEventPublisher.AddHandler<ScenarioStartedEvent>(ScenarioStartedEventHandler);
            testThreadEventPublisher.AddHandler<ScenarioFinishedEvent>(ScenarioFinishedEventHandler);
        }

        private void FeatureFinishedEventHandler(FeatureFinishedEvent featureFinishedEvent)
        {
            var featureName = featureFinishedEvent.FeatureContext.FeatureInfo.Title;
            var featureState = featureStatesByFeatureName[featureName];
 
            lock (featureState)
            {
                // Remove the worker thread marker for this thread
                featureState.workerThreadMarkers.TryPop(out int result);

                // Check if there are other threads still working on this feature
                if (featureState.workerThreadMarkers.TryPeek(out result))
                {
                    // There are other threads still working on this feature, so we won't publish the TestRunFinished message just yet
                    return;
                }
            }


            if (!featureState.Enabled)
                return;

            var ts = objectContainer.Resolve<IClock>().GetNowDateAndTime();

            featureState.Messages.Enqueue(new ReqnrollCucumberMessage
            {
                CucumberMessageSource = featureName,
                //TODO: add feature status
                Envelope = Envelope.Create(new TestRunFinished(null, true, Converters.ToTimestamp(ts), null))
            });

            foreach (var message in featureState.Messages)
            {
                broker.Publish(message);
            }

            broker.Complete(featureName);
        }

        private void FeatureStartedEventHandler(FeatureStartedEvent featureStartedEvent)
        {
            var featureName = featureStartedEvent.FeatureContext.FeatureInfo.Title;
            var enabled = featureStartedEvent.FeatureContext.FeatureInfo.FeatureCucumberMessages.Source == null ? false : true;

            var featureState = new FeatureState
            {
                Name = featureName,
                Enabled = enabled
            };

            if (!featureStatesByFeatureName.TryAdd(featureName, featureState))
            {
                // This feature has already been started by another thread (executing a different scenario)
                var featureState_alreadyrunning = featureStatesByFeatureName[featureName];
                featureState_alreadyrunning.workerThreadMarkers.Push(1); // add a marker that this thread is active as well

                // None of the rest of this method should be executed
                return;
            }

            var traceListener = objectContainer.Resolve<ITraceListener>();
            traceListener.WriteTestOutput($"FeatureStartedEventHandler: {featureName}");

            if (!enabled)
                return;

            featureState.Messages.Enqueue(new ReqnrollCucumberMessage
            {
                CucumberMessageSource = featureName,
                Envelope = Envelope.Create(new Meta(
                    Cucumber.Messages.ProtocolVersion.Version,
                    new Product("placeholder", "placeholder"),
                    new Product("placeholder", "placeholder"),
                    new Product("placeholder", "placeholder"),
                    new Product("placeholder", "placeholder"),
                    null))
            });

            Gherkin.CucumberMessages.Types.Source gherkinSource = System.Text.Json.JsonSerializer.Deserialize<Gherkin.CucumberMessages.Types.Source>(featureStartedEvent.FeatureContext.FeatureInfo.FeatureCucumberMessages.Source);
            Io.Cucumber.Messages.Types.Source messageSource = CucumberMessageTransformer.ToSource(gherkinSource);
            featureState.Messages.Enqueue(new ReqnrollCucumberMessage
            {
                CucumberMessageSource = featureName,
                Envelope = Envelope.Create(messageSource)
            });

            Gherkin.CucumberMessages.Types.GherkinDocument gherkinDoc = System.Text.Json.JsonSerializer.Deserialize<Gherkin.CucumberMessages.Types.GherkinDocument>(featureStartedEvent.FeatureContext.FeatureInfo.FeatureCucumberMessages.GherkinDocument);
            GherkinDocument gherkinDocument = CucumberMessageTransformer.ToGherkinDocument(gherkinDoc);

            featureState.Messages.Enqueue(new ReqnrollCucumberMessage
            {
                CucumberMessageSource = featureName,
                Envelope = Envelope.Create(gherkinDocument)
            });

            var gherkinPickles = System.Text.Json.JsonSerializer.Deserialize<List<Gherkin.CucumberMessages.Types.Pickle>>(featureStartedEvent.FeatureContext.FeatureInfo.FeatureCucumberMessages.Pickles);
            var pickles = CucumberMessageTransformer.ToPickles(gherkinPickles);

            string lastID = ExtractID(pickles);
            featureState.IDGenerator = IdGeneratorFactory.Create(lastID);

            foreach (var pickle in pickles)
            {
                featureState.PicklesByScenarioName.Add(pickle.Name, pickle);

                featureState.Messages.Enqueue(new ReqnrollCucumberMessage
                {
                    CucumberMessageSource = featureName,
                    Envelope = Envelope.Create(pickle)
                });
            }

            var bindingRegistry = objectContainer.Resolve<IBindingRegistry>();
            if (bindingRegistry.IsValid)
            {
                foreach (var binding in bindingRegistry.GetStepDefinitions())
                {
                    var stepDefinition = CucumberMessageFactory.ToStepDefinition(binding, featureState.IDGenerator);
                    var pattern = CanonicalizeStepDefinitionPattern(stepDefinition);
                    featureState.StepDefinitionsByPattern.Add(pattern, stepDefinition.Id);

                    featureState.Messages.Enqueue(new ReqnrollCucumberMessage
                    {
                        CucumberMessageSource = featureName,
                        Envelope = Envelope.Create(stepDefinition)
                    });
                }
            }

            featureState.Messages.Enqueue(new ReqnrollCucumberMessage
            {
                CucumberMessageSource = featureName,
                Envelope = Envelope.Create(new TestRunStarted(Converters.ToTimestamp(featureStartedEvent.Timestamp)))
            });

        }

        private string CanonicalizeStepDefinitionPattern(StepDefinition stepDefinition)
        {
            var sr = stepDefinition.SourceReference;
            var signature = sr.JavaMethod != null ? String.Join(",", sr.JavaMethod.MethodParameterTypes) : "";

            return $"{stepDefinition.Pattern}({signature})";
        }

        private string ExtractID(List<Pickle> pickles)
        {
            return pickles.Last().Id;
        }

        private void ScenarioStartedEventHandler(ScenarioStartedEvent scenarioStartedEvent)
        {
            var featureName = scenarioStartedEvent.FeatureContext.FeatureInfo.Title;
            var scenarioName = scenarioStartedEvent.ScenarioContext.ScenarioInfo.Title;

            var featureState = featureStatesByFeatureName[featureName];

            var scenarioState = new ScenarioState(scenarioStartedEvent.ScenarioContext, featureState);
            featureState.ScenarioName2StateMap.Add(scenarioName, scenarioState);

            foreach (Envelope e in scenarioState.ProcessEvent(scenarioStartedEvent))
            {
                featureState.Messages.Enqueue(new ReqnrollCucumberMessage { CucumberMessageSource = featureName, Envelope = e });
            }
        }
        private void ScenarioFinishedEventHandler(ScenarioFinishedEvent scenarioFinishedEvent)
        {
            var featureName = scenarioFinishedEvent.FeatureContext.FeatureInfo.Title;
            var scenarioName = scenarioFinishedEvent.ScenarioContext.ScenarioInfo.Title;
            var featureState = featureStatesByFeatureName[featureName];
            var scenarioState = featureState.ScenarioName2StateMap[scenarioName];
            foreach (Envelope e in scenarioState.ProcessEvent(scenarioFinishedEvent))
            {
                featureState.Messages.Enqueue(new ReqnrollCucumberMessage { CucumberMessageSource = featureName, Envelope = e });
            }
        }


    }
}
