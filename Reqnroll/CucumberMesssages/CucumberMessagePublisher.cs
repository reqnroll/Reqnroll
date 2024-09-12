using Reqnroll.BoDi;
using Reqnroll.Events;
using Reqnroll.Tracing;
using Reqnroll.Plugins;
using Reqnroll.UnitTestProvider;
using System.Collections.Concurrent;
using System.Diagnostics;

namespace Reqnroll.CucumberMessages
{
    public class CucumberMessagePublisher : ICucumberMessagePublisher, IRuntimePlugin
    {
        private ICucumberMessageBroker broker;
        private IObjectContainer objectContainer;
        private ConcurrentDictionary<string, FeatureEventProcessor> featureProcessorsByFeatureName = new();
        bool Enabled = false;

        public CucumberMessagePublisher(ICucumberMessageBroker CucumberMessageBroker, IObjectContainer objectContainer)
        {
            Debugger.Launch();
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
            var featureName = featureStartedEvent.FeatureContext.FeatureInfo.Title;

            // This checks to confirm that the Feature was successfully serialized into the required GherkinDocument and Pickles;
            // if not, then this is disabled for this feature
            // if true, then it checks with the broker to confirm that a listener/sink has been registered
            Enabled = broker.Enabled;
            if (!Enabled)
                return;

            var featureEnabled = featureStartedEvent.FeatureContext.FeatureInfo.FeatureCucumberMessages.Source == null ? false : true;

            var featureProc = new FeatureEventProcessor
            {
                Name = featureName,
                Enabled = featureEnabled
            };

            // todo: need a lock around this
            if (!featureProcessorsByFeatureName.TryAdd(featureName, featureProc))
            {
                // This feature has already been started by another thread (executing a different scenario)
                var featureState_alreadyrunning = featureProcessorsByFeatureName[featureName];
                featureState_alreadyrunning.workerThreadMarkers.Push(1); // add a marker that this thread is active as well

                // None of the rest of this method should be executed
                return;
            }

            var traceListener = objectContainer.Resolve<ITraceListener>();
            traceListener.WriteTestOutput($"Cucumber Message Publisher: FeatureStartedEventHandler: {featureName}");

            if (!featureEnabled)
                return;

            ProcessEvent(featureStartedEvent, featureName);
        }

        private void FeatureFinishedEventHandler(FeatureFinishedEvent featureFinishedEvent)
        {
            if (!Enabled)
                return;


            var featureName = featureFinishedEvent.FeatureContext.FeatureInfo.Title;
            var featureProcessor = featureProcessorsByFeatureName[featureName];

            lock (featureProcessor)
            {
                // Remove the worker thread marker for this thread
                featureProcessor.workerThreadMarkers.TryPop(out int result);

                // Check if there are other threads still working on this feature
                if (featureProcessor.workerThreadMarkers.TryPeek(out result))
                {
                    // There are other threads still working on this feature, so we won't publish the TestRunFinished message just yet
                    return;
                }
                featureProcessor.Finished = true;
            }


            if (!featureProcessor.Enabled)
                return;

            ProcessEvent(featureFinishedEvent, featureName);

            foreach (var message in featureProcessor.Messages)
            {
                broker.Publish(message);
            }

            broker.Complete(featureName);
        }

        private void ScenarioStartedEventHandler(ScenarioStartedEvent scenarioStartedEvent)
        {
            if (!Enabled)
                return;


            var featureName = scenarioStartedEvent.FeatureContext.FeatureInfo.Title;
            ProcessEvent(scenarioStartedEvent, featureName);
        }

        private void ScenarioFinishedEventHandler(ScenarioFinishedEvent scenarioFinishedEvent)
        {
            if (!Enabled)
                return;


            var featureName = scenarioFinishedEvent.FeatureContext.FeatureInfo.Title;
            ProcessEvent(scenarioFinishedEvent, featureName);
        }

        private void StepStartedEventHandler(StepStartedEvent stepStartedEvent)
        {
            if (!Enabled)
                return;


            var featureName = stepStartedEvent.FeatureContext.FeatureInfo.Title;
            ProcessEvent(stepStartedEvent, featureName);
        }

        private void StepFinishedEventHandler(StepFinishedEvent stepFinishedEvent)
        {
            if (!Enabled)
                return;


            var featureName = stepFinishedEvent.FeatureContext.FeatureInfo.Title;
            ProcessEvent(stepFinishedEvent, featureName);
        }

        private void HookBindingStartedEventHandler(HookBindingStartedEvent hookBindingStartedEvent)
        {
            if (!Enabled)
                return;


            var featureName = hookBindingStartedEvent.ContextManager?.FeatureContext?.FeatureInfo?.Title;
            ProcessEvent(hookBindingStartedEvent, featureName);
        }

        private void HookBindingFinishedEventHandler(HookBindingFinishedEvent hookBindingEvent)
        {
            if (!Enabled)
                return;


            var featureName = hookBindingEvent.ContextManager?.FeatureContext?.FeatureInfo?.Title;
            ProcessEvent(hookBindingEvent, featureName);
        }

        private void AttachmentAddedEventHandler(AttachmentAddedEvent attachmentAddedEvent)
        {
            if (!Enabled)
                return;


            var featureName = attachmentAddedEvent.FeatureName;
            ProcessEvent(attachmentAddedEvent, featureName);
        }

        private void OutputAddedEventHandler(OutputAddedEvent outputAddedEvent)
        {
            if (!Enabled)
                return;


            ProcessEvent(outputAddedEvent, outputAddedEvent.FeatureName);
        }


        private void ProcessEvent(ExecutionEvent anEvent, string featureName)
        {
            if (!Enabled)
                return;


            var featureProcessor = featureProcessorsByFeatureName[featureName];
            if (!featureProcessor.Enabled) 
                return;

            featureProcessor.ProcessEvent(anEvent);
        }
    }
}
