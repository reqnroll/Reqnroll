using Cucumber.Messages;
using Gherkin.CucumberMessages;
using Io.Cucumber.Messages.Types;
using Reqnroll.Bindings;
using Reqnroll.BoDi;
using Reqnroll.Formatters.ExecutionTracking;
using Reqnroll.Formatters.PayloadProcessing.Cucumber;
using Reqnroll.Events;
using Reqnroll.Plugins;
using Reqnroll.Time;
using Reqnroll.Tracing;
using Reqnroll.UnitTestProvider;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Reqnroll.Formatters.PubSub;

/// <summary>
/// This class is responsible for publishing Cucumber Messages to the <see cref="CucumberMessageBroker"/>.
/// It uses the set of <see cref="IExecutionEvent"/> to track overall execution of features and steps and drive generation of messages.
/// It uses the <see cref="IRuntimePlugin"/> interface to force the runtime to load it during startup (although it is not an external plugin per se).
/// </summary>
public class CucumberMessagePublisher : IRuntimePlugin, IAsyncExecutionEventListener
{
    internal ICucumberMessageBroker _broker;
    internal IObjectContainer _testThreadObjectContainer;

    // Started Features by name
    internal ConcurrentDictionary<string, IFeatureTracker> _startedFeatures = new();
    internal BindingMessagesGenerator _bindingCaches;

    // This dictionary tracks the StepDefintions(ID) by their method signature
    // used during TestCase creation to map from a Step Definition binding to its ID
    // shared to each Feature tracker so that we keep a single list
    internal ConcurrentDictionary<string, string> StepDefinitionsByMethodSignature => _bindingCaches.StepDefinitionIdByMethodSignaturePatternCache;

    public IIdGenerator SharedIdGenerator { get; private set; }

    private string _testRunStartedId;
    internal bool _enabled = false;

    // This tracks the set of BeforeTestRun and AfterTestRun hooks that were called during the test run
    internal readonly ConcurrentDictionary<string, TestRunHookTracker> _testRunHookTrackers = new();
    // This tracks all Attachments and Output Events; used during publication to sequence them in the correct order.
    internal readonly AttachmentTracker _attachmentTracker = new();

    // Holds all Messages that are pending publication (collected from Feature Trackers as each Feature completes)
    internal List<Envelope> _messages = new();


    internal ICucumberMessageFactory _messageFactory;
    private bool _startupCompleted = false;

    public CucumberMessagePublisher(ICucumberMessageBroker broker)
    {
        _broker = broker;
        _broker.BrokerReadyEvent += BrokerReady;
    }

    public void Initialize(RuntimePluginEvents runtimePluginEvents, RuntimePluginParameters runtimePluginParameters, UnitTestProviderConfiguration unitTestProviderConfiguration)
    {
        runtimePluginEvents.RegisterGlobalDependencies += (sender, args) =>
        {
            args.ObjectContainer.RegisterTypeAs<GuidIdGenerator, IIdGenerator>();
            if (!args.ObjectContainer.IsRegistered<ICucumberMessageFactory>())
            {
                args.ObjectContainer.RegisterFactoryAs<ICucumberMessageFactory>(() => new CucumberMessageFactory());
            }
        };

        runtimePluginEvents.CustomizeTestThreadDependencies += (sender, args) =>
        {
            _testThreadObjectContainer = args.ObjectContainer;
            var testThreadExecutionEventPublisher = args.ObjectContainer.Resolve<ITestThreadExecutionEventPublisher>();
            testThreadExecutionEventPublisher.AddListener(this);
        };
    }

    public async Task OnEventAsync(IExecutionEvent executionEvent)
    {
        switch (executionEvent)
        {
            case TestRunStartedEvent testRunStartedEvent:
                await PublisherStartup(testRunStartedEvent);
                break;
            case TestRunFinishedEvent testRunFinishedEvent:
                await PublisherTestRunCompleteAsync(null, new RuntimePluginAfterTestRunEventArgs(_testThreadObjectContainer));
                break;
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
        }
    }

    internal void BrokerReady(object sender, BrokerReadyEventArgs args)
    {
        _enabled = _broker.Enabled;
    }

    internal async Task PublisherStartup(IExecutionEvent executionEvent) { 

        if (!_enabled || _startupCompleted)
            return;

        SharedIdGenerator = _testThreadObjectContainer.Resolve<IIdGenerator>();
        _messageFactory = _testThreadObjectContainer.Resolve<ICucumberMessageFactory>();
        _testRunStartedId = SharedIdGenerator.GetNewId();
        _bindingCaches = new BindingMessagesGenerator(SharedIdGenerator, _messageFactory);
        var clock = _testThreadObjectContainer.Resolve<IClock>();
        var traceListener = _testThreadObjectContainer.Resolve<ITraceListener>();
        try
        {
            await _broker.PublishAsync(Envelope.Create(_messageFactory.ToTestRunStarted(clock.GetNowDateAndTime(), _testRunStartedId)));
            await _broker.PublishAsync(Envelope.Create(_messageFactory.ToMeta(_testThreadObjectContainer)));
            foreach (var msg in _bindingCaches.PopulateBindingCachesAndGenerateBindingMessages(_testThreadObjectContainer.Resolve<IBindingRegistry>()))
            {
                // this publishes StepDefinition, Hook, StepArgumentTransform messages
                await _broker.PublishAsync(msg);
            }
        }
        catch (System.Exception ex)
        {
            traceListener.WriteToolOutput($"Error publishing messages: {ex.Message}");
        }

        _startupCompleted = true;
    }

