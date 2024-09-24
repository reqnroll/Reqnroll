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

namespace Reqnroll.CucumberMessages
{
    public class CucumberMessagePublisher : ICucumberMessagePublisher, IRuntimePlugin
    {
        private ICucumberMessageBroker broker;
        private IObjectContainer objectContainer;
        private ConcurrentDictionary<string, FeatureTracker> StartedFeatures = new();
        private ConcurrentDictionary<string, TestCaseCucumberMessageTracker> testCaseTrackersById = new();
        bool Enabled = false;

        public CucumberMessagePublisher(ICucumberMessageBroker CucumberMessageBroker, IObjectContainer objectContainer)
        {
            //Debugger.Launch();
            broker = CucumberMessageBroker;
        }
        public void Initialize(RuntimePluginEvents runtimePluginEvents, RuntimePluginParameters runtimePluginParameters, UnitTestProviderConfiguration unitTestProviderConfiguration)
        {
            runtimePluginEvents.CustomizeFeatureDependencies += (sender, args) =>
            {
                objectContainer = args.ObjectContainer;
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
            var traceListener = objectContainer.Resolve<ITraceListener>();
            var featureName = featureStartedEvent.FeatureContext.FeatureInfo.Title;

            // This checks to confirm that the Feature was successfully serialized into the required GherkinDocument and Pickles;
            // if not, then this is disabled for this feature
            // if true, then it checks with the broker to confirm that a listener/sink has been registered
            Enabled = broker.Enabled;
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
            if (StartedFeatures.TryAdd(featureName, ft))
            {
                foreach (var msg in ft.StaticMessages)
                {
                    broker.Publish(new ReqnrollCucumberMessage() { CucumberMessageSource = featureName, Envelope = msg });
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
            var featureTestCases = testCaseTrackersById.Values.Where(tc => tc.FeatureName == featureName).ToList();

            // IF all TestCaseCucumberMessageTrackers are done, then send the messages to the CucumberMessageBroker
            if (featureTestCases.All(tc => tc.Finished))
            {
                var testRunStatus = featureTestCases.All(tc => tc.ScenarioExecutionStatus == ScenarioExecutionStatus.OK);
                var msg = Io.Cucumber.Messages.Types.Envelope.Create(CucumberMessageFactory.ToTestRunFinished(testRunStatus, featureFinishedEvent));
                broker.Publish(new ReqnrollCucumberMessage() { CucumberMessageSource = featureName, Envelope = msg });
                broker.Complete(featureName);
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
            var id = featureName + scenarioStartedEvent.ScenarioContext.ScenarioInfo.PickleId;
            if (StartedFeatures.TryGetValue(featureName, out var featureTracker))
            {
                var tccmt = new TestCaseCucumberMessageTracker(featureTracker);
                traceListener.WriteTestOutput($"Cucumber Message Publisher: ScenarioStartedEventHandler: {featureName} {id} started");
                testCaseTrackersById.TryAdd(id, tccmt);
                tccmt.ProcessEvent(scenarioStartedEvent);
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

            var tccmt = testCaseTrackersById[scenarioFinishedEvent.FeatureContext.FeatureInfo.CucumberMessages_TestCaseTrackerId];
            tccmt.ProcessEvent(scenarioFinishedEvent);

            foreach (var msg in tccmt.TestCaseCucumberMessages())
            {
                broker.Publish(new ReqnrollCucumberMessage() { CucumberMessageSource = tccmt.FeatureName, Envelope = msg });
            }
        }

        private void StepStartedEventHandler(StepStartedEvent stepStartedEvent)
        {
            if (!Enabled)
                return;

            var tccmt = testCaseTrackersById[stepStartedEvent.FeatureContext.FeatureInfo.CucumberMessages_TestCaseTrackerId];
            tccmt.ProcessEvent(stepStartedEvent);
        }

        private void StepFinishedEventHandler(StepFinishedEvent stepFinishedEvent)
        {
            if (!Enabled)
                return;

            var tccmt = testCaseTrackersById[stepFinishedEvent.FeatureContext.FeatureInfo.CucumberMessages_TestCaseTrackerId];
            tccmt.ProcessEvent(stepFinishedEvent);
        }

        private void HookBindingStartedEventHandler(HookBindingStartedEvent hookBindingStartedEvent)
        {
            if (!Enabled)
                return;

            var tccmt = testCaseTrackersById[hookBindingStartedEvent.ContextManager.FeatureContext.FeatureInfo.CucumberMessages_TestCaseTrackerId];
            tccmt.ProcessEvent(hookBindingStartedEvent);
        }

        private void HookBindingFinishedEventHandler(HookBindingFinishedEvent hookBindingFinishedEvent)
        {
            if (!Enabled)
                return;

            var tccmt = testCaseTrackersById[hookBindingFinishedEvent.ContextManager.FeatureContext.FeatureInfo.CucumberMessages_TestCaseTrackerId];
            tccmt.ProcessEvent(hookBindingFinishedEvent);
        }

        private void AttachmentAddedEventHandler(AttachmentAddedEvent attachmentAddedEvent)
        {
            if (!Enabled)
                return;

            var tccmt = testCaseTrackersById[attachmentAddedEvent.FeatureInfo.CucumberMessages_TestCaseTrackerId];
            tccmt.ProcessEvent(attachmentAddedEvent);
        }

        private void OutputAddedEventHandler(OutputAddedEvent outputAddedEvent)
        {
            if (!Enabled)
                return;

            var tccmt = testCaseTrackersById[outputAddedEvent.FeatureInfo.CucumberMessages_TestCaseTrackerId];
            tccmt.ProcessEvent(outputAddedEvent);
        }
    }
}
