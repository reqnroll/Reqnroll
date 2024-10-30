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
using System.Threading.Tasks;
using Reqnroll.EnvironmentAccess;
using System.Text.RegularExpressions;

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
    public class CucumberMessagePublisher : ICucumberMessagePublisher, IRuntimePlugin, IAsyncExecutionEventListener
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

        private string _testRunStartedId;
        bool Enabled = false;

        // This tracks the set of BeforeTestRun and AfterTestRun hooks that were called during the test run
        private readonly ConcurrentDictionary<string, TestRunHookTracker> TestRunHookTrackers = new();

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
            testThreadEventPublisher.AddAsyncListener(this);
        }

        public async Task OnEventAsync(IExecutionEvent executionEvent)
        {
            switch (executionEvent)
            {
                case FeatureStartedEvent featureStartedEvent:
                    await FeatureStartedEventHandler(featureStartedEvent);
                    break;
                case FeatureFinishedEvent featureFinishedEvent:
                    await FeatureFinishedEventHandler(featureFinishedEvent);
                    break;
                case ScenarioStartedEvent scenarioStartedEvent:
                    await ScenarioStartedEventHandler(scenarioStartedEvent);
                    break;
                case ScenarioFinishedEvent scenarioFinishedEvent:
                    await ScenarioFinishedEventHandler(scenarioFinishedEvent);
                    break;
                case StepStartedEvent stepStartedEvent:
                    await StepStartedEventHandler(stepStartedEvent);
                    break;
                case StepFinishedEvent stepFinishedEvent:
                    await StepFinishedEventHandler(stepFinishedEvent);
                    break;
                case HookBindingStartedEvent hookBindingStartedEvent:
                    await HookBindingStartedEventHandler(hookBindingStartedEvent);
                    break;
                case HookBindingFinishedEvent hookBindingFinishedEvent:
                    await HookBindingFinishedEventHandler(hookBindingFinishedEvent);
                    break;
                case AttachmentAddedEvent attachmentAddedEvent:
                    await AttachmentAddedEventHandler(attachmentAddedEvent);
                    break;
                case OutputAddedEvent outputAddedEvent:
                    await OutputAddedEventHandler(outputAddedEvent);
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
            _testRunStartedId = SharedIDGenerator.GetNewId();

            Task.Run(async () =>
            {
                await _broker.PublishAsync(new ReqnrollCucumberMessage() { CucumberMessageSource = "startup", Envelope = Envelope.Create(CucumberMessageFactory.ToTestRunStarted(DateTime.Now, _testRunStartedId)) });
                await _broker.PublishAsync(new ReqnrollCucumberMessage() { CucumberMessageSource = "startup", Envelope = CucumberMessageFactory.ToMeta(args.ObjectContainer) });
                foreach (var msg in PopulateBindingCachesAndGenerateBindingMessages(args.ObjectContainer))
                {
                    await _broker.PublishAsync(new ReqnrollCucumberMessage() { CucumberMessageSource = "startup", Envelope = msg });
                }
            }).Wait();
        }
        private IEnumerable<Envelope> PopulateBindingCachesAndGenerateBindingMessages(IObjectContainer objectContainer)
        {
            var bindingRegistry = objectContainer.Resolve<IBindingRegistry>();

            foreach (var stepTransform in bindingRegistry.GetStepTransformations())
            {
                if (StepArgumentTransforms.Contains(stepTransform))
                    continue;
                StepArgumentTransforms.Add(stepTransform);
                var parameterType = CucumberMessageFactory.ToParameterType(stepTransform, SharedIDGenerator);
                yield return Envelope.Create(parameterType);
            }

            foreach (var binding in bindingRegistry.GetStepDefinitions().Where(sd => !sd.IsValid))
            {
                var errmsg = binding.ErrorMessage;
                if (errmsg.Contains("Undefined parameter type"))
                {
                    var paramName = Regex.Match(errmsg, "Undefined parameter type '(.*)'").Groups[1].Value;
                    if (UndefinedParameterTypeBindings.Contains(binding))
                        continue;
                    UndefinedParameterTypeBindings.Add(binding);
                    var undefinedParameterType = CucumberMessageFactory.ToUndefinedParameterType(binding.SourceExpression, paramName, SharedIDGenerator);
                    yield return Envelope.Create(undefinedParameterType);
                }
            }

            foreach (var binding in bindingRegistry.GetStepDefinitions().Where(sd => sd.IsValid))
            {
                var pattern = CucumberMessageFactory.CanonicalizeStepDefinitionPattern(binding);
                if (StepDefinitionsByPattern.ContainsKey(pattern))
                    continue;
                var stepDefinition = CucumberMessageFactory.ToStepDefinition(binding, SharedIDGenerator);
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
                var hook = CucumberMessageFactory.ToHook(hookBinding, SharedIDGenerator);
                if (StepDefinitionsByPattern.TryAdd(hookId, hook.Id))
                {
                    yield return Envelope.Create(hook);
                };
            }

        }
        private void PublisherTestRunComplete(object sender, RuntimePluginAfterTestRunEventArgs e)
        {
            if (!Enabled)
                return;
            var status = StartedFeatures.Values.All(f => f.FeatureExecutionSuccess);
            
            Task.Run(async () => 
                await _broker.PublishAsync(new ReqnrollCucumberMessage() { CucumberMessageSource = "shutdown", Envelope = Envelope.Create(CucumberMessageFactory.ToTestRunFinished(status, DateTime.Now, _testRunStartedId)) })
                ).Wait();
        }

        #region TestThreadExecutionEventPublisher Event Handling Methods
        // The following methods handle the events published by the TestThreadExecutionEventPublisher
        // When one of these calls the Broker, that method is async; otherwise these are sync methods that return a completed Task (to allow them to be called async from the TestThreadExecutionEventPublisher)
        private async Task FeatureStartedEventHandler(FeatureStartedEvent featureStartedEvent)
        {
            _broker = _brokerFactory.Value;
            var traceListener = objectContainer.Resolve<ITraceListener>();

            var featureName = featureStartedEvent.FeatureContext?.FeatureInfo?.Title;

            Enabled = _broker.Enabled;
            if (!Enabled || String.IsNullOrEmpty(featureName))
            {
                return;
            }

            // The following should be thread safe when multiple instances of the Test Class are running in parallel. 
            // If StartedFeatures.ContainsKey returns true, then we know another instance of this Feature class has already started. We don't need a second instance of the 
            // FeatureTracker, and we don't want multiple copies of the static messages to be published.
            if (StartedFeatures.ContainsKey(featureName))
            {
                // Already started, don't repeat the following steps
                return;
            }

            // Creating multiple copies of the same FeatureTracker is safe as it causes no side-effects.
            // If two or more threads are running this code simultaneously, all but one of them will get created but then will be ignored.
            var ft = new FeatureTracker(featureStartedEvent, _testRunStartedId, SharedIDGenerator, StepDefinitionsByPattern, StepArgumentTransforms, UndefinedParameterTypeBindings);

            // This will add a FeatureTracker to the StartedFeatures dictionary only once, and if it is enabled, it will publish the static messages shared by all steps.
            if (StartedFeatures.TryAdd(featureName, ft) && ft.Enabled)
            {
                foreach (var msg in ft.StaticMessages)
                {
                    await _broker.PublishAsync(new ReqnrollCucumberMessage() { CucumberMessageSource = featureName, Envelope = msg });
                }
            }
        }

        private Task FeatureFinishedEventHandler(FeatureFinishedEvent featureFinishedEvent)
        {
            // For this and subsequent events, we pull up the FeatureTracker by feature name.
            // If the feature name is not avaiable (such as might be the case in certain test setups), we ignore the event.
            var featureName = featureFinishedEvent.FeatureContext?.FeatureInfo?.Title;
            if (!Enabled || String.IsNullOrEmpty(featureName))
            {
                return Task.CompletedTask;
            }
            if (!StartedFeatures.ContainsKey(featureName) || !StartedFeatures[featureName].Enabled)
            {
                return Task.CompletedTask;
            }
            var featureTracker = StartedFeatures[featureName];
            featureTracker.ProcessEvent(featureFinishedEvent);
            return Task.CompletedTask;
            // throw an exception if any of the TestCaseCucumberMessageTrackers are not done?

        }

        private Task ScenarioStartedEventHandler(ScenarioStartedEvent scenarioStartedEvent)
        {
            var featureName = scenarioStartedEvent.FeatureContext?.FeatureInfo?.Title;
            if (!Enabled || String.IsNullOrEmpty(featureName))
                return Task.CompletedTask;
            var traceListener = objectContainer.Resolve<ITraceListener>();
            if (StartedFeatures.TryGetValue(featureName, out var featureTracker))
            {
                if (featureTracker.Enabled)
                {
                    featureTracker.ProcessEvent(scenarioStartedEvent);
                }
                else
                {
                    return Task.CompletedTask;
                }
            }
            else
            {
                traceListener.WriteTestOutput($"Cucumber Message Publisher: ScenarioStartedEventHandler: {featureName} FeatureTracker not available");
                throw new ApplicationException("FeatureTracker not available");
            }

            return Task.CompletedTask;
        }

        private async Task ScenarioFinishedEventHandler(ScenarioFinishedEvent scenarioFinishedEvent)
        {
            var featureName = scenarioFinishedEvent.FeatureContext?.FeatureInfo?.Title;

            if (!Enabled || String.IsNullOrEmpty(featureName))
                return;
            if (StartedFeatures.TryGetValue(featureName, out var featureTracker))
            {
                foreach (var msg in featureTracker.ProcessEvent(scenarioFinishedEvent))
                {
                    await _broker.PublishAsync(new ReqnrollCucumberMessage() { CucumberMessageSource = featureName, Envelope = msg });
                }
            }
        }

        private Task StepStartedEventHandler(StepStartedEvent stepStartedEvent)
        {
            var featureName = stepStartedEvent.FeatureContext?.FeatureInfo?.Title;

            if (!Enabled || String.IsNullOrEmpty(featureName))
                return Task.CompletedTask;

            if (StartedFeatures.TryGetValue(featureName, out var featureTracker))
            {
                featureTracker.ProcessEvent(stepStartedEvent);
            }

            return Task.CompletedTask;
        }

        private Task StepFinishedEventHandler(StepFinishedEvent stepFinishedEvent)
        {
            var featureName = stepFinishedEvent.FeatureContext?.FeatureInfo?.Title;
            if (!Enabled || String.IsNullOrEmpty(featureName))
                return Task.CompletedTask;

            if (StartedFeatures.TryGetValue(featureName, out var featureTracker))
            {
                featureTracker.ProcessEvent(stepFinishedEvent);
            }

            return Task.CompletedTask;
        }

        private async Task HookBindingStartedEventHandler(HookBindingStartedEvent hookBindingStartedEvent)
        {
            if (hookBindingStartedEvent.HookBinding.HookType == Bindings.HookType.BeforeTestRun || hookBindingStartedEvent.HookBinding.HookType == Bindings.HookType.AfterTestRun)
            {
                string hookId = SharedIDGenerator.GetNewId();
                var hookTracker = new TestRunHookTracker(hookId, hookBindingStartedEvent, _testRunStartedId);
                TestRunHookTrackers.TryAdd(hookTracker.HookBindingSignature, hookTracker);
                await _broker.PublishAsync(new ReqnrollCucumberMessage() { CucumberMessageSource = "testRunHook", Envelope = CucumberMessageFactory.ToTestRunHookStarted(hookTracker) });
                return;
            }

            var featureName = hookBindingStartedEvent.ContextManager?.FeatureContext?.FeatureInfo?.Title;
            if (!Enabled || String.IsNullOrEmpty(featureName))
                return;

            if (StartedFeatures.TryGetValue(featureName, out var featureTracker))
            {
                featureTracker.ProcessEvent(hookBindingStartedEvent);
            }

            return;
        }

        private async Task HookBindingFinishedEventHandler(HookBindingFinishedEvent hookBindingFinishedEvent)
        {
            if (hookBindingFinishedEvent.HookBinding.HookType == Bindings.HookType.BeforeTestRun || hookBindingFinishedEvent.HookBinding.HookType == Bindings.HookType.AfterTestRun)
            {
                var signature = CucumberMessageFactory.CanonicalizeHookBinding(hookBindingFinishedEvent.HookBinding);
                if (!TestRunHookTrackers.TryGetValue(signature, out var hookTracker))
                    return;
                await _broker.PublishAsync(new ReqnrollCucumberMessage() { CucumberMessageSource = "testRunHook", Envelope = CucumberMessageFactory.ToTestRunHookFinished(hookTracker) });
                return;
            }


            var featureName = hookBindingFinishedEvent.ContextManager?.FeatureContext?.FeatureInfo?.Title;
            if (!Enabled || String.IsNullOrEmpty(featureName))
                return;

            if (StartedFeatures.TryGetValue(featureName, out var featureTracker))
            {
                featureTracker.ProcessEvent(hookBindingFinishedEvent);
            }

            return;
        }

        private Task AttachmentAddedEventHandler(AttachmentAddedEvent attachmentAddedEvent)
        {
            var featureName = attachmentAddedEvent.FeatureInfo?.Title;
            if (!Enabled || String.IsNullOrEmpty(featureName))
                return Task.CompletedTask;

            if (StartedFeatures.TryGetValue(featureName, out var featureTracker))
            {
                featureTracker.ProcessEvent(attachmentAddedEvent);
            }

            return Task.CompletedTask;
        }

        private Task OutputAddedEventHandler(OutputAddedEvent outputAddedEvent)
        {
            var featureName = outputAddedEvent.FeatureInfo?.Title;
            if (!Enabled || String.IsNullOrEmpty(featureName))
                return Task.CompletedTask;

            if (StartedFeatures.TryGetValue(featureName, out var featureTracker))
            {
                featureTracker.ProcessEvent(outputAddedEvent);
            }

            return Task.CompletedTask;
        }
        #endregion
    }
}
