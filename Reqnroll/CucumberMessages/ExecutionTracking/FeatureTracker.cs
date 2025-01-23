﻿using Gherkin.CucumberMessages;
using Io.Cucumber.Messages.Types;
using Reqnroll.Bindings;
using Reqnroll.CucumberMessages.PayloadProcessing.Cucumber;
using Reqnroll.CucumberMessages.RuntimeSupport;
using Reqnroll.Events;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text.RegularExpressions;
using System.Threading.Tasks;

namespace Reqnroll.CucumberMessages.ExecutionTracking
{
    /// <summary>
    /// FeatureTracker is responsible for tracking the execution of a Feature.
    /// There will be one instance of this class per gherkin Feature.
    /// </summary>
    public class FeatureTracker
    {
        // Static Messages are those generated during code generation (Source, GherkinDocument & Pickles)
        // and the StepTransformations, StepDefinitions and Hook messages which are global to the entire Solution.
        internal IEnumerable<Envelope> StaticMessages => _staticMessages.Value;
        private Lazy<IEnumerable<Envelope>> _staticMessages;

        // ID Generator to use when generating IDs for TestCase messages and beyond
        // If gherkin feature was generated using integer IDs, then we will use an integer ID generator seeded with the last known integer ID
        // otherwise we'll use a GUID ID generator. We can't know ahead of time which type of ID generator to use, therefore this is not set by the constructor.
        public IIdGenerator IDGenerator { get; set; }
        public string TestRunStartedId { get; }

        // This dictionary tracks the StepDefintions(ID) by their method signature
        // used during TestCase creation to map from a Step Definition binding to its ID
        // This dictionary is shared across all Features (via the Publisher)
        // The same is true of the StepTransformations and StepDefinitionBindings used for Undefined Parameter Types
        internal ConcurrentDictionary<string, string> StepDefinitionsByPattern = new();
        private ConcurrentBag<IStepArgumentTransformationBinding> StepTransformRegistry;
        private ConcurrentBag<IStepDefinitionBinding> UndefinedParameterTypes;

        // This dictionary maps from (string) PickkleID to the TestCase tracker
        private ConcurrentDictionary<string, TestCaseTracker> testCaseTrackersById = new();

        public string FeatureName { get; set; }
        public bool Enabled { get; private set; }

        // This dictionary maps from (string) PickleIDIndex to (string) PickleID
        public Dictionary<string, string> PickleIds { get; } = new();
        public PickleJar PickleJar { get; set; }

        public bool FeatureExecutionSuccess { get; private set; }

        // This constructor is used by the Publisher when it sees a Feature (by name) for the first time
        public FeatureTracker(FeatureStartedEvent featureStartedEvent, string testRunStartedId, IIdGenerator idGenerator, ConcurrentDictionary<string, string> stepDefinitionPatterns, ConcurrentBag<IStepArgumentTransformationBinding> stepTransformRegistry, ConcurrentBag<IStepDefinitionBinding> undefinedParameterTypes)
        {
            TestRunStartedId = testRunStartedId;
            StepDefinitionsByPattern = stepDefinitionPatterns;
            StepTransformRegistry = stepTransformRegistry;
            UndefinedParameterTypes = undefinedParameterTypes;
            IDGenerator = idGenerator;
            FeatureName = featureStartedEvent.FeatureContext.FeatureInfo.Title;
            var featureHasCucumberMessages = featureStartedEvent.FeatureContext.FeatureInfo.FeatureCucumberMessages != null;
            Enabled = featureHasCucumberMessages && featureStartedEvent.FeatureContext.FeatureInfo.FeatureCucumberMessages.Pickles != null ? true : false;
            ProcessEvent(featureStartedEvent);
        }
        internal void ProcessEvent(FeatureStartedEvent featureStartedEvent)
        {
            if (!Enabled) return;
            // This has side-effects needed for proper execution of subsequent events; eg, the Ids of the static messages get generated and then subsequent events generate Ids that follow
            _staticMessages = new Lazy<IEnumerable<Envelope>>(() => GenerateStaticMessages(featureStartedEvent));
        }

        // This method is used to generate the static messages (Source, GherkinDocument & Pickles) and the StepTransformations, StepDefinitions and Hook messages which are global to the entire Solution
        // This should be called only once per Feature. As such, it relies on the use of a lock section within the Publisher to ensure that only a single instance of the FeatureTracker is created per Feature
        private IEnumerable<Envelope> GenerateStaticMessages(FeatureStartedEvent featureStartedEvent)
        {
            var gd = featureStartedEvent.FeatureContext.FeatureInfo.FeatureCucumberMessages.GherkinDocument();

            var pickles = featureStartedEvent.FeatureContext.FeatureInfo.FeatureCucumberMessages.Pickles().ToList();

            var idReWriter = new CucumberMessages.RuntimeSupport.IdReWriter();
            idReWriter.ReWriteIds(gd, pickles, IDGenerator, out var reWrittenGherkinDocument, out var reWrittenPickles);
            gd = reWrittenGherkinDocument;
            pickles = reWrittenPickles.ToList();

            for (int i = 0; i < pickles.Count; i++)
            {
                PickleIds.Add(i.ToString(), pickles[i].Id);
            }

            PickleJar = new PickleJar(pickles);

            yield return Envelope.Create(featureStartedEvent.FeatureContext.FeatureInfo.FeatureCucumberMessages.Source());

            yield return Envelope.Create(gd);
            foreach (var pickle in pickles)
            {
                yield return Envelope.Create(pickle);
            }

        }

        // This method is used to identify the last ID generated from the set generated during code gen. 
        // It takes advantage of the design assumption that Pickles are generated last, and that PickleSteps are generated before the ID of the Pickle itself.
        // Therefore, the ID of the last Pickle is last ID generated.
        // Subsequent Messages can be assigned IDs starting from that one (assuming incrementing integer IDs).
        // 
        // Note: Should the method of assigning IDs ever change (or their sequence of assignment) in the code generator, then this method may need to change as well.
        private string ExtractLastID(List<Pickle> pickles)
        {
            return pickles.Last().Id;
        }

        // When the FeatureFinished event fires, we calculate the Feature-level Execution Status
        // If Scenarios are running in parallel, this event will fire multiple times (once per each instance of the test class).
        // Running this method multiple times is harmless. The FeatureExecutionSuccess property is only consumed upon the TestRunComplete event (ie, only once).
        public void ProcessEvent(FeatureFinishedEvent featureFinishedEvent)
        {
            var testCases = testCaseTrackersById.Values.ToList();

            // Calculate the Feature-level Execution Status
            FeatureExecutionSuccess = testCases.All(tc => tc.Finished) switch
            {
                true => testCases.All(tc => tc.ScenarioExecutionStatus == ScenarioExecutionStatus.OK),
                _ => false
            };
        }

        public void ProcessEvent(ScenarioStartedEvent scenarioStartedEvent)
        {
            var pickleIndex = scenarioStartedEvent.ScenarioContext?.ScenarioInfo?.PickleIdIndex;

            // The following validations and ANE throws are in place to help identify threading bugs when Scenarios are run in parallel.
            // TODO: consider removing these or placing them within #IFDEBUG

            if (String.IsNullOrEmpty(pickleIndex))
            {
                // Should never happen
                if (scenarioStartedEvent.ScenarioContext == null)
                    throw new ArgumentNullException("ScenarioContext", "ScenarioContext is not properly initialized for Cucumber Messages.");
                if (scenarioStartedEvent.ScenarioContext.ScenarioInfo == null)
                    throw new ArgumentNullException("ScenarioInfo", "ScenarioContext/ScenarioInfo is not properly initialized for Cucumber Messages.");
                throw new ArgumentNullException("PickleIdIndex", "ScenarioContext/ScenarioInfo does not have a properly initialized PickleIdIndex.");
            }

            if (PickleIds.TryGetValue(pickleIndex, out var pickleId))
            {
                if (PickleJar == null)
                    throw new ArgumentNullException("PickleJar", "PickleJar is not properly initialized for Cucumber Messages.");
                // Fetch the PickleStepSequence for this Pickle and give to the ScenarioInfo
                var pickleStepSequence = PickleJar.PickleStepSequenceFor(pickleIndex);
                scenarioStartedEvent.ScenarioContext.ScenarioInfo.PickleStepSequence = pickleStepSequence; ;

                TestCaseTracker tccmt;
                if(testCaseTrackersById.TryGetValue(pickleId, out tccmt))
                {
                    // This represents a re-execution of the TestCase.
                    tccmt.ProcessEvent(scenarioStartedEvent);
                }
                else // This is the first time this TestCase (aka, pickle) is getting executed. New up a TestCaseTracker for it.
                {
                    tccmt = new TestCaseTracker(this, pickleId);
                    tccmt.ProcessEvent(scenarioStartedEvent);
                    testCaseTrackersById.TryAdd(pickleId, tccmt);
                }
            }
        }

        public IEnumerable<Envelope> ProcessEvent(ScenarioFinishedEvent scenarioFinishedEvent)
        {
            var pickleIndex = scenarioFinishedEvent.ScenarioContext?.ScenarioInfo?.PickleIdIndex;
            if (String.IsNullOrEmpty(pickleIndex)) return Enumerable.Empty<Envelope>();

            if (PickleIds.TryGetValue(pickleIndex, out var pickleId))
            {
                if (testCaseTrackersById.TryGetValue(pickleId, out var tccmt))
                {
                    tccmt.ProcessEvent(scenarioFinishedEvent);

                    return tccmt.TestCaseCucumberMessages();
                }
            }
            return Enumerable.Empty<Envelope>();
        }

        public void ProcessEvent(StepStartedEvent stepStartedEvent)
        {
            var pickleIndex = stepStartedEvent.ScenarioContext?.ScenarioInfo?.PickleIdIndex;

            if (String.IsNullOrEmpty(pickleIndex)) return;

            if (PickleIds.TryGetValue(pickleIndex, out var pickleId))
            {
                if (testCaseTrackersById.TryGetValue(pickleId, out var tccmt))
                {
                    tccmt.ProcessEvent(stepStartedEvent);
                }
            }
        }

        public void ProcessEvent(StepFinishedEvent stepFinishedEvent)
        {
            var pickleIndex = stepFinishedEvent.ScenarioContext?.ScenarioInfo?.PickleIdIndex;

            if (String.IsNullOrEmpty(pickleIndex)) return;
            if (PickleIds.TryGetValue(pickleIndex, out var pickleId))
            {
                if (testCaseTrackersById.TryGetValue(pickleId, out var tccmt))
                {
                    tccmt.ProcessEvent(stepFinishedEvent);
                }
            }
        }

        public void ProcessEvent(HookBindingStartedEvent hookBindingStartedEvent)
        {
            var pickleIndex = hookBindingStartedEvent.ContextManager?.ScenarioContext?.ScenarioInfo?.PickleIdIndex;

            if (String.IsNullOrEmpty(pickleIndex)) return;

            if (PickleIds.TryGetValue(pickleIndex, out var pickleId))
            {
                if (testCaseTrackersById.TryGetValue(pickleId, out var tccmt))
                    tccmt.ProcessEvent(hookBindingStartedEvent);
            }
        }

        public void ProcessEvent(HookBindingFinishedEvent hookBindingFinishedEvent)
        {
            var pickleIndex = hookBindingFinishedEvent.ContextManager?.ScenarioContext?.ScenarioInfo?.PickleIdIndex;

            if (String.IsNullOrEmpty(pickleIndex)) return;

            if (PickleIds.TryGetValue(pickleIndex, out var pickleId))
            {
                if (testCaseTrackersById.TryGetValue(pickleId, out var tccmt))
                    tccmt.ProcessEvent(hookBindingFinishedEvent);
            }
        }

        public void ProcessEvent(AttachmentAddedEvent attachmentAddedEvent)
        {
            var pickleId = attachmentAddedEvent.FeatureInfo?.CucumberMessages_PickleId;

            if (String.IsNullOrEmpty(pickleId)) return;

            if (testCaseTrackersById.TryGetValue(pickleId, out var tccmt))
            {
                tccmt.ProcessEvent(attachmentAddedEvent);
            }
        }

        public void ProcessEvent(OutputAddedEvent outputAddedEvent)
        {
            var pickleId = outputAddedEvent.FeatureInfo?.CucumberMessages_PickleId;

            if (String.IsNullOrEmpty(pickleId)) return;

            if (testCaseTrackersById.TryGetValue(pickleId, out var tccmt))
            {
                tccmt.ProcessEvent(outputAddedEvent);
            }
        }
    }
}