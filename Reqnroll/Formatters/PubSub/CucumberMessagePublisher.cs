using Cucumber.Messages;
using Gherkin.CucumberMessages;
using Io.Cucumber.Messages.Types;
using Reqnroll.Bindings;
using Reqnroll.Events;
using Reqnroll.Formatters.ExecutionTracking;
using Reqnroll.Formatters.PayloadProcessing.Cucumber;
using Reqnroll.Formatters.RuntimeSupport;
using Reqnroll.Plugins;
using Reqnroll.Time;
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
public class CucumberMessagePublisher : IAsyncExecutionEventListener, ICucumberMessagePublisher
{
    private readonly IFormatterLog _logger;
    private readonly IMetaMessageGenerator _metaMessageGenerator;
    private readonly IIdGenerator _sharedIdGenerator;
    private readonly string _testRunStartedId;
    private readonly ICucumberMessageBroker _broker;
    private readonly IClock _clock;

    public ICucumberMessageFactory MessageFactory { get; internal set; }
    public IBindingMessagesGenerator BindingMessagesGenerator { get; internal set; }

    // Started Features by name
    public ConcurrentDictionary<FeatureInfo, Lazy<Task<IFeatureExecutionTracker>>> StartedFeatures { get; internal set; } = new();

    // This tracks the set of BeforeTestRun and AfterTestRun hooks that were called during the test run
    public ConcurrentDictionary<IBinding, TestRunHookExecutionTracker> TestRunHookTrackers { get; } = new();

    // Holds all Messages that are pending publication (collected from Feature Trackers as each Feature completes)
    public List<Envelope> Messages { get; internal set; } = new();

    public bool Enabled { get; internal set; } = false;

    // This field will be updated as each Feature completes. If all complete successfully, this will remain as true.
    // This field ultimately used to set the TestRunFinished message's status.
    public bool AllFeaturesPassed { get; private set; } = true;

    // StartupCompleted is set to true the first time the Publisher handles the TestRunStartedEvent. It is used as a guard against abnormal behavior from the test runner.
    public bool StartupCompleted { get; internal set; } = false;

    // This dictionary tracks the StepDefinitions(ID) by their method signature
    // used during TestCase creation to map from a Step Definition binding to its ID
    // shared to each Feature tracker so that we keep a single list
    private IReadOnlyDictionary<IBinding, string> StepDefinitionIdByMethodBinding => BindingMessagesGenerator.StepDefinitionIdByBinding;

    public CucumberMessagePublisher(ICucumberMessageBroker broker,
                                    IBindingMessagesGenerator bindingMessagesGenerator,
                                    IFormatterLog logger,
                                    IIdGenerator idGenerator,
                                    ICucumberMessageFactory messageFactory,
                                    IClock clock,
                                    IMetaMessageGenerator metaMessageGenerator
        )
    {
        _logger = logger;
        _logger.WriteMessage("DEBUG: Formatters: Publisher in constructor.");
        _sharedIdGenerator = idGenerator;
        _broker = broker;
        _clock = clock;
        BindingMessagesGenerator = bindingMessagesGenerator;
        MessageFactory = messageFactory;
        _testRunStartedId = _sharedIdGenerator.GetNewId();
        _metaMessageGenerator = metaMessageGenerator;
    }

    public void Initialize(RuntimePluginEvents runtimePluginEvents)
    {
        _logger.WriteMessage("DEBUG: Publisher in Initialize()");
        _broker.Initialize();

        runtimePluginEvents.CustomizeTestThreadDependencies += (_, args) =>
        {
            var testThreadExecutionEventPublisher = args.ObjectContainer.Resolve<ITestThreadExecutionEventPublisher>();
            testThreadExecutionEventPublisher.AddListener(this);
        };
    }

