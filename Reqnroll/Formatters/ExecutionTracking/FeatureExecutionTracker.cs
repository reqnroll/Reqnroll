using Gherkin.CucumberMessages;
using Io.Cucumber.Messages.Types;
using Reqnroll.Formatters.PayloadProcessing.Cucumber;
using Reqnroll.Formatters.RuntimeSupport;
using Reqnroll.Events;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using Reqnroll.Time;
using Reqnroll.Bindings.Reflection;
using Reqnroll.Bindings;

namespace Reqnroll.Formatters.ExecutionTracking;

/// <summary>
/// FeatureExecutionTracker is responsible for tracking the execution of a Feature.
/// There will be one instance of this class per gherkin Feature.
/// </summary>
public class FeatureExecutionTracker : IFeatureExecutionTracker
{
    // Static Messages are those generated during code generation (Source, GherkinDocument & Pickles)
    // and the StepTransformations, StepDefinitions and Hook messages which are global to the entire Solution.
    private readonly Lazy<IEnumerable<Envelope>> _staticMessagesFactory;
    public IEnumerable<Envelope> StaticMessages => _staticMessagesFactory.Value;

    private PickleJar _pickleJar;

    // ID Generator to use when generating IDs for TestCase messages and beyond
    public IIdGenerator IdGenerator { get; }
    public string TestRunStartedId { get; }

    // This dictionary tracks the StepDefinitions(ID) by their method signature
    // used during TestCase creation to map from a Step Definition binding to its ID
    // This dictionary is shared across all Features (via the Publisher)
    public IReadOnlyDictionary<IBinding, string> StepDefinitionsByBinding { get; }

    // This maintains the list of PickleExecutionTrackers, identified by (string) PickleID, running within this feature
    private readonly ConcurrentDictionary<string, IPickleExecutionTracker> _pickleExecutionTrackersById = new();
    public Func<FeatureExecutionTracker, string, IPickleExecutionTracker> PickleExecutionTrackerFactory { get; internal set; }

    // This dictionary maps from (string) PickleIdIndex to (string) PickleID
    public Dictionary<string, string> PickleIds { get; } = new();

    public string FeatureName { get; }
    public bool Enabled { get; }
    public bool FeatureExecutionSuccess { get; private set; }

    public IEnumerable<IPickleExecutionTracker> PickleExecutionTrackers => _pickleExecutionTrackersById.Values;

    // This constructor is used by the Publisher when it sees a Feature (by name) for the first time
    public FeatureExecutionTracker(FeatureStartedEvent featureStartedEvent, string testRunStartedId, IIdGenerator idGenerator, IReadOnlyDictionary<IBinding, string> stepDefinitionIdsByMethod, ICucumberMessageFactory messageFactory)
    {
        TestRunStartedId = testRunStartedId;
        StepDefinitionsByBinding = stepDefinitionIdsByMethod;
        IdGenerator = idGenerator;
        FeatureName = featureStartedEvent.FeatureContext.FeatureInfo.Title;
        var featureHasCucumberMessages = featureStartedEvent.FeatureContext.FeatureInfo.FeatureCucumberMessages != null && featureStartedEvent.FeatureContext.FeatureInfo.FeatureCucumberMessages.Pickles != null; ;
        featureHasCucumberMessages = featureHasCucumberMessages && featureStartedEvent.FeatureContext.FeatureInfo.FeatureCucumberMessages.Pickles() != null;
        Enabled = featureHasCucumberMessages;
        _staticMessagesFactory = new Lazy<IEnumerable<Envelope>>(() => GenerateStaticMessages(featureStartedEvent));
        _ = _staticMessagesFactory.Value;

        var clock = featureStartedEvent.FeatureContext.FeatureContainer.Resolve<IClock>(); //TODO: better retrieval of IClock
        PickleExecutionTrackerFactory = (ft, pickleId) =>
            new PickleExecutionTracker(pickleId, ft.TestRunStartedId, ft.FeatureName, ft.Enabled, ft.IdGenerator, ft.StepDefinitionsByBinding, clock.GetNowDateAndTime(), messageFactory);
    }

    // At the completion of Feature execution, this is called to generate all non-static Messages
    // Iterating through all Scenario trackers, generating all messages.
    public IEnumerable<Envelope> RuntimeGeneratedMessages
    {
        get
        {
            return PickleExecutionTrackers
                   .OrderBy(st => st.TestCaseStartedTimeStamp)
                   .SelectMany(st => st.RuntimeGeneratedMessages);
        }
    }

    // This method is used to generate the static messages (Source, GherkinDocument & Pickles) and the StepTransformations, StepDefinitions and Hook messages which are global to the entire Solution
    // This should be called only once per Feature. 
    private IEnumerable<Envelope> GenerateStaticMessages(FeatureStartedEvent featureStartedEvent)
    {

        var source = featureStartedEvent.FeatureContext.FeatureInfo.FeatureCucumberMessages.Source();
        if (source != null) yield return Envelope.Create(source);

        var gherkinDocument = featureStartedEvent.FeatureContext.FeatureInfo.FeatureCucumberMessages.GherkinDocument();
        if (gherkinDocument != null) yield return Envelope.Create(gherkinDocument);

        var featurepickles = featureStartedEvent.FeatureContext.FeatureInfo.FeatureCucumberMessages.Pickles();
        if (featurepickles != null)
        {
            var pickles = featurepickles.ToList();
            for (int i = 0; i < pickles.Count; i++)
            {
                PickleIds.Add(i.ToString(), pickles[i].Id);
            }

            _pickleJar = new PickleJar(pickles);

            foreach (var pickle in pickles)
            {
                yield return Envelope.Create(pickle);
            }
        }
    }