    private DateTime RetrieveDateTime(Envelope e)
    {
        return e.Content() switch
        {
            Attachment attachment => _attachmentTracker.FindMatchingAttachment(attachment).Timestamp.ToUniversalTime(),
            _ => Converters.ToDateTime(e.Timestamp())
        };
    }

    internal async Task PublisherTestRunCompleteAsync(object sender, RuntimePluginAfterTestRunEventArgs args)
    {
        if (!_enabled)
            return;
        var status = _startedFeatures.Values.All(f => f.FeatureExecutionSuccess);
        // publish all TestCase messages
        var testCaseMessages = _messages.Where(e => e.Content() is TestCase).ToList();
        // sort the remaining Messages by timestamp
        var executionMessages = _messages.Except(testCaseMessages).OrderBy(e => RetrieveDateTime(e)).ToList();

        var clock = args.ObjectContainer.Resolve<IClock>();
        // publish them in order to the broker
        foreach (var env in testCaseMessages)
        {
            await _broker.PublishAsync(env);
        }

        foreach (var env in executionMessages)
        {
            await _broker.PublishAsync(env);
        }

        await _broker.PublishAsync(Envelope.Create(_messageFactory.ToTestRunFinished(status, clock.GetNowDateAndTime(), _testRunStartedId)));

        _startedFeatures.Clear();
    }

    #region TestThreadExecutionEventPublisher Event Handling Methods

    // The following methods handle the events published by the TestThreadExecutionEventPublisher
    // When one of these calls the Broker, that method is async; otherwise these are sync methods that return a completed Task (to allow them to be called async from the TestThreadExecutionEventPublisher)
    private async Task FeatureStartedEventHandler(FeatureStartedEvent featureStartedEvent)
    {
        var traceListener = _testThreadObjectContainer.Resolve<ITraceListener>();

        var featureName = featureStartedEvent.FeatureContext?.FeatureInfo?.Title;

        if (!_enabled || string.IsNullOrEmpty(featureName))
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

        var ft = new FeatureTracker(featureStartedEvent, _testRunStartedId, SharedIdGenerator, StepDefinitionsByMethodSignature, _messageFactory);
        if (_startedFeatures.TryAdd(featureName, ft) && ft.Enabled)
        {
            try
            {
                foreach (var msg in ft.StaticMessages) // Static Messages == known at compile-time (Source, Gerkhin Document, and Pickle messages)
                {
                    await _broker.PublishAsync(msg);
                }
            }
            catch (System.Exception ex)
            {
                traceListener.WriteToolOutput($"Error publishing messages: {ex.Message}");
            }
        }
    }

    private Task FeatureFinishedEventHandler(FeatureFinishedEvent featureFinishedEvent)
    {
        // For this and subsequent events, we pull up the FeatureTracker by feature name.
        // If the feature name is not available (such as might be the case in certain test setups), we ignore the event.
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
        var featureTracker = GetFeatureTracker(scenarioStartedEvent);
        featureTracker?.ProcessEvent(scenarioStartedEvent);

        return Task.CompletedTask;
    }

    private IFeatureTracker GetFeatureTracker<T>(T eventData) where T : IExecutionEvent
    {
        var featureName = GetFeatureName(eventData);

        if (!_enabled || string.IsNullOrEmpty(featureName))
            return null;

        if (_startedFeatures.TryGetValue(featureName, out var featureTracker) && featureTracker.Enabled)
        {
            return featureTracker;
        }

        return null;
    }

    // Helper method to extract feature name from different event types
    private string GetFeatureName(IExecutionEvent eventData)
    {
        return eventData switch
        {
            ScenarioStartedEvent sse => sse.FeatureContext?.FeatureInfo?.Title,
            ScenarioFinishedEvent sfe => sfe.FeatureContext?.FeatureInfo?.Title,
            StepStartedEvent stse => stse.FeatureContext?.FeatureInfo?.Title,
            StepFinishedEvent stfe => stfe.FeatureContext?.FeatureInfo?.Title,
            _ => null
        };
    }

    private Task ScenarioFinishedEventHandler(ScenarioFinishedEvent scenarioFinishedEvent)
    {
        var featureTracker = GetFeatureTracker(scenarioFinishedEvent);
        featureTracker?.ProcessEvent(scenarioFinishedEvent);

        return Task.CompletedTask;
    }

