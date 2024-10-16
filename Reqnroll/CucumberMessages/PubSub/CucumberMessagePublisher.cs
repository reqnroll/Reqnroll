using Reqnroll.BoDi;
using Reqnroll.Events;
using Reqnroll.Tracing;
using Reqnroll.Plugins;
using Reqnroll.UnitTestProvider;
using System.Collections.Concurrent;
using System;
using System.Collections.Generic;
using System.Linq;
using Reqnroll.CucumberMessages.ExecutionTracking;
using Reqnroll.CucumberMessages.PayloadProcessing.Cucumber;
using Io.Cucumber.Messages.Types;
using Reqnroll.CucumberMessages.RuntimeSupport;
using Reqnroll.CucumberMessages.Configuration;
using Gherkin.CucumberMessages;
using Reqnroll.Bindings;

namespace Reqnroll.CucumberMessages.PubSub
{
    /// <summary>
    /// Cucumber Message Publisher
    /// This class is responsible for publishing CucumberMessages to the CucumberMessageBroker
    /// 
    /// It uses the set of ExecutionEvents to track overall execution of Features and steps and drive generation of messages
    /// 
    /// It uses the IRuntimePlugin interface to force the runtime to load it during startup (although it is not an external plugin per se).
    /// </summary>
    public class CucumberMessagePublisher : ICucumberMessagePublisher, IRuntimePlugin, IExecutionEventListener
    {
        private Lazy<ICucumberMessageBroker> _brokerFactory;
        private ICucumberMessageBroker _broker;
        private IObjectContainer objectContainer;

        // Started Features by name
        private ConcurrentDictionary<string, FeatureTracker> StartedFeatures = new();

        // This dictionary tracks the StepDefintions(ID) by their method signature
        // used during TestCase creation to map from a Step Definition binding to its ID
        // shared to each Feature tracker so that we keep a single list
        internal ConcurrentDictionary<string, string> StepDefinitionsByPattern = new();
        private ConcurrentBag<IStepArgumentTransformationBinding> StepArgumentTransforms = new();
        private ConcurrentBag<IStepDefinitionBinding> UndefinedParameterTypeBindings = new();
        public IIdGenerator SharedIDGenerator { get; private set; }


        bool Enabled = false;

        public CucumberMessagePublisher()
        {
        }
        public void Initialize(RuntimePluginEvents runtimePluginEvents, RuntimePluginParameters runtimePluginParameters, UnitTestProviderConfiguration unitTestProviderConfiguration)
        {
            runtimePluginEvents.CustomizeGlobalDependencies += (sender, args) =>
            {
                var pluginLifecycleEvents = args.ObjectContainer.Resolve<RuntimePluginTestExecutionLifecycleEvents>();
                pluginLifecycleEvents.BeforeTestRun += PublisherStartup;
                pluginLifecycleEvents.AfterTestRun += PublisherTestRunComplete;
            };
            runtimePluginEvents.CustomizeTestThreadDependencies += (sender, args) =>
              {
                  objectContainer = args.ObjectContainer;
                  _brokerFactory = new Lazy<ICucumberMessageBroker>(() => objectContainer.Resolve<ICucumberMessageBroker>());
                  var testThreadExecutionEventPublisher = args.ObjectContainer.Resolve<ITestThreadExecutionEventPublisher>();
                  HookIntoTestThreadExecutionEventPublisher(testThreadExecutionEventPublisher);
              };
        }

        public void HookIntoTestThreadExecutionEventPublisher(ITestThreadExecutionEventPublisher testThreadEventPublisher)
        {
            testThreadEventPublisher.AddListener(this);
        }

