using Gherkin.CucumberMessages;
using Io.Cucumber.Messages.Types;
using Reqnroll.Formatters.RuntimeSupport;
using Reqnroll.Events;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using Reqnroll.Bindings;
using Reqnroll.Formatters.PubSub;
using System.Threading.Tasks;

namespace Reqnroll.Formatters.ExecutionTracking;

/// <summary>
/// FeatureExecutionTracker is responsible for tracking the execution of a Feature.
/// There will be one instance of this class per gherkin Feature.
/// </summary>
public class FeatureExecutionTracker : IFeatureExecutionTracker
{
    private readonly IPickleExecutionTrackerFactory _pickleExecutionTrackerFactory;
    private readonly IMessagePublisher _publisher;

    // ID Generator to use when generating IDs for TestCase messages and beyond
    public IIdGenerator IdGenerator { get; }
    public string TestRunStartedId { get; }

    public PickleJar PickleJar { get; protected set; }

    // This dictionary tracks the StepDefinitions(ID) by their method signature
    // used during TestCase creation to map from a Step Definition binding to its ID
    // This dictionary is shared across all Features (via the Publisher)
    public IReadOnlyDictionary<IBinding, string> StepDefinitionsByBinding { get; }

    // This maintains the list of PickleExecutionTrackers, identified by (string) PickleID, running within this feature
    private readonly ConcurrentDictionary<string, IPickleExecutionTracker> _pickleExecutionTrackersById = new();

    // This dictionary maps from (string) PickleIdIndex to (string) PickleID
    public Dictionary<string, string> PickleIds { get; } = new();

    public string FeatureName { get; }
    public bool Enabled { get; }
    public bool FeatureExecutionSuccess { get; private set; }

    public IEnumerable<IPickleExecutionTracker> PickleExecutionTrackers => _pickleExecutionTrackersById.Values;

    // This constructor is used by the Publisher when it sees a Feature (by name) for the first time
    public FeatureExecutionTracker(FeatureStartedEvent featureStartedEvent, string testRunStartedId, IReadOnlyDictionary<IBinding, string> stepDefinitionIdsByMethod, IIdGenerator idGenerator, IPickleExecutionTrackerFactory pickleExecutionTrackerFactory, IMessagePublisher publisher)
    {
        TestRunStartedId = testRunStartedId;
        StepDefinitionsByBinding = stepDefinitionIdsByMethod;
        IdGenerator = idGenerator;
        FeatureName = featureStartedEvent.FeatureContext.FeatureInfo.Title;
        Enabled = featureStartedEvent.FeatureContext.FeatureInfo.FeatureCucumberMessages?.HasMessages ?? false;
        _pickleExecutionTrackerFactory = pickleExecutionTrackerFactory;
        _publisher = publisher;
    }

    // This method is used to generate the static messages (Source, GherkinDocument & Pickles) and the StepTransformations, StepDefinitions and Hook messages which are global to the entire Solution
    // This should be called only once per Feature. 
    public async Task ProcessEvent(FeatureStartedEvent featureStartedEvent)
    {
        if (!Enabled)
        {
            // If the feature is not enabled, we do not generate any static messages
            return;
        }

        var source = featureStartedEvent.FeatureContext.FeatureInfo.FeatureCucumberMessages.Source;
        if (source != null) await _publisher.PublishAsync(Envelope.Create(source));

        var gherkinDocument = featureStartedEvent.FeatureContext.FeatureInfo.FeatureCucumberMessages.GherkinDocument;
        if (gherkinDocument != null) await _publisher.PublishAsync(Envelope.Create(gherkinDocument));

        var featurePickles = featureStartedEvent.FeatureContext.FeatureInfo.FeatureCucumberMessages.Pickles?.ToList();
        if (featurePickles != null)
        {
            for (int i = 0; i < featurePickles.Count; i++)
            {
                var pickle = featurePickles[i];
                PickleIds.Add(i.ToString(), pickle.Id);
                await _publisher.PublishAsync(Envelope.Create(pickle));
            }
            PickleJar = new PickleJar(featurePickles);
        }
    }