    public async Task OnEventAsync(IExecutionEvent executionEvent)
    {
        var hookType = executionEvent switch
        {
            HookStartedEvent se => se.HookType.ToString(),
            HookFinishedEvent fe => fe.HookType.ToString(),
            _ => ""
        };
        hookType = hookType.IsNotNullOrEmpty() ? "." + hookType : String.Empty;
        _logger.WriteMessage($"DEBUG: Publisher dispatching {executionEvent.GetType().Name}{hookType}");
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

    internal async Task PublisherStartup(TestRunStartedEvent testRunStartEvent)
    {
        _logger.WriteMessage($"DEBUG: Formatters.Publisher.PublisherStartup invoked. Enabled: {Enabled}; StartupCompleted: {StartupCompleted}");
        if (StartupCompleted)
            return;

        Enabled = _broker.IsEnabled && BindingMessagesGenerator.Ready ? true : Enabled;
        _logger.WriteMessage($"DEBUG: Formatters.Publisher.PublisherStartup: Broker: {_broker.IsEnabled}; bindingCache: {BindingMessagesGenerator.Ready}");
        if (!Enabled)
        {
            StartupCompleted = true;
            return;
        }

        try
        {
            await _broker.PublishAsync(Envelope.Create(MessageFactory.ToTestRunStarted(_clock.GetNowDateAndTime(), _testRunStartedId)));
            await _broker.PublishAsync(Envelope.Create(_metaMessageGenerator.GenerateMetaMessage()));
            _logger.WriteMessage($"DEBUG: Formatters.Publisher.PublisherStartup - published TestRunStarted");

            foreach (var msg in BindingMessagesGenerator.StaticBindingMessages)
            {
                // this publishes StepDefinition, Hook, StepArgumentTransform messages
                await _broker.PublishAsync(msg);
                _logger.WriteMessage($"DEBUG: Formatters.Publisher.PublisherStartup - published {msg.Content().GetType().Name}");
            }
        }
        catch (System.Exception ex)
        {
            _logger.WriteMessage($"Error publishing messages: {ex.Message}");
        }
        _logger.WriteMessage($"DEBUG: Formatters.Publisher.PublisherStartup - startup completed");

        StartupCompleted = true;
    }

    private DateTime RetrieveDateTime(Envelope e)
    {
        return Converters.ToDateTime(e.Timestamp());
    }

    internal async Task PublisherTestRunCompleteAsync(TestRunFinishedEvent testRunFinishedEvent)
    {
        _logger.WriteMessage($"DEBUG: Formatter:Publisher.TestRunComplete invoked. Enabled: {Enabled}; StartupCompleted: {StartupCompleted}");
        if (!Enabled)
            return;

        foreach (var featureTracker in StartedFeatures.Values)
        {
            var tracker = await featureTracker.Value;
            Messages.AddRange(tracker.RuntimeGeneratedMessages);

            if (!tracker.FeatureExecutionSuccess)
                AllFeaturesPassed = false;
        }
        // publish all TestCase messages
        var testCaseMessages = Messages.Where(e => e.Content() is TestCase).ToList();
        // sort the remaining Messages by timestamp
        var executionMessages = Messages.Except(testCaseMessages).OrderBy(RetrieveDateTime).ToList();
        _logger.WriteMessage($"DEBUG: Formatter:Publisher.TestRunComplete: has {testCaseMessages.Count} test case messages & {executionMessages.Count} execution messages.");
        // publish them in order to the broker
        foreach (var env in testCaseMessages)
        {
            await _broker.PublishAsync(env);
        }
        _logger.WriteMessage($"DEBUG: Formatter:Publisher.TestRunComplete: testCase messages written.");

        foreach (var env in executionMessages)
        {
            await _broker.PublishAsync(env);
        }
        _logger.WriteMessage($"DEBUG: Formatter:Publisher.TestRunComplete: exec messages written.");

        await _broker.PublishAsync(Envelope.Create(MessageFactory.ToTestRunFinished(AllFeaturesPassed, _clock.GetNowDateAndTime(), _testRunStartedId)));
        _logger.WriteMessage($"DEBUG: Formatter:Publisher.TestRunComplete: TestRunFinished Message written");
    }

    #region TestThreadExecutionEventPublisher Event Handling Methods

    // The following methods handle the events published by the TestThreadExecutionEventPublisher
    // When one of these calls the Broker, that method is async; otherwise these are sync methods that return a completed Task (to allow them to be called async from the TestThreadExecutionEventPublisher)
    private async Task FeatureStartedEventHandler(FeatureStartedEvent featureStartedEvent)
    {
        var featureInfo = featureStartedEvent.FeatureContext?.FeatureInfo;
        _logger.WriteMessage($"DEBUG: Formatters.Publisher.FeatureStartedEvent invoked {featureInfo?.Title} with startup completed status: {StartupCompleted}");

        if (!Enabled || featureInfo == null)
            return;

        // If this is the first time this feature is started, create and initialize a new FeatureExecutionTracker for it.
        // If the feature has already started, retrieve the existing tracker to ensure consistent tracking.
        // The use of Lazy<Task<IFeatureExecutionTracker>> ensures that concurrent attempts to start the same feature will wait for the first initialization to complete,
        // preventing duplicate trackers and race conditions.
        var trackerTask = StartedFeatures.GetOrAdd(featureInfo, _ =>
            new Lazy<Task<IFeatureExecutionTracker>>(() =>
                Task.Run<IFeatureExecutionTracker>(async () =>
                {
                    var featureExecutionTracker = new FeatureExecutionTracker(featureStartedEvent, _testRunStartedId, _sharedIdGenerator, StepDefinitionIdByMethodBinding, MessageFactory);
                    if (featureExecutionTracker.Enabled)
                    {
                        try
                        {
                            foreach (var msg in featureExecutionTracker.StaticMessages)
                            {
                                await _broker.PublishAsync(msg);
                            }
                        }
                        catch (System.Exception ex)
                        {
                            _logger.WriteMessage($"Error publishing messages: {ex.Message}");
                        }
                    }
                    return featureExecutionTracker;
                })));

        await trackerTask.Value;
    }

    internal async Task FeatureFinishedEventHandler(FeatureFinishedEvent featureFinishedEvent)
    {
        // For this and subsequent events, we pull up the FeatureExecutionTracker by featureInfo.
        var featureInfo = featureFinishedEvent.FeatureContext?.FeatureInfo;
        if (!Enabled || featureInfo == null)
        {
            return;
        }

        if (StartedFeatures.TryGetValue(featureInfo, out var featureTrackerTask))
        {
            var featureTracker = await featureTrackerTask.Value;
            if (!featureTracker.Enabled)
            {
                return;
            }

            // The following will compute the overall execution status of the feature.
            // Message generation is deferred until the TestRunFinishedEvent is published (see PublisherTestRunCompleteAsync).
            // This allows for multiple features to be processed in parallel; we can't tell which of these FeatureFinishedEvents is the final one.

            featureTracker.ProcessEvent(featureFinishedEvent);
        }
        // throw an exception if any of the TestCaseExecutionTrackers are not done?
    }

    private async Task ScenarioStartedEventHandler(ScenarioStartedEvent scenarioStartedEvent)
    {
        var featureTracker = await GetFeatureTracker(scenarioStartedEvent);
        featureTracker?.ProcessEvent(scenarioStartedEvent);
    }

    private async Task<IFeatureExecutionTracker> GetFeatureTracker<T>(T eventData) where T : IExecutionEvent
    {
        var featureInfo = GetFeatureId(eventData);

        if (!Enabled || featureInfo == null)
            return null;

        if (StartedFeatures.TryGetValue(featureInfo, out var featureTrackerTask))
        {
            var featureTracker = await featureTrackerTask.Value;
            if (!featureTracker.Enabled)
                return null;
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

    private async Task ScenarioFinishedEventHandler(ScenarioFinishedEvent scenarioFinishedEvent)
    {
        var featureTracker = await GetFeatureTracker(scenarioFinishedEvent);
        featureTracker?.ProcessEvent(scenarioFinishedEvent);
    }

    private async Task StepStartedEventHandler(StepStartedEvent stepStartedEvent)
    {
        var featureTracker = await GetFeatureTracker(stepStartedEvent);
        featureTracker?.ProcessEvent(stepStartedEvent);
    }

    private async Task StepFinishedEventHandler(StepFinishedEvent stepFinishedEvent)
    {
        var featureTracker = await GetFeatureTracker(stepFinishedEvent);
        featureTracker?.ProcessEvent(stepFinishedEvent);
    }

    private async Task HookBindingStartedEventHandler(HookBindingStartedEvent hookBindingStartedEvent)
    {
        if (!Enabled)
            return;

        switch (hookBindingStartedEvent.HookBinding.HookType)
        {
            case Bindings.HookType.BeforeTestRun:
            case Bindings.HookType.AfterTestRun:
            case Bindings.HookType.BeforeFeature:
            case Bindings.HookType.AfterFeature:
                var hookRunStartedId = _sharedIdGenerator.GetNewId();
                var hookId = StepDefinitionIdByMethodBinding[hookBindingStartedEvent.HookBinding];
                var hookTracker = new TestRunHookExecutionTracker(hookRunStartedId, hookId, _testRunStartedId, MessageFactory);
                TestRunHookTrackers.TryAdd(hookBindingStartedEvent.HookBinding, hookTracker);

                hookTracker.ProcessEvent(hookBindingStartedEvent);
                Messages.AddRange(((IGenerateMessage)hookTracker).GenerateFrom(hookBindingStartedEvent));
                return;

            default:
                var featureTracker = await GetFeatureTracker(hookBindingStartedEvent);
                featureTracker?.ProcessEvent(hookBindingStartedEvent);
                break;
        }
    }

    private async Task HookBindingFinishedEventHandler(HookBindingFinishedEvent hookBindingFinishedEvent)
    {
        if (!Enabled)
            return;

        switch (hookBindingFinishedEvent.HookBinding.HookType)
        {
            case Bindings.HookType.BeforeTestRun:
            case Bindings.HookType.AfterTestRun:
            case Bindings.HookType.BeforeFeature:
            case Bindings.HookType.AfterFeature:
                if (!TestRunHookTrackers.TryGetValue(hookBindingFinishedEvent.HookBinding, out var hookTracker)) // should not happen
                    return;
                hookTracker.ProcessEvent(hookBindingFinishedEvent);

                Messages.AddRange(((IGenerateMessage)hookTracker).GenerateFrom(hookBindingFinishedEvent));
                return;

            default:
                var featureTracker = await GetFeatureTracker(hookBindingFinishedEvent);
                featureTracker?.ProcessEvent(hookBindingFinishedEvent);
                break;
        }
    }

    private TestRunHookExecutionTracker ActiveTestRunHook
    {
        get
        {
            if (TestRunHookTrackers.Count == 0)
                return null;
            // Return the first active hook tracker
            return TestRunHookTrackers.Values.FirstOrDefault(hook => hook.IsActive);
        }
    }

    private async Task AttachmentAddedEventHandler(AttachmentAddedEvent attachmentAddedEvent)
    {
        if (!Enabled)
            return;

        var featureInfo = attachmentAddedEvent.FeatureInfo;
        if (featureInfo == null || string.IsNullOrEmpty(attachmentAddedEvent.ScenarioInfo?.Title))
        {
            // This is a TestRun-level attachment (not tied to any feature or scenario)
            var attachmentIssuedByHookId = ActiveTestRunHook?.HookStartedId ?? "";
            if (string.IsNullOrEmpty(attachmentIssuedByHookId))
            {
                _logger.WriteMessage("Error: AttachmentAddedEvent without a FeatureInfo or ScenarioInfo should be issued by a TestRun Hook. Attachment will be ignored by formatters.");
            }
            else
            {
                var attachmentTracker = new AttachmentTracker(_testRunStartedId, null, null, attachmentIssuedByHookId, MessageFactory);
                attachmentTracker.ProcessEvent(attachmentAddedEvent);

                Messages.Add(Envelope.Create(MessageFactory.ToAttachment(attachmentTracker)));
            }
            return;
        }
        if (StartedFeatures.TryGetValue(featureInfo, out var featureTrackerTask))
        {
            var tracker = await featureTrackerTask.Value;
            tracker?.ProcessEvent(attachmentAddedEvent);
        }
    }

    private async Task OutputAddedEventHandler(OutputAddedEvent outputAddedEvent)
    {
        if (!Enabled)
            return;

        var featureInfo = outputAddedEvent.FeatureInfo;
        if (featureInfo == null || string.IsNullOrEmpty(outputAddedEvent.ScenarioInfo?.Title))
        {
            // This is a TestRun-level attachment (not tied to any feature) or is an output coming from a Before/AfterFeature hook
            var outputIssuedByHookId = ActiveTestRunHook?.HookStartedId ?? "";
            if (string.IsNullOrEmpty(outputIssuedByHookId))
            {
                _logger.WriteMessage("OutputAddedEvent without a FeatureInfo or ScenarioInfo should be issued by a TestRun Hook. OutputEvent will be ignored by formatters.");
            }
            else
            {
                var outputMessageTracker = new OutputMessageTracker(_testRunStartedId, null, null, outputIssuedByHookId, MessageFactory);
                outputMessageTracker.ProcessEvent(outputAddedEvent);

                Messages.Add(Envelope.Create(MessageFactory.ToAttachment(outputMessageTracker)));
            }
            return;
        }

        if (StartedFeatures.TryGetValue(featureInfo, out var featureTrackerTask))
        {
            var tracker = await featureTrackerTask.Value;
            tracker?.ProcessEvent(outputAddedEvent);
        }
    }
    #endregion
}