        public void OnEvent(IExecutionEvent executionEvent)
        {
            switch (executionEvent)
            {
                case FeatureStartedEvent featureStartedEvent:
                    FeatureStartedEventHandler(featureStartedEvent);
                    break;
                case FeatureFinishedEvent featureFinishedEvent:
                    FeatureFinishedEventHandler(featureFinishedEvent);
                    break;
                case ScenarioStartedEvent scenarioStartedEvent:
                    ScenarioStartedEventHandler(scenarioStartedEvent);
                    break;
                case ScenarioFinishedEvent scenarioFinishedEvent:
                    ScenarioFinishedEventHandler(scenarioFinishedEvent);
                    break;
                case StepStartedEvent stepStartedEvent:
                    StepStartedEventHandler(stepStartedEvent);
                    break;
                case StepFinishedEvent stepFinishedEvent:
                    StepFinishedEventHandler(stepFinishedEvent);
                    break;
                case HookBindingStartedEvent hookBindingStartedEvent:
                    HookBindingStartedEventHandler(hookBindingStartedEvent);
                    break;
                case HookBindingFinishedEvent hookBindingFinishedEvent:
                    HookBindingFinishedEventHandler(hookBindingFinishedEvent);
                    break;
                case AttachmentAddedEvent attachmentAddedEvent:
                    AttachmentAddedEventHandler(attachmentAddedEvent);
                    break;
                case OutputAddedEvent outputAddedEvent:
                    OutputAddedEventHandler(outputAddedEvent);
                    break;
                default:
                    break;
            }
        }

        // This method will get called after TestRunStartedEvent has been published and after any BeforeTestRun hooks have been called
        // The TestRunStartedEvent will be used by the FileOutputPlugin to launch the File writing thread and establish Messages configuration
        // Running this after the BeforeTestRun hooks will allow them to programmatically configure CucumberMessages
        private void PublisherStartup(object sender, RuntimePluginBeforeTestRunEventArgs args)
        {
            _broker = _brokerFactory.Value;

            Enabled = _broker.Enabled;

            if (!Enabled)
            {
                return;
            }

            SharedIDGenerator = IdGeneratorFactory.Create(CucumberConfiguration.Current.IDGenerationStyle);

            _broker.Publish(new ReqnrollCucumberMessage() { CucumberMessageSource = "startup", Envelope = Envelope.Create(CucumberMessageFactory.ToTestRunStarted(DateTime.Now)) });
            _broker.Publish(new ReqnrollCucumberMessage() { CucumberMessageSource = "startup", Envelope = CucumberMessageFactory.ToMeta(args.ObjectContainer) });
        }
        private void PublisherTestRunComplete(object sender, RuntimePluginAfterTestRunEventArgs e)
        {
            if (!Enabled)
                return;
            var status = StartedFeatures.Values.All(f => f.FeatureExecutionSuccess);
            _broker.Publish(new ReqnrollCucumberMessage() { CucumberMessageSource = "shutdown", Envelope = Envelope.Create(CucumberMessageFactory.ToTestRunFinished(status, DateTime.Now)) });
        }


        private void FeatureStartedEventHandler(FeatureStartedEvent featureStartedEvent)
        {
            _broker = _brokerFactory.Value;
            var traceListener = objectContainer.Resolve<ITraceListener>();

            var featureName = featureStartedEvent.FeatureContext?.FeatureInfo?.Title;

            Enabled = _broker.Enabled;
            if (!Enabled || String.IsNullOrEmpty(featureName))
            {
                return;
            }

            if (StartedFeatures.ContainsKey(featureName))
            {
                // Already started, don't repeat the following steps
                return;
            }

            var ft = new FeatureTracker(featureStartedEvent, SharedIDGenerator, StepDefinitionsByPattern, StepArgumentTransforms, UndefinedParameterTypeBindings);

            // This will add a FeatureTracker to the StartedFeatures dictionary only once, and if it is enabled, it will publish the static messages shared by all steps.
            if (StartedFeatures.TryAdd(featureName, ft) && ft.Enabled)
            {
                foreach (var msg in ft.StaticMessages)
                {
                    _broker.Publish(new ReqnrollCucumberMessage() { CucumberMessageSource = featureName, Envelope = msg });
                }
            }
        }

        private void FeatureFinishedEventHandler(FeatureFinishedEvent featureFinishedEvent)
        {
            // For this and subsequent events, we pull up the FeatureTracker by feature name.
            // If the feature name is not avaiable (such as might be the case in certain test setups), we ignore the event.
            var featureName = featureFinishedEvent.FeatureContext?.FeatureInfo?.Title;
            if (!Enabled || String.IsNullOrEmpty(featureName))
            {
                return;
            }
            if (!StartedFeatures.ContainsKey(featureName) || !StartedFeatures[featureName].Enabled)
            {
                return;
            }
            var featureTracker = StartedFeatures[featureName];
            featureTracker.ProcessEvent(featureFinishedEvent);

            // throw an exception if any of the TestCaseCucumberMessageTrackers are not done?

        }