    // This method is called by the CucumberMessagePublisher when the TestRun is completed
    // Since we can't rely on the FeatureFinished event as an indicator of Feature completion (due to parallel execution),
    // this artificial event is used to signal that all Features have been processed and the trackers can be finalized (and publish any messages that are pending).
    public async Task FinalizeTracking()
    {
        var pickleExecutionTrackers = PickleExecutionTrackers.ToList();
        foreach (var tracker in pickleExecutionTrackers)
        {
            await tracker.FinalizeTracking();
        }
        
        // Calculate the feature-level execution status
        FeatureExecutionSuccess = pickleExecutionTrackers.All(
            tc => tc.Finished 
            && (tc.ScenarioExecutionStatus == ScenarioExecutionStatus.OK || tc.ScenarioExecutionStatus == ScenarioExecutionStatus.Skipped));
    }


    internal IPickleExecutionTracker GetOrAddPickleExecutionTracker(string pickleId)
    {
        return _pickleExecutionTrackersById.GetOrAdd(pickleId, id => _pickleExecutionTrackerFactory.CreatePickleTracker(this, id));
    }

    public async Task ProcessEvent(ScenarioStartedEvent scenarioStartedEvent)
    {
        var pickleIndex = scenarioStartedEvent.ScenarioContext?.ScenarioInfo?.PickleIdIndex;

        if (string.IsNullOrEmpty(pickleIndex))
        {
            throw new ArgumentNullException(nameof(scenarioStartedEvent), "ScenarioContext.ScenarioInfo.PickleIdIndex is not initialized");
        }

        if (PickleIds.TryGetValue(pickleIndex, out var pickleId))
        {
            if (PickleJar == null)
                throw new InvalidOperationException("PickleJar is not properly initialized for Cucumber Messages.");

            // Fetch the PickleStepSequence for this Pickle and give to the ScenarioInfo
            var pickleStepSequence = PickleJar.PickleStepSequenceFor(pickleIndex);
            scenarioStartedEvent.ScenarioContext.ScenarioInfo.PickleStepSequence = pickleStepSequence;

            // Get: This represents a re-execution of the TestCase
            // Add: This is the first time this TestCase (aka, pickle) is getting executed. New up a PickleExecutionTracker for it.
            var testCaseTracker = GetOrAddPickleExecutionTracker(pickleId);
            await testCaseTracker.ProcessEvent(scenarioStartedEvent);
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

    public async Task ProcessEvent(ScenarioFinishedEvent scenarioFinishedEvent)
    {
        if (TryGetTestCaseTracker(scenarioFinishedEvent.ScenarioContext, out var testCaseTracker))
        {
            await testCaseTracker.ProcessEvent(scenarioFinishedEvent);
        }
    }

    public async Task ProcessEvent(StepStartedEvent stepStartedEvent)
    {
        if (TryGetTestCaseTracker(stepStartedEvent.ScenarioContext, out var testCaseTracker))
        {
            await testCaseTracker.ProcessEvent(stepStartedEvent);
        }
    }

    public async Task ProcessEvent(StepFinishedEvent stepFinishedEvent)
    {
        if (TryGetTestCaseTracker(stepFinishedEvent.ScenarioContext, out var testCaseTracker))
        {
            await testCaseTracker.ProcessEvent(stepFinishedEvent);
        }
    }

    public async Task ProcessEvent(HookBindingStartedEvent hookBindingStartedEvent)
    {
        if (TryGetTestCaseTracker(hookBindingStartedEvent.ContextManager?.ScenarioContext, out var testCaseTracker))
        {
            await testCaseTracker.ProcessEvent(hookBindingStartedEvent);
        }
    }

    public async Task ProcessEvent(HookBindingFinishedEvent hookBindingFinishedEvent)
    {
        if (TryGetTestCaseTracker(hookBindingFinishedEvent.ContextManager?.ScenarioContext, out var testCaseTracker))
        {
            await testCaseTracker.ProcessEvent(hookBindingFinishedEvent);
        }
    }

    public async Task ProcessEvent(AttachmentAddedEvent attachmentAddedEvent)
    {
        if (TryGetTestCaseTracker(attachmentAddedEvent.ScenarioInfo, out var testCaseTracker))
        {
            await testCaseTracker.ProcessEvent(attachmentAddedEvent);
        }
    }

    public async Task ProcessEvent(OutputAddedEvent outputAddedEvent)
    {
        if (TryGetTestCaseTracker(outputAddedEvent.ScenarioInfo, out var testCaseTracker))
        {
            await testCaseTracker.ProcessEvent(outputAddedEvent);
        }
    }
}