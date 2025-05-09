using Gherkin.CucumberMessages;
using Io.Cucumber.Messages.Types;
using Reqnroll.Bindings;
using Reqnroll.CucumberMessages.PayloadProcessing.Cucumber;
using Reqnroll.CucumberMessages.RuntimeSupport;
using Reqnroll.Events;
using Reqnroll.Time;
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
    public class FeatureTracker : IFeatureTracker
    {
        // Static Messages are those generated during code generation (Source, GherkinDocument & Pickles)
        // and the StepTransformations, StepDefinitions and Hook messages which are global to the entire Solution.
        public IEnumerable<Envelope> StaticMessages => _staticMessages.Value;
        private Lazy<IEnumerable<Envelope>> _staticMessages;

        // ID Generator to use when generating IDs for TestCase messages and beyond
        // If gherkin feature was generated using integer IDs, then we will use an integer ID generator seeded with the last known integer ID
        // otherwise we'll use a GUID ID generator. We can't know ahead of time which type of ID generator to use, therefore this is not set by the constructor.
        public IIdGenerator IDGenerator { get; set; }
        public string TestRunStartedId { get; }

        // This dictionary tracks the StepDefintions(ID) by their method signature
        // used during TestCase creation to map from a Step Definition binding to its ID
        // This dictionary is shared across all Features (via the Publisher)
        internal ConcurrentDictionary<string, string> StepDefinitionsByPattern = new();

        // This maintains the list of TestCases, identified by (string) PickkleID, running within this Feature
        internal TestCaseTrackers TestCaseTrackersById;

        public string FeatureName { get; }
        public bool Enabled { get; }

        // This dictionary maps from (string) PickleIDIndex to (string) PickleID
        internal Dictionary<string, string> PickleIds { get; } = new();
        private PickleJar PickleJar { get; set; }

        public bool FeatureExecutionSuccess { get; private set; }

        // This constructor is used by the Publisher when it sees a Feature (by name) for the first time
        public FeatureTracker(FeatureStartedEvent featureStartedEvent, string testRunStartedId, IIdGenerator idGenerator, ConcurrentDictionary<string, string> stepDefinitionPatterns)
        {
            TestRunStartedId = testRunStartedId;
            StepDefinitionsByPattern = stepDefinitionPatterns;
            IDGenerator = idGenerator;
            FeatureName = featureStartedEvent.FeatureContext.FeatureInfo.Title;
            var featureHasCucumberMessages = featureStartedEvent.FeatureContext.FeatureInfo.FeatureCucumberMessages != null;
            Enabled = featureHasCucumberMessages && featureStartedEvent.FeatureContext.FeatureInfo.FeatureCucumberMessages.Source != null ? true : false;
            TestCaseTrackersById = new TestCaseTrackers(this, featureStartedEvent.FeatureContext.FeatureContainer.Resolve<IClock>());
            ProcessEvent(featureStartedEvent);
        }

        // At the completion of Feature execution, this is called to generate all non-static Messages
        // Iterating through all Scenario trackers, generating all messages.
        public IEnumerable<Envelope> RuntimeGeneratedMessages
        {
            get
            {
                return TestCaseTrackersById.GetAll().OrderBy(tc => tc.TestCaseStartedTimeStamp).SelectMany(scenario => scenario.RuntimeGeneratedMessages);
            }
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

        // When the FeatureFinished event fires, we calculate the Feature-level Execution Status
        // If Scenarios are running in parallel, this event will fire multiple times (once per each instance of the test class).
        // Running this method multiple times is harmless. The FeatureExecutionSuccess property is only consumed upon the TestRunComplete event (ie, only once).
        public void ProcessEvent(FeatureFinishedEvent featureFinishedEvent)
        {
            var testCases = TestCaseTrackersById.GetAll().ToList();

            // Calculate the Feature-level Execution Status
            FeatureExecutionSuccess = testCases.All(tc => tc.Finished && tc.ScenarioExecutionStatus == ScenarioExecutionStatus.OK);
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

                if (TestCaseTrackersById.TryGet(pickleId, out var tct))
                {
                    // This represents a re-execution of the TestCase.
                    tct.ProcessEvent((ExecutionEvent)scenarioStartedEvent);
                }
                else // This is the first time this TestCase (aka, pickle) is getting executed. New up a TestCaseTracker for it.
                {
                    if (TestCaseTrackersById.TryAddNew(pickleId, out var newtct))
                    {
                        newtct.ProcessEvent((ExecutionEvent)scenarioStartedEvent);
                    }
                }
            }
        }

        public void ProcessEvent(ScenarioFinishedEvent scenarioFinishedEvent)
        {
            var pickleIndex = scenarioFinishedEvent.ScenarioContext?.ScenarioInfo?.PickleIdIndex;
            if (String.IsNullOrEmpty(pickleIndex)) return;

            if (PickleIds.TryGetValue(pickleIndex, out var pickleId))
            {
                if (TestCaseTrackersById.TryGet(pickleId, out var tccmt))
                {
                    tccmt.ProcessEvent((ExecutionEvent)scenarioFinishedEvent);
                }
            }
        }

        public void ProcessEvent(StepStartedEvent stepStartedEvent)
        {
            var pickleIndex = stepStartedEvent.ScenarioContext?.ScenarioInfo?.PickleIdIndex;

            if (String.IsNullOrEmpty(pickleIndex)) return;

            if (PickleIds.TryGetValue(pickleIndex, out var pickleId))
            {
                if (TestCaseTrackersById.TryGet(pickleId, out var tccmt))
                {
                    tccmt.ProcessEvent((ExecutionEvent)stepStartedEvent);
                }
            }
        }

        public void ProcessEvent(StepFinishedEvent stepFinishedEvent)
        {
            var pickleIndex = stepFinishedEvent.ScenarioContext?.ScenarioInfo?.PickleIdIndex;

            if (String.IsNullOrEmpty(pickleIndex)) return;
            if (PickleIds.TryGetValue(pickleIndex, out var pickleId))
            {
                if (TestCaseTrackersById.TryGet(pickleId, out var tccmt))
                {
                    tccmt.ProcessEvent((ExecutionEvent)stepFinishedEvent);
                }
            }
        }

        public void ProcessEvent(HookBindingStartedEvent hookBindingStartedEvent)
        {
            var pickleIndex = hookBindingStartedEvent.ContextManager?.ScenarioContext?.ScenarioInfo?.PickleIdIndex;

            if (String.IsNullOrEmpty(pickleIndex)) return;

            if (PickleIds.TryGetValue(pickleIndex, out var pickleId))
            {
                if (TestCaseTrackersById.TryGet(pickleId, out var tccmt))
                    tccmt.ProcessEvent((ExecutionEvent)hookBindingStartedEvent);
            }
        }

        public void ProcessEvent(HookBindingFinishedEvent hookBindingFinishedEvent)
        {
            var pickleIndex = hookBindingFinishedEvent.ContextManager?.ScenarioContext?.ScenarioInfo?.PickleIdIndex;

            if (String.IsNullOrEmpty(pickleIndex)) return;

            if (PickleIds.TryGetValue(pickleIndex, out var pickleId))
            {
                if (TestCaseTrackersById.TryGet(pickleId, out var tccmt))
                    tccmt.ProcessEvent((ExecutionEvent)hookBindingFinishedEvent);
            }
        }

        public void ProcessEvent(AttachmentAddedEvent attachmentAddedEvent)
        {

            var pickleId = attachmentAddedEvent.FeatureInfo?.CucumberMessages_PickleId;

            if (String.IsNullOrEmpty(pickleId)) return;

            if (TestCaseTrackersById.TryGet(pickleId, out var tccmt))
            {
                tccmt.ProcessEvent((ExecutionEvent)attachmentAddedEvent);
            }
        }

        public void ProcessEvent(OutputAddedEvent outputAddedEvent)
        {
            var pickleId = outputAddedEvent.FeatureInfo?.CucumberMessages_PickleId;

            if (String.IsNullOrEmpty(pickleId)) return;

            if (TestCaseTrackersById.TryGet(pickleId, out var tccmt))
            {
                tccmt.ProcessEvent((ExecutionEvent)outputAddedEvent);
            }
        }
    }
}