    private Task StepStartedEventHandler(StepStartedEvent stepStartedEvent)
    {
        var featureTracker = GetFeatureTracker(stepStartedEvent);
        featureTracker?.ProcessEvent(stepStartedEvent);

        return Task.CompletedTask;
    }

    private Task StepFinishedEventHandler(StepFinishedEvent stepFinishedEvent)
    {
        var featureTracker = GetFeatureTracker(stepFinishedEvent);
        featureTracker?.ProcessEvent(stepFinishedEvent);

        return Task.CompletedTask;
    }

    private Task HookBindingStartedEventHandler(HookBindingStartedEvent hookBindingStartedEvent)
    {
        if (!_enabled)
            return Task.CompletedTask;

        switch (hookBindingStartedEvent.HookBinding.HookType)
        {
            case Bindings.HookType.BeforeTestRun:
            case Bindings.HookType.AfterTestRun:
            case Bindings.HookType.BeforeFeature:
            case Bindings.HookType.AfterFeature:
                var hookRunStartedId = SharedIdGenerator.GetNewId();
                var signature = _messageFactory.CanonicalizeHookBinding(hookBindingStartedEvent.HookBinding);
                var hookId = StepDefinitionsByMethodSignature[signature];
                var hookTracker = new TestRunHookTracker(hookRunStartedId, hookId, hookBindingStartedEvent.Timestamp, _testRunStartedId, _messageFactory);
                _testRunHookTrackers.TryAdd(signature, hookTracker);
                _messages.AddRange(hookTracker.GenerateFrom(hookBindingStartedEvent));
                return Task.CompletedTask;

            default:
                var featureName = hookBindingStartedEvent.ContextManager?.FeatureContext?.FeatureInfo?.Title;
                if (!_enabled || String.IsNullOrEmpty(featureName))
                    return Task.CompletedTask;

                if (_startedFeatures.TryGetValue(featureName, out var featureTracker))
                {
                    featureTracker.ProcessEvent(hookBindingStartedEvent);
                }
                break;
        }

        return Task.CompletedTask;
    }

    private Task HookBindingFinishedEventHandler(HookBindingFinishedEvent hookBindingFinishedEvent)
    {
        if (!_enabled)
            return Task.CompletedTask;

        switch (hookBindingFinishedEvent.HookBinding.HookType)
        {
            case Bindings.HookType.BeforeTestRun:
            case Bindings.HookType.AfterTestRun:
            case Bindings.HookType.BeforeFeature:
            case Bindings.HookType.AfterFeature:
                var signature = _messageFactory.CanonicalizeHookBinding(hookBindingFinishedEvent.HookBinding);
                if (!_testRunHookTrackers.TryGetValue(signature, out var hookTracker)) // should not happen
                    return Task.CompletedTask;
                hookTracker.Duration = hookBindingFinishedEvent.Duration;
                hookTracker.Exception = hookBindingFinishedEvent.HookException;
                hookTracker.TimeStamp = hookBindingFinishedEvent.Timestamp;

                _messages.AddRange(hookTracker.GenerateFrom(hookBindingFinishedEvent));
                return Task.CompletedTask;

            default:
                var featureName = hookBindingFinishedEvent.ContextManager?.FeatureContext?.FeatureInfo?.Title;
                if (!_enabled || String.IsNullOrEmpty(featureName))
                    return Task.CompletedTask;

                if (_startedFeatures.TryGetValue(featureName, out var featureTracker))
                {
                    featureTracker.ProcessEvent(hookBindingFinishedEvent);
                }
                break;
        }

        return Task.CompletedTask;
    }

    private Task AttachmentAddedEventHandler(AttachmentAddedEvent attachmentAddedEvent)
    {
        if (!_enabled)
            return Task.CompletedTask;

        _attachmentTracker.RecordAttachment(attachmentAddedEvent);

        var featureName = attachmentAddedEvent.FeatureInfo?.Title;
        if (string.IsNullOrEmpty(featureName) || string.IsNullOrEmpty(attachmentAddedEvent.ScenarioInfo?.Title))
        {
            // This is a TestRun-level attachment (not tied to any feature)
            _messages.Add(Envelope.Create(_messageFactory.ToAttachment(new AttachmentAddedEventWrapper(attachmentAddedEvent, _testRunStartedId, null, null, _messageFactory))));
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
        if (string.IsNullOrEmpty(featureName) || string.IsNullOrEmpty(outputAddedEvent.ScenarioInfo?.Title))
        {
            // This is a TestRun-level attachment (not tied to any feature) or is an Output coming from a Before/AfterFeature hook
            _messages.Add(Envelope.Create(_messageFactory.ToAttachment(new OutputAddedEventWrapper(outputAddedEvent, _testRunStartedId, null, null, _messageFactory))));
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