    // When the FeatureFinished event fires, we calculate the feature-level execution status
    // If scenarios are running in parallel, this event will fire multiple times (once per each instance of the test class).
    // Running this method multiple times is harmless. The FeatureExecutionSuccess property is only consumed upon the TestRunComplete event (ie, only once).
    public void ProcessEvent(FeatureFinishedEvent featureFinishedEvent)
    {
        var pickleExecutionTrackers = PickleExecutionTrackers.ToList();

        // Calculate the feature-level execution status
        FeatureExecutionSuccess = pickleExecutionTrackers.All(tc => tc.Finished && tc.ScenarioExecutionStatus == ScenarioExecutionStatus.OK);
    }

    internal IPickleExecutionTracker GetOrAddPickleExecutionTracker(string pickleId)
    {
        return _pickleExecutionTrackersById.GetOrAdd(pickleId, id => PickleExecutionTrackerFactory(this, id));
    }

    public void ProcessEvent(ScenarioStartedEvent scenarioStartedEvent)
    {
        var pickleIndex = scenarioStartedEvent.ScenarioContext?.ScenarioInfo?.PickleIdIndex;

        if (string.IsNullOrEmpty(pickleIndex))
        {
            throw new ArgumentNullException(nameof(scenarioStartedEvent), "ScenarioContext.ScenarioInfo.PickleIdIndex is not initialized");
        }

        if (PickleIds.TryGetValue(pickleIndex, out var pickleId))
        {
            if (_pickleJar == null)
                throw new InvalidOperationException("PickleJar is not properly initialized for Cucumber Messages.");

            // Fetch the PickleStepSequence for this Pickle and give to the ScenarioInfo
            var pickleStepSequence = _pickleJar.PickleStepSequenceFor(pickleIndex);
            scenarioStartedEvent.ScenarioContext.ScenarioInfo.PickleStepSequence = pickleStepSequence; ;

            // Get: This represents a re-execution of the TestCase
            // Add: This is the first time this TestCase (aka, pickle) is getting executed. New up a PickleExecutionTracker for it.
            var testCaseTracker = GetOrAddPickleExecutionTracker(pickleId);
            testCaseTracker.ProcessEvent(scenarioStartedEvent);
        }
    }

    private bool TryGetTestCaseTracker(IScenarioContext scenarioContext, out IPickleExecutionTracker pickleExecutionTracker)
    {
        return TryGetTestCaseTracker(scenarioContext?.ScenarioInfo, out pickleExecutionTracker);
    }

    private bool TryGetTestCaseTracker(ScenarioInfo scenarioInfo, out IPickleExecutionTracker pickleExecutionTracker)
    {
        pickleExecutionTracker = null;
        var pickleIndex = scenarioInfo?.PickleIdIndex;

        if (!string.IsNullOrEmpty(pickleIndex) &&
            PickleIds.TryGetValue(pickleIndex, out var pickleId) &&
            _pickleExecutionTrackersById.TryGetValue(pickleId, out pickleExecutionTracker))
        {
            return true;
        }

        return false;
    }

    public void ProcessEvent(ScenarioFinishedEvent scenarioFinishedEvent)
    {
        if (TryGetTestCaseTracker(scenarioFinishedEvent.ScenarioContext, out var testCaseTracker))
        {
            testCaseTracker.ProcessEvent(scenarioFinishedEvent);
        }
    }

    public void ProcessEvent(StepStartedEvent stepStartedEvent)
    {
        if (TryGetTestCaseTracker(stepStartedEvent.ScenarioContext, out var testCaseTracker))
        {
            testCaseTracker.ProcessEvent(stepStartedEvent);
        }
    }

    public void ProcessEvent(StepFinishedEvent stepFinishedEvent)
    {
        if (TryGetTestCaseTracker(stepFinishedEvent.ScenarioContext, out var testCaseTracker))
        {
            testCaseTracker.ProcessEvent(stepFinishedEvent);
        }
    }

    public void ProcessEvent(HookBindingStartedEvent hookBindingStartedEvent)
    {
        if (TryGetTestCaseTracker(hookBindingStartedEvent.ContextManager?.ScenarioContext, out var testCaseTracker))
        {
            testCaseTracker.ProcessEvent(hookBindingStartedEvent);
        }
    }

    public void ProcessEvent(HookBindingFinishedEvent hookBindingFinishedEvent)
    {
        if (TryGetTestCaseTracker(hookBindingFinishedEvent.ContextManager?.ScenarioContext, out var testCaseTracker))
        {
            testCaseTracker.ProcessEvent(hookBindingFinishedEvent);
        }
    }

    public void ProcessEvent(AttachmentAddedEvent attachmentAddedEvent)
    {
        if (TryGetTestCaseTracker(attachmentAddedEvent.ScenarioInfo, out var testCaseTracker))
        {
            testCaseTracker.ProcessEvent(attachmentAddedEvent);
        }
    }

    public void ProcessEvent(OutputAddedEvent outputAddedEvent)
    {
        if (TryGetTestCaseTracker(outputAddedEvent.ScenarioInfo, out var testCaseTracker))
        {
            testCaseTracker.ProcessEvent(outputAddedEvent);
        }
    }
}