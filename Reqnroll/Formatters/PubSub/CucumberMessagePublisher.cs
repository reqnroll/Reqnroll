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
using Reqnroll.UnitTestProvider;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Reqnroll.Formatters.RuntimeSupport;

namespace Reqnroll.Formatters.PubSub;

/// <summary>
/// This class is responsible for publishing Cucumber Messages to the <see cref="CucumberMessageBroker"/>.
/// It uses the set of <see cref="IExecutionEvent"/> to track overall execution of features and steps and drive generation of messages.
/// It uses the <see cref="IRuntimePlugin"/> interface to force the runtime to load it during startup (although it is not an external plugin per se).
/// </summary>
public class CucumberMessagePublisher : IRuntimePlugin, IAsyncExecutionEventListener
{
    internal ICucumberMessageBroker _broker;
    internal IObjectContainer _globalObjectContainer;

    // Started Features by name
    internal ConcurrentDictionary<FeatureInfo, IFeatureExecutionTracker> _startedFeatures = new();
    internal BindingMessagesGenerator _bindingCaches;
    internal IClock _clock;
    internal IFormatterLog _logger;

    // This dictionary tracks the StepDefinitions(ID) by their method signature
    // used during TestCase creation to map from a Step Definition binding to its ID
    // shared to each Feature tracker so that we keep a single list
    internal ConcurrentDictionary<IBinding, string> StepDefinitionIdByMethodBinding => _bindingCaches.StepDefinitionIdByBinding;

    public IIdGenerator SharedIdGenerator { get; private set; }

    private string _testRunStartedId;
    internal bool _enabled = false;

    // This tracks the set of BeforeTestRun and AfterTestRun hooks that were called during the test run
    internal readonly ConcurrentDictionary<IBinding, TestRunHookExecutionTracker> _testRunHookTrackers = new();
    // This tracks all Attachments and Output Events; used during publication to sequence them in the correct order.
    internal readonly OutputEventsTracker OutputEventsTracker = new();

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
        runtimePluginEvents.RegisterGlobalDependencies += (_, args) =>
        {
            _globalObjectContainer = args.ObjectContainer;
            args.ObjectContainer.RegisterTypeAs<GuidIdGenerator, IIdGenerator>();
            if (!args.ObjectContainer.IsRegistered<ICucumberMessageFactory>())
            {
                args.ObjectContainer.RegisterFactoryAs<ICucumberMessageFactory>((Delegate)(() => new CucumberMessageFactory()));
            }
        };

        runtimePluginEvents.CustomizeTestThreadDependencies += (_, args) =>
        {
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
                await PublisherTestRunCompleteAsync(testRunFinishedEvent);
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

    internal async Task PublisherStartup(IExecutionEvent executionEvent)
    {

        if (!_enabled || _startupCompleted)
            return;

        SharedIdGenerator = _globalObjectContainer.Resolve<IIdGenerator>();
        _messageFactory = _globalObjectContainer.Resolve<ICucumberMessageFactory>();
        _testRunStartedId = SharedIdGenerator.GetNewId();
        _bindingCaches = new BindingMessagesGenerator(SharedIdGenerator, _messageFactory);
        _clock = _globalObjectContainer.Resolve<IClock>();
        _logger = _globalObjectContainer.Resolve<IFormatterLog>();
        try
        {
            await _broker.PublishAsync(Envelope.Create(_messageFactory.ToTestRunStarted(_clock.GetNowDateAndTime(), _testRunStartedId)));
            await _broker.PublishAsync(Envelope.Create(_messageFactory.ToMeta(_globalObjectContainer)));
            foreach (var msg in _bindingCaches.PopulateBindingCachesAndGenerateBindingMessages(_globalObjectContainer.Resolve<IBindingRegistry>()))
            {
                // this publishes StepDefinition, Hook, StepArgumentTransform messages
                await _broker.PublishAsync(msg);
            }
        }
        catch (System.Exception ex)
        {
            _logger.WriteMessage($"Error publishing messages: {ex.Message}");
        }

        _startupCompleted = true;
    }

    private DateTime RetrieveDateTime(Envelope e)
    {
        return e.Content() switch
        {
            Attachment attachment => OutputEventsTracker.GetTimestampForMatchingAttachment(attachment).ToUniversalTime(),
            _ => Converters.ToDateTime(e.Timestamp())
        };
    }

    internal async Task PublisherTestRunCompleteAsync(TestRunFinishedEvent testRunFinishedEvent)
    {
        if (!_enabled)
            return;
        var status = _startedFeatures.Values.All(ft => ft.FeatureExecutionSuccess);
        // publish all TestCase messages
        var testCaseMessages = _messages.Where(e => e.Content() is TestCase).ToList();
        // sort the remaining Messages by timestamp
        var executionMessages = _messages.Except(testCaseMessages).OrderBy(RetrieveDateTime).ToList();

        // publish them in order to the broker
        foreach (var env in testCaseMessages)
        {
            await _broker.PublishAsync(env);
        }

        foreach (var env in executionMessages)
        {
            await _broker.PublishAsync(env);
        }

        await _broker.PublishAsync(Envelope.Create(_messageFactory.ToTestRunFinished(status, _clock.GetNowDateAndTime(), _testRunStartedId)));

        _startedFeatures.Clear();
    }

    #region TestThreadExecutionEventPublisher Event Handling Methods

    // The following methods handle the events published by the TestThreadExecutionEventPublisher
    // When one of these calls the Broker, that method is async; otherwise these are sync methods that return a completed Task (to allow them to be called async from the TestThreadExecutionEventPublisher)
    private async Task FeatureStartedEventHandler(FeatureStartedEvent featureStartedEvent)
    {
        var featureInfo = featureStartedEvent.FeatureContext?.FeatureInfo;

        if (!_enabled || featureInfo == null)
        {
            return;
        }

        // The following should be thread safe when multiple instances of the Test Class are running in parallel. 
        // If _startedFeatures.ContainsKey returns true, then we know another instance of this Feature class has already started. We don't need a second instance of the 
        // FeatureExecutionTracker, and we don't want multiple copies of the static messages to be published.
        if (_startedFeatures.ContainsKey(featureInfo))
        {
            // Already started, don't repeat the following steps
            return;
        }

        var featureExecutionTracker = new FeatureExecutionTracker(featureStartedEvent, _testRunStartedId, SharedIdGenerator, StepDefinitionIdByMethodBinding, _messageFactory);
        if (_startedFeatures.TryAdd(featureInfo, featureExecutionTracker) && featureExecutionTracker.Enabled)
        {
            try
            {
                foreach (var msg in featureExecutionTracker.StaticMessages) // Static Messages == known at compile-time (Source, Gherkin Document, and Pickle messages)
                {
                    await _broker.PublishAsync(msg);
                }
            }
            catch (System.Exception ex)
            {
                _logger.WriteMessage($"Error publishing messages: {ex.Message}");
            }
        }
    }

    private Task FeatureFinishedEventHandler(FeatureFinishedEvent featureFinishedEvent)
    {
        // For this and subsequent events, we pull up the FeatureExecutionTracker by featureInfo.
        var featureInfo = featureFinishedEvent.FeatureContext?.FeatureInfo;
        if (!_enabled || featureInfo == null)
        {
            return Task.CompletedTask;
        }
        if (!_startedFeatures.ContainsKey(featureInfo) || !_startedFeatures[featureInfo].Enabled)
        {
            return Task.CompletedTask;
        }
        var featureTracker = _startedFeatures[featureInfo];
        featureTracker.ProcessEvent(featureFinishedEvent);
        foreach (var msg in featureTracker.RuntimeGeneratedMessages)
        {
            _messages.Add(msg);
        }

        return Task.CompletedTask;
        // throw an exception if any of the TestCaseExecutionTrackers are not done?
    }

    private Task ScenarioStartedEventHandler(ScenarioStartedEvent scenarioStartedEvent)
    {
        var featureTracker = GetFeatureTracker(scenarioStartedEvent);
        featureTracker?.ProcessEvent(scenarioStartedEvent);

        return Task.CompletedTask;
    }

    private IFeatureExecutionTracker GetFeatureTracker<T>(T eventData) where T : IExecutionEvent
    {
        var featureInfo = GetFeatureId(eventData);

        if (!_enabled || featureInfo == null)
            return null;

        if (_startedFeatures.TryGetValue(featureInfo, out var featureTracker) && featureTracker.Enabled)
        {
            return featureTracker;
        }

        return null;
    }

    // Helper method to extract feature name from different event types
    private FeatureInfo GetFeatureId(IExecutionEvent eventData)
    {
        return eventData switch
        {
            ScenarioStartedEvent sse => sse.FeatureContext?.FeatureInfo,
            ScenarioFinishedEvent sfe => sfe.FeatureContext?.FeatureInfo,
            StepStartedEvent stse => stse.FeatureContext?.FeatureInfo,
            StepFinishedEvent stfe => stfe.FeatureContext?.FeatureInfo,
            HookBindingStartedEvent hbse => hbse.ContextManager.FeatureContext.FeatureInfo,
            HookBindingFinishedEvent hbfe => hbfe.ContextManager.FeatureContext.FeatureInfo,
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
                //var signature = _messageFactory.CanonicalizeHookBinding(hookBindingStartedEvent.HookBinding);
                var hookId = StepDefinitionIdByMethodBinding[hookBindingStartedEvent.HookBinding];
                var hookTracker = new TestRunHookExecutionTracker(hookRunStartedId, hookId, _testRunStartedId, _messageFactory);
                _testRunHookTrackers.TryAdd(hookBindingStartedEvent.HookBinding, hookTracker);

                hookTracker.ProcessEvent(hookBindingStartedEvent);
                _messages.AddRange(((IGenerateMessage)hookTracker).GenerateFrom(hookBindingStartedEvent));
                return Task.CompletedTask;

            default:
                var featureTracker = GetFeatureTracker(hookBindingStartedEvent);
                featureTracker?.ProcessEvent(hookBindingStartedEvent);
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
                //var signature = _messageFactory.CanonicalizeHookBinding(hookBindingFinishedEvent.HookBinding);
                if (!_testRunHookTrackers.TryGetValue(hookBindingFinishedEvent.HookBinding, out var hookTracker)) // should not happen
                    return Task.CompletedTask;
                hookTracker.ProcessEvent(hookBindingFinishedEvent);

                _messages.AddRange(((IGenerateMessage)hookTracker).GenerateFrom(hookBindingFinishedEvent));
                return Task.CompletedTask;

            default:
                var featureTracker = GetFeatureTracker(hookBindingFinishedEvent);
                featureTracker?.ProcessEvent(hookBindingFinishedEvent);
                break;
        }

        return Task.CompletedTask;
    }

    private Task AttachmentAddedEventHandler(AttachmentAddedEvent attachmentAddedEvent)
    {
        if (!_enabled)
            return Task.CompletedTask;

        OutputEventsTracker.ProcessEvent(attachmentAddedEvent);

        var featureInfo = attachmentAddedEvent.FeatureInfo;
        if (featureInfo == null || string.IsNullOrEmpty(attachmentAddedEvent.ScenarioInfo?.Title))
        {
            // This is a TestRun-level attachment (not tied to any feature or scenario)
            var attachmentTracker = new AttachmentTracker(_testRunStartedId, null, null, _messageFactory);
            attachmentTracker.ProcessEvent(attachmentAddedEvent);

            _messages.Add(Envelope.Create(_messageFactory.ToAttachment(attachmentTracker)));
            return Task.CompletedTask;
        }
        if (_startedFeatures.TryGetValue(featureInfo, out var featureTracker))
        {
            featureTracker.ProcessEvent(attachmentAddedEvent);
        }

        return Task.CompletedTask;
    }

    private Task OutputAddedEventHandler(OutputAddedEvent outputAddedEvent)
    {
        if (!_enabled)
            return Task.CompletedTask;

        OutputEventsTracker.ProcessEvent(outputAddedEvent);

        var featureInfo = outputAddedEvent.FeatureInfo;
        if (featureInfo == null || string.IsNullOrEmpty(outputAddedEvent.ScenarioInfo?.Title))
        {
            // This is a TestRun-level attachment (not tied to any feature) or is an output coming from a Before/AfterFeature hook
            var outputMessageTracker = new OutputMessageTracker(_testRunStartedId, null, null, _messageFactory);
            outputMessageTracker.ProcessEvent(outputAddedEvent);

            _messages.Add(Envelope.Create(_messageFactory.ToAttachment(outputMessageTracker)));
            return Task.CompletedTask;
        }

        if (_startedFeatures.TryGetValue(featureInfo, out var featureTracker))
        {
            featureTracker.ProcessEvent(outputAddedEvent);
        }

        return Task.CompletedTask;
    }
    #endregion
}