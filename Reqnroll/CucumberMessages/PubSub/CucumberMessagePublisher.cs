using Reqnroll.BoDi;
using Reqnroll.Events;
using Reqnroll.Tracing;
using Reqnroll.Plugins;
using Reqnroll.UnitTestProvider;
using System.Collections.Concurrent;
using System.Diagnostics;
using System;
using System.Collections.Generic;
using System.Linq;
using Reqnroll.CucumberMessages.ExecutionTracking;

namespace Reqnroll.CucumberMessages.PubSub
{
    public class CucumberMessagePublisher : ICucumberMessagePublisher, IRuntimePlugin
    {
        private Lazy<ICucumberMessageBroker> _brokerFactory;
        private ICucumberMessageBroker _broker;
        private IObjectContainer objectContainer;
        private ConcurrentDictionary<string, FeatureTracker> StartedFeatures = new();
        private ConcurrentDictionary<string, TestCaseCucumberMessageTracker> testCaseTrackersById = new();
        bool Enabled = false;

        public CucumberMessagePublisher()
        {
            //Debugger.Launch();
        }
        public void Initialize(RuntimePluginEvents runtimePluginEvents, RuntimePluginParameters runtimePluginParameters, UnitTestProviderConfiguration unitTestProviderConfiguration)
        {
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
            testThreadEventPublisher.AddHandler<FeatureStartedEvent>(FeatureStartedEventHandler);
            testThreadEventPublisher.AddHandler<FeatureFinishedEvent>(FeatureFinishedEventHandler);
            testThreadEventPublisher.AddHandler<ScenarioStartedEvent>(ScenarioStartedEventHandler);
            testThreadEventPublisher.AddHandler<ScenarioFinishedEvent>(ScenarioFinishedEventHandler);
            testThreadEventPublisher.AddHandler<StepStartedEvent>(StepStartedEventHandler);
            testThreadEventPublisher.AddHandler<StepFinishedEvent>(StepFinishedEventHandler);
            testThreadEventPublisher.AddHandler<HookBindingStartedEvent>(HookBindingStartedEventHandler);
            testThreadEventPublisher.AddHandler<HookBindingFinishedEvent>(HookBindingFinishedEventHandler);
            testThreadEventPublisher.AddHandler<AttachmentAddedEvent>(AttachmentAddedEventHandler);
            testThreadEventPublisher.AddHandler<OutputAddedEvent>(OutputAddedEventHandler);
        }

        private void FeatureStartedEventHandler(FeatureStartedEvent featureStartedEvent)
        {
            _broker = _brokerFactory.Value;
            var traceListener = objectContainer.Resolve<ITraceListener>();
            var featureName = featureStartedEvent.FeatureContext.FeatureInfo.Title;

            // This checks to confirm that the Feature was successfully serialized into the required GherkinDocument and Pickles;
            // if not, then this is disabled for this feature
            // if true, then it checks with the _broker to confirm that a listener/sink has been registered
            Enabled = _broker.Enabled;
            if (!Enabled)
            {
                traceListener.WriteTestOutput($"Cucumber Message Publisher: FeatureStartedEventHandler: Broker is disabled for {featureName}.");
                return;
            }

            if (StartedFeatures.ContainsKey(featureName))
            {
                traceListener.WriteTestOutput($"Cucumber Message Publisher: FeatureStartedEventHandler: {featureName} already started");
                return;
            }

            traceListener.WriteTestOutput($"Cucumber Message Publisher: FeatureStartedEventHandler: {featureName}");
            var ft = new FeatureTracker(featureStartedEvent);
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
            var traceListener = objectContainer.Resolve<ITraceListener>();
            if (!Enabled)
            {
                traceListener.WriteTestOutput($"Cucumber Message Publisher: FeatureFinishedEventHandler: Broker is disabled for {featureFinishedEvent.FeatureContext.FeatureInfo.Title}.");
                return;
            }
            var featureName = featureFinishedEvent.FeatureContext.FeatureInfo.Title;
            if (!StartedFeatures.ContainsKey(featureName) || !StartedFeatures[featureName].Enabled)
            {
                traceListener.WriteTestOutput($"Cucumber Message Publisher: FeatureFinishedEventHandler: {featureName} was not started or is Disabled.");
                return;
            }
            var featureTestCases = testCaseTrackersById.Values.Where(tc => tc.FeatureName == featureName).ToList();

            // IF all TestCaseCucumberMessageTrackers are done, then send the messages to the CucumberMessageBroker
            if (featureTestCases.All(tc => tc.Finished))
            {
                var testRunStatus = featureTestCases.All(tc => tc.ScenarioExecutionStatus == ScenarioExecutionStatus.OK);
                var msg = Io.Cucumber.Messages.Types.Envelope.Create(CucumberMessageFactory.ToTestRunFinished(testRunStatus, featureFinishedEvent));
                _broker.Publish(new ReqnrollCucumberMessage() { CucumberMessageSource = featureName, Envelope = msg });
                _broker.Complete(featureName);
            }
            else
            {
                traceListener.WriteTestOutput($"Cucumber Message Publisher: FeatureFinishedEventHandler: Error: {featureTestCases.Count(tc => !tc.Finished)} test cases not marked as finished for Feature {featureName}. TestRunFinished event will not be sent.");
            }

            // throw an exception if any of the TestCaseCucumberMessageTrackers are not done?

        }

        private void ScenarioStartedEventHandler(ScenarioStartedEvent scenarioStartedEvent)
        {
            if (!Enabled)
                return;
            var traceListener = objectContainer.Resolve<ITraceListener>();
            var featureName = scenarioStartedEvent.FeatureContext.FeatureInfo.Title;
            if (StartedFeatures.TryGetValue(featureName, out var featureTracker))
            {
                if (featureTracker.Enabled)
                {
                    var id = featureName + scenarioStartedEvent.ScenarioContext.ScenarioInfo.PickleId;
                    var tccmt = new TestCaseCucumberMessageTracker(featureTracker);
                    tccmt.ProcessEvent(scenarioStartedEvent);
                    traceListener.WriteTestOutput($"Cucumber Message Publisher: ScenarioStartedEventHandler: {featureName} {id} started");
                    testCaseTrackersById.TryAdd(id, tccmt);
                }
                else
                {
                    traceListener.WriteTestOutput($"Cucumber Message Publisher: ScenarioStartedEventHandler: {featureName} FeatureTracker is disabled");
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
            if (!Enabled)
                return;
            var testCaseTrackerId = scenarioFinishedEvent.FeatureContext.FeatureInfo.CucumberMessages_TestCaseTrackerId;
            if (testCaseTrackerId != null && testCaseTrackersById.TryGetValue(testCaseTrackerId, out var tccmt))
            {
                tccmt.ProcessEvent(scenarioFinishedEvent);

                foreach (var msg in tccmt.TestCaseCucumberMessages())
                {
                    _broker.Publish(new ReqnrollCucumberMessage() { CucumberMessageSource = tccmt.FeatureName, Envelope = msg });
                }
            }
        }

        private void StepStartedEventHandler(StepStartedEvent stepStartedEvent)
        {
            if (!Enabled)
                return;
            var testCaseTrackerId = stepStartedEvent.FeatureContext.FeatureInfo.CucumberMessages_TestCaseTrackerId;
            if (testCaseTrackerId != null && testCaseTrackersById.TryGetValue(testCaseTrackerId, out var tccmt))
                tccmt.ProcessEvent(stepStartedEvent);
        }

        private void StepFinishedEventHandler(StepFinishedEvent stepFinishedEvent)
        {
            if (!Enabled)
                return;

            var testCaseTrackerId = stepFinishedEvent.FeatureContext.FeatureInfo.CucumberMessages_TestCaseTrackerId;
            if (testCaseTrackerId != null && testCaseTrackersById.TryGetValue(testCaseTrackerId, out var tccmt))
                tccmt.ProcessEvent(stepFinishedEvent);
        }

        private void HookBindingStartedEventHandler(HookBindingStartedEvent hookBindingStartedEvent)
        {
            if (!Enabled)
                return;

            // FeatureContext and FeatureInfo will not be available for BeforeTestRun, AfterTestRun, BeforeFeature, AfterFeature hooks. 
            // Bypass them by checking for null
            var testCaseTrackerId = hookBindingStartedEvent.ContextManager.FeatureContext?.FeatureInfo?.CucumberMessages_TestCaseTrackerId;
            if (testCaseTrackerId != null && testCaseTrackersById.TryGetValue(testCaseTrackerId, out var tccmt))
                tccmt.ProcessEvent(hookBindingStartedEvent);
        }

        private void HookBindingFinishedEventHandler(HookBindingFinishedEvent hookBindingFinishedEvent)
        {
            if (!Enabled)
                return;

            // FeatureContext and FeatureInfo will not be available for BeforeTestRun, AfterTestRun, BeforeFeature, AfterFeature hooks. 
            // Bypass them by checking for null
            var testCaseTrackerId = hookBindingFinishedEvent.ContextManager.FeatureContext?.FeatureInfo?.CucumberMessages_TestCaseTrackerId;
            if (testCaseTrackerId != null && testCaseTrackersById.TryGetValue(testCaseTrackerId, out var tccmt))
                tccmt.ProcessEvent(hookBindingFinishedEvent);
        }

        private void AttachmentAddedEventHandler(AttachmentAddedEvent attachmentAddedEvent)
        {
            if (!Enabled)
                return;

            var testCaseTrackerId = attachmentAddedEvent.FeatureInfo.CucumberMessages_TestCaseTrackerId;
            if (testCaseTrackerId != null && testCaseTrackersById.TryGetValue(testCaseTrackerId, out var tccmt))
                tccmt.ProcessEvent(attachmentAddedEvent);
        }

        private void OutputAddedEventHandler(OutputAddedEvent outputAddedEvent)
        {
            if (!Enabled)
                return;

            var testCaseTrackerId = outputAddedEvent.FeatureInfo.CucumberMessages_TestCaseTrackerId;
            if (testCaseTrackerId != null && testCaseTrackersById.TryGetValue(testCaseTrackerId, out var tccmt))
                tccmt.ProcessEvent(outputAddedEvent);
        }
    }
}
