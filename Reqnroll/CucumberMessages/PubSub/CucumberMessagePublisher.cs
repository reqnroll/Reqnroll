using Reqnroll.BoDi;
using Reqnroll.Events;
using Reqnroll.Tracing;
using Reqnroll.Plugins;
using Reqnroll.UnitTestProvider;
using System.Collections.Concurrent;
using System;
using System.Linq;
using Reqnroll.CucumberMessages.ExecutionTracking;
using Reqnroll.CucumberMessages.PayloadProcessing.Cucumber;
using Io.Cucumber.Messages.Types;
using Reqnroll.CucumberMessages.RuntimeSupport;
using Reqnroll.CucumberMessages.Configuration;
using Gherkin.CucumberMessages;
using Reqnroll.Bindings;
using System.Threading.Tasks;
using System.Text.RegularExpressions;
using System.Collections.Generic;
using Cucumber.Messages;

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
    public class CucumberMessagePublisher : IRuntimePlugin, IAsyncExecutionEventListener
    {
        private Lazy<ICucumberMessageBroker> _brokerFactory;
        private ICucumberMessageBroker _broker;
        private IObjectContainer _testThreadObjectContainer;

        public static object _lock = new object();

        // Started Features by name
        private ConcurrentDictionary<string, FeatureTracker> _startedFeatures = new();

        // This dictionary tracks the StepDefintions(ID) by their method signature
        // used during TestCase creation to map from a Step Definition binding to its ID
        // shared to each Feature tracker so that we keep a single list
        internal ConcurrentDictionary<string, string> StepDefinitionsByPattern { get; } = new();
        private ConcurrentBag<IStepArgumentTransformationBinding> _stepArgumentTransforms = new();
        private ConcurrentBag<IStepDefinitionBinding> _undefinedParameterTypeBindings = new();
        public IIdGenerator SharedIDGenerator { get; private set; }

        private string _testRunStartedId;
        bool _enabled = false;

        // This tracks the set of BeforeTestRun and AfterTestRun hooks that were called during the test run
        private readonly ConcurrentDictionary<string, TestRunHookTracker> _testRunHookTrackers = new();
        // This tracks all Attachments and Output Events; used during publication to sequence them in the correct order.
        private readonly AttachmentTracker _attachmentTracker = new();

        // Holds all Messages that are pending publication (collected from Feature Trackers as each Feature completes)
        private List<Envelope> _messages = new();

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
                  _testThreadObjectContainer = args.ObjectContainer;
                  _brokerFactory = new Lazy<ICucumberMessageBroker>(() => _testThreadObjectContainer.Resolve<ICucumberMessageBroker>());
                  var testThreadExecutionEventPublisher = args.ObjectContainer.Resolve<ITestThreadExecutionEventPublisher>();
                  testThreadExecutionEventPublisher.AddAsyncListener(this);
              };
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

            _enabled = _broker.Enabled;

            if (!_enabled)
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
                    // this publishes StepDefinition, Hook, StepArgumentTransform messages
                    await _broker.PublishAsync(new ReqnrollCucumberMessage() { CucumberMessageSource = "startup", Envelope = msg });
                }
            }).Wait();
        }
        private IEnumerable<Envelope> PopulateBindingCachesAndGenerateBindingMessages(IObjectContainer objectContainer)
        {
            var bindingRegistry = objectContainer.Resolve<IBindingRegistry>();

            foreach (var stepTransform in bindingRegistry.GetStepTransformations())
            {
                if (_stepArgumentTransforms.Contains(stepTransform))
                    continue;
                _stepArgumentTransforms.Add(stepTransform);
                var parameterType = CucumberMessageFactory.ToParameterType(stepTransform, SharedIDGenerator);
                yield return Envelope.Create(parameterType);
            }

            foreach (var binding in bindingRegistry.GetStepDefinitions().Where(sd => !sd.IsValid))
            {
                var errmsg = binding.ErrorMessage;
                if (errmsg.Contains("Undefined parameter type"))
                {
                    var paramName = Regex.Match(errmsg, "Undefined parameter type '(.*)'").Groups[1].Value;
                    if (_undefinedParameterTypeBindings.Contains(binding))
                        continue;
                    _undefinedParameterTypeBindings.Add(binding);
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

        private DateTime RetrieveDateTime(Envelope e)
        {
            if (e.Attachment == null)
                return Converters.ToDateTime(e.Timestamp());
            // what remains is an Attachment.
            // match it up with an AttachmentAddedEvent in the AttachmentTracker
            // and use that event's timestamp as a proxy for the timestamp of the Attachment

            return _attachmentTracker.FindMatchingAttachment(e.Attachment).Timestamp;
        }

        private void PublisherTestRunComplete(object sender, RuntimePluginAfterTestRunEventArgs e)
        {
            if (!_enabled)
                return;
            var status = _startedFeatures.Values.All(f => f.FeatureExecutionSuccess);
            // publish all TestCase messages
            var testCaseMessages = _messages.Where(e => e.Content() is TestCase).OrderBy(e => e.Content().Id());
            // sort the remaining Messages by timestamp
            var executionMessages = _messages.Except(testCaseMessages).OrderBy(e => RetrieveDateTime(e)).ToList();

            // publish them in order to the broker
            Task.Run(async () =>
            {
                foreach (var env in testCaseMessages)
                {
                    await _broker.PublishAsync(new ReqnrollCucumberMessage() { CucumberMessageSource = "testCaseMessages", Envelope = env });
                }

                foreach (var env in executionMessages)
                {
                    await _broker.PublishAsync(new ReqnrollCucumberMessage() { CucumberMessageSource = "testExecutionMessages", Envelope = env });
                }

                await _broker.PublishAsync(new ReqnrollCucumberMessage() { CucumberMessageSource = "shutdown", Envelope = Envelope.Create(CucumberMessageFactory.ToTestRunFinished(status, DateTime.Now, _testRunStartedId)) });
            }).Wait();

            _startedFeatures.Clear();
        }

        #region TestThreadExecutionEventPublisher Event Handling Methods

        // The following methods handle the events published by the TestThreadExecutionEventPublisher
        // When one of these calls the Broker, that method is async; otherwise these are sync methods that return a completed Task (to allow them to be called async from the TestThreadExecutionEventPublisher)
        private async Task FeatureStartedEventHandler(FeatureStartedEvent featureStartedEvent)
        {
            _broker = _brokerFactory.Value;
            var traceListener = _testThreadObjectContainer.Resolve<ITraceListener>();

            var featureName = featureStartedEvent.FeatureContext?.FeatureInfo?.Title;

            _enabled = _broker.Enabled;
            if (!_enabled || String.IsNullOrEmpty(featureName))
            {
                return;
            }

            // The following should be thread safe when multiple instances of the Test Class are running in parallel. 
            // If _startedFeatures.ContainsKey returns true, then we know another instance of this Feature class has already started. We don't need a second instance of the 
            // FeatureTracker, and we don't want multiple copies of the static messages to be published.
            if (_startedFeatures.ContainsKey(featureName))
            {
                // Already started, don't repeat the following steps
                return;
            }

            await Task.Run(() =>
            {
                lock (_lock)
                {
                    // This will add a FeatureTracker to the _startedFeatures dictionary only once, and if it is enabled, it will publish the static messages shared by all steps.

                    // We publish the messages before adding the featureTracker to the _startedFeatures dictionary b/c other parallel scenario threads might be running. 
                    //   We don't want them to run until after the static messages have been published (and the PickleJar has been populated as a result).
                    if (!_startedFeatures.ContainsKey(featureName))
                    {
                        var ft = new FeatureTracker(featureStartedEvent, _testRunStartedId, SharedIDGenerator, StepDefinitionsByPattern);
                        if (ft.Enabled)
                            Task.Run(async () =>
                            {
                                foreach (var msg in ft.StaticMessages) // Static Messages == known at compile-time (Source, Gerkhin Document, and Pickle messages)
                                {
                                    await _broker.PublishAsync(new ReqnrollCucumberMessage() { CucumberMessageSource = featureName, Envelope = msg });
                                }
                            }).Wait();
                        _startedFeatures.TryAdd(featureName, ft);
                    }
                }
            });
        }

        private Task FeatureFinishedEventHandler(FeatureFinishedEvent featureFinishedEvent)
        {
            // For this and subsequent events, we pull up the FeatureTracker by feature name.
            // If the feature name is not avaiable (such as might be the case in certain test setups), we ignore the event.
            var featureName = featureFinishedEvent.FeatureContext?.FeatureInfo?.Title;
            if (!_enabled || String.IsNullOrEmpty(featureName))
            {
                return Task.CompletedTask;
            }
            if (!_startedFeatures.ContainsKey(featureName) || !_startedFeatures[featureName].Enabled)
            {
                return Task.CompletedTask;
            }
            var featureTracker = _startedFeatures[featureName];
            featureTracker.ProcessEvent(featureFinishedEvent);
            foreach (var msg in featureTracker.RuntimeGeneratedMessages)
            {
                _messages.Add(msg);
            }

            return Task.CompletedTask;
            // throw an exception if any of the TestCaseCucumberMessageTrackers are not done?

        }

        private Task ScenarioStartedEventHandler(ScenarioStartedEvent scenarioStartedEvent)
        {
            var featureName = scenarioStartedEvent.FeatureContext?.FeatureInfo?.Title;
            if (!_enabled || String.IsNullOrEmpty(featureName))
                return Task.CompletedTask;
            var traceListener = _testThreadObjectContainer.Resolve<ITraceListener>();
            if (_startedFeatures.TryGetValue(featureName, out var featureTracker))
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

        private Task ScenarioFinishedEventHandler(ScenarioFinishedEvent scenarioFinishedEvent)
        {
            var featureName = scenarioFinishedEvent.FeatureContext?.FeatureInfo?.Title;

            if (!_enabled || String.IsNullOrEmpty(featureName))
                return Task.CompletedTask;
            if (_startedFeatures.TryGetValue(featureName, out var featureTracker))
            {
                featureTracker.ProcessEvent(scenarioFinishedEvent);
            }
            return Task.CompletedTask;
        }

        private Task StepStartedEventHandler(StepStartedEvent stepStartedEvent)
        {
            var featureName = stepStartedEvent.FeatureContext?.FeatureInfo?.Title;

            if (!_enabled || String.IsNullOrEmpty(featureName))
                return Task.CompletedTask;

            if (_startedFeatures.TryGetValue(featureName, out var featureTracker))
            {
                featureTracker.ProcessEvent(stepStartedEvent);
            }

            return Task.CompletedTask;
        }

        private Task StepFinishedEventHandler(StepFinishedEvent stepFinishedEvent)
        {
            var featureName = stepFinishedEvent.FeatureContext?.FeatureInfo?.Title;
            if (!_enabled || String.IsNullOrEmpty(featureName))
                return Task.CompletedTask;

            if (_startedFeatures.TryGetValue(featureName, out var featureTracker))
            {
                featureTracker.ProcessEvent(stepFinishedEvent);
            }

            return Task.CompletedTask;
        }

        private Task HookBindingStartedEventHandler(HookBindingStartedEvent hookBindingStartedEvent)
        {
            if (!_enabled)
                return Task.CompletedTask;

            if (hookBindingStartedEvent.HookBinding.HookType == Bindings.HookType.BeforeTestRun || hookBindingStartedEvent.HookBinding.HookType == Bindings.HookType.AfterTestRun
                || hookBindingStartedEvent.HookBinding.HookType == Bindings.HookType.BeforeFeature || hookBindingStartedEvent.HookBinding.HookType == Bindings.HookType.AfterFeature)
            {
                string hookRunStartedId = SharedIDGenerator.GetNewId();
                var signature = CucumberMessageFactory.CanonicalizeHookBinding(hookBindingStartedEvent.HookBinding);
                var hookId = StepDefinitionsByPattern[signature];
                var hookTracker = new TestRunHookTracker(hookRunStartedId, hookId, hookBindingStartedEvent, _testRunStartedId);
                _testRunHookTrackers.TryAdd(signature, hookTracker);
                _messages.Add(Envelope.Create(CucumberMessageFactory.ToTestRunHookStarted(hookTracker)));
                return Task.CompletedTask;
            }

            var featureName = hookBindingStartedEvent.ContextManager?.FeatureContext?.FeatureInfo?.Title;
            if (!_enabled || String.IsNullOrEmpty(featureName))
                return Task.CompletedTask;

            if (_startedFeatures.TryGetValue(featureName, out var featureTracker))
            {
                featureTracker.ProcessEvent(hookBindingStartedEvent);
            }

            return Task.CompletedTask;
        }

        private Task HookBindingFinishedEventHandler(HookBindingFinishedEvent hookBindingFinishedEvent)
        {
            if (!_enabled)
                return Task.CompletedTask;

            if (hookBindingFinishedEvent.HookBinding.HookType == Bindings.HookType.BeforeTestRun || hookBindingFinishedEvent.HookBinding.HookType == Bindings.HookType.AfterTestRun
                || hookBindingFinishedEvent.HookBinding.HookType == Bindings.HookType.BeforeFeature || hookBindingFinishedEvent.HookBinding.HookType == Bindings.HookType.AfterFeature)
            {
                var signature = CucumberMessageFactory.CanonicalizeHookBinding(hookBindingFinishedEvent.HookBinding);
                if (!_testRunHookTrackers.TryGetValue(signature, out var hookTracker)) // should not happen
                    return Task.CompletedTask;
                hookTracker.Duration = hookBindingFinishedEvent.Duration;
                hookTracker.Exception = hookBindingFinishedEvent.HookException;
                hookTracker.TimeStamp = hookBindingFinishedEvent.Timestamp;

                _messages.Add(Envelope.Create(CucumberMessageFactory.ToTestRunHookFinished(hookTracker)));
                return Task.CompletedTask;
            }


            var featureName = hookBindingFinishedEvent.ContextManager?.FeatureContext?.FeatureInfo?.Title;
            if (!_enabled || String.IsNullOrEmpty(featureName))
                return Task.CompletedTask;

            if (_startedFeatures.TryGetValue(featureName, out var featureTracker))
            {
                featureTracker.ProcessEvent(hookBindingFinishedEvent);
            }

            return Task.CompletedTask;
        }

        private Task AttachmentAddedEventHandler(AttachmentAddedEvent attachmentAddedEvent)
        {
            if (!_enabled)
                return Task.CompletedTask;

            _attachmentTracker.RecordAttachment(attachmentAddedEvent);

            var featureName = attachmentAddedEvent.FeatureInfo?.Title;
            if (String.IsNullOrEmpty(featureName) || String.IsNullOrEmpty(attachmentAddedEvent.ScenarioInfo?.Title))
            {
                // This is a TestRun-level attachment (not tied to any feature)
                _messages.Add(Envelope.Create(CucumberMessageFactory.ToAttachment(new AttachmentAddedEventWrapper(attachmentAddedEvent, _testRunStartedId, null, null))));
                return Task.CompletedTask;
            }
            if (_startedFeatures.TryGetValue(featureName, out var featureTracker))
            {
                featureTracker.ProcessEvent(attachmentAddedEvent);
            }

            return Task.CompletedTask;
        }

        private Task OutputAddedEventHandler(OutputAddedEvent outputAddedEvent)
        {
            if (!_enabled)
                return Task.CompletedTask;

            _attachmentTracker.RecordOutput(outputAddedEvent);

            var featureName = outputAddedEvent.FeatureInfo?.Title;
            if (String.IsNullOrEmpty(featureName) || String.IsNullOrEmpty(outputAddedEvent.ScenarioInfo?.Title))
            {
                // This is a TestRun-level attachment (not tied to any feature) or is an Output coming from a Before/AfterFeature hook
                _messages.Add(Envelope.Create(CucumberMessageFactory.ToAttachment(new OutputAddedEventWrapper(outputAddedEvent, _testRunStartedId, null, null))));
                return Task.CompletedTask;
            }

            if (_startedFeatures.TryGetValue(featureName, out var featureTracker))
            {
                featureTracker.ProcessEvent(outputAddedEvent);
            }

            return Task.CompletedTask;
        }
        #endregion
    }
}