        private void ScenarioStartedEventHandler(ScenarioStartedEvent scenarioStartedEvent)
        {
            var featureName = scenarioStartedEvent.FeatureContext?.FeatureInfo?.Title;
            if (!Enabled || String.IsNullOrEmpty(featureName))
                return;
            var traceListener = objectContainer.Resolve<ITraceListener>();
            if (StartedFeatures.TryGetValue(featureName, out var featureTracker))
            {
                if (featureTracker.Enabled)
                {
                    featureTracker.ProcessEvent(scenarioStartedEvent);
                }
                else
                {
                    return;
                }
            }
            else
            {
                traceListener.WriteTestOutput($"Cucumber Message Publisher: ScenarioStartedEventHandler: {featureName} FeatureTracker not available");
                throw new ApplicationException("FeatureTracker not available");
            }
        }

        private void ScenarioFinishedEventHandler(ScenarioFinishedEvent scenarioFinishedEvent)
        {
            var featureName = scenarioFinishedEvent.FeatureContext?.FeatureInfo?.Title;

            if (!Enabled || String.IsNullOrEmpty(featureName))
                return;
            if (StartedFeatures.TryGetValue(featureName, out var featureTracker))
            {
                foreach (var msg in featureTracker.ProcessEvent(scenarioFinishedEvent))
                {
                    _broker.Publish(new ReqnrollCucumberMessage() { CucumberMessageSource = featureName, Envelope = msg });
                }
            }
        }

        private void StepStartedEventHandler(StepStartedEvent stepStartedEvent)
        {
            var featureName = stepStartedEvent.FeatureContext?.FeatureInfo?.Title;

            if (!Enabled || String.IsNullOrEmpty(featureName))
                return;

            if (StartedFeatures.TryGetValue(featureName, out var featureTracker))
            {
                featureTracker.ProcessEvent(stepStartedEvent);
            }
        }

        private void StepFinishedEventHandler(StepFinishedEvent stepFinishedEvent)
        {
            var featureName = stepFinishedEvent.FeatureContext?.FeatureInfo?.Title;
            if (!Enabled || String.IsNullOrEmpty(featureName))
                return;

            if (StartedFeatures.TryGetValue(featureName, out var featureTracker))
            {
                featureTracker.ProcessEvent(stepFinishedEvent);
            }
        }

        private void HookBindingStartedEventHandler(HookBindingStartedEvent hookBindingStartedEvent)
        {
            var featureName = hookBindingStartedEvent.ContextManager?.FeatureContext?.FeatureInfo?.Title;
            if (!Enabled || String.IsNullOrEmpty(featureName))
                return;

            if (StartedFeatures.TryGetValue(featureName, out var featureTracker))
            {
                featureTracker.ProcessEvent(hookBindingStartedEvent);
            }
        }

        private void HookBindingFinishedEventHandler(HookBindingFinishedEvent hookBindingFinishedEvent)
        {
            var featureName = hookBindingFinishedEvent.ContextManager?.FeatureContext?.FeatureInfo?.Title;
            if (!Enabled || String.IsNullOrEmpty(featureName))
                return;

            if (StartedFeatures.TryGetValue(featureName, out var featureTracker))
            {
                featureTracker.ProcessEvent(hookBindingFinishedEvent);
            }
        }

        private void AttachmentAddedEventHandler(AttachmentAddedEvent attachmentAddedEvent)
        {
            var featureName = attachmentAddedEvent.FeatureInfo?.Title;
            if (!Enabled || String.IsNullOrEmpty(featureName))
                return;

            if (StartedFeatures.TryGetValue(featureName, out var featureTracker))
            {
                featureTracker.ProcessEvent(attachmentAddedEvent);
            }
        }

        private void OutputAddedEventHandler(OutputAddedEvent outputAddedEvent)
        {
            var featureName = outputAddedEvent.FeatureInfo?.Title;
            if (!Enabled || String.IsNullOrEmpty(featureName))
                return;

            if (StartedFeatures.TryGetValue(featureName, out var featureTracker))
            {
                featureTracker.ProcessEvent(outputAddedEvent);
            }
        }
    }
}
