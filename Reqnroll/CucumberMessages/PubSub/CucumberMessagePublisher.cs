using Reqnroll.BoDi;
using Reqnroll.Events;
using Reqnroll.Tracing;
using Reqnroll.Plugins;
using Reqnroll.UnitTestProvider;
using System.Collections.Concurrent;
using System;
using System.Collections.Generic;
using System.Linq;
using Reqnroll.CucumberMessages.ExecutionTracking;
using Reqnroll.CucumberMessages.PayloadProcessing.Cucumber;
using Io.Cucumber.Messages.Types;

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
    public class CucumberMessagePublisher : ICucumberMessagePublisher, IRuntimePlugin
    {
        private Lazy<ICucumberMessageBroker> _brokerFactory;
        private ICucumberMessageBroker _broker;
        private IObjectContainer objectContainer;

        // Started Features by name
        private ConcurrentDictionary<string, FeatureTracker> StartedFeatures = new();
        private ConcurrentDictionary<string, TestCaseCucumberMessageTracker> testCaseTrackersById = new();
        bool Enabled = false;

        public CucumberMessagePublisher()
        {
        }
        public void Initialize(RuntimePluginEvents runtimePluginEvents, RuntimePluginParameters runtimePluginParameters, UnitTestProviderConfiguration unitTestProviderConfiguration)
        {
            runtimePluginEvents.CustomizeGlobalDependencies += (sender, args) =>
            {
                var pluginLifecycleEvents = args.ObjectContainer.Resolve<RuntimePluginTestExecutionLifecycleEvents>();
                pluginLifecycleEvents.BeforeTestRun += PublisherStartup;
            };
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

        // This method will get called after TestRunStartedEvent has been published and after any BeforeTestRun hooks have been called
        // The TestRunStartedEvent will be used by the FileOutputPlugin to launch the File writing thread and establish Messages configuration
        // Running this after the BeforeTestRun hooks will allow them to programmatically configure CucumberMessages
        private void PublisherStartup(object sender, RuntimePluginBeforeTestRunEventArgs args)
        {
            _broker = _brokerFactory.Value;

            Enabled = _broker.Enabled;

            if (!Enabled)
            {
                return;
            }
            _broker.Publish(new ReqnrollCucumberMessage() { CucumberMessageSource = "startup", Envelope = Envelope.Create(CucumberMessageFactory.ToTestRunStarted(DateTime.Now)) });
        }

        private void FeatureStartedEventHandler(FeatureStartedEvent featureStartedEvent)
        {
            _broker = _brokerFactory.Value;
            var traceListener = objectContainer.Resolve<ITraceListener>();
            var featureName = featureStartedEvent.FeatureContext.FeatureInfo.Title;

            Enabled = _broker.Enabled;
            if (!Enabled)
            {
                return;
            }

            if (StartedFeatures.ContainsKey(featureName))
            {
                // Already started, don't repeat the following steps
                return;
            }

            var ft = new FeatureTracker(featureStartedEvent);

            // This will add a FeatureTracker to the StartedFeatures dictionary only once, and if it is enabled, it will publish the static messages shared by all steps.
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
            if (!Enabled)
            {
                return;
            }
            var featureName = featureFinishedEvent.FeatureContext.FeatureInfo.Title;
            if (!StartedFeatures.ContainsKey(featureName) || !StartedFeatures[featureName].Enabled)
            {
                return;
            }
            var featureTestCases = testCaseTrackersById.Values.Where(tc => tc.FeatureName == featureName).ToList();

            // IF all TestCaseCucumberMessageTrackers are done, then send the messages to the CucumberMessageBroker
            if (featureTestCases.All(tc => tc.Finished))
            {
                var testRunStatus = featureTestCases.All(tc => tc.ScenarioExecutionStatus == ScenarioExecutionStatus.OK);
                var msg = Io.Cucumber.Messages.Types.Envelope.Create(CucumberMessageFactory.ToTestRunFinished(testRunStatus, featureFinishedEvent));
                _broker.Publish(new ReqnrollCucumberMessage() { CucumberMessageSource = featureName, Envelope = msg });
            }
            else
            {
                // If the feature has no steps, then we should send the finished message;
                if (featureTestCases.Count == 0)
                {
                    var msg = Io.Cucumber.Messages.Types.Envelope.Create(CucumberMessageFactory.ToTestRunFinished(true, featureFinishedEvent));
                    _broker.Publish(new ReqnrollCucumberMessage() { CucumberMessageSource = featureName, Envelope = msg });
                }

                // else is an error of a nonsensical state; should we throw an exception?
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
                    var pickleId = featureTracker.PickleIds[scenarioStartedEvent.ScenarioContext.ScenarioInfo.PickleIdIndex];
                    var id = featureName + @"/" + pickleId;
                    var tccmt = new TestCaseCucumberMessageTracker(featureTracker);
                    tccmt.ProcessEvent(scenarioStartedEvent);
                    testCaseTrackersById.TryAdd(id, tccmt);
                }
                else
                {
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

            var testCaseTrackerId = hookBindingStartedEvent.ContextManager.FeatureContext?.FeatureInfo?.CucumberMessages_TestCaseTrackerId;
            if (testCaseTrackerId != null && testCaseTrackersById.TryGetValue(testCaseTrackerId, out var tccmt))
                tccmt.ProcessEvent(hookBindingStartedEvent);
        }

        private void HookBindingFinishedEventHandler(HookBindingFinishedEvent hookBindingFinishedEvent)
        {
            if (!Enabled)
                return;

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
