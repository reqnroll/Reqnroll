using Reqnroll.BoDi;
using Reqnroll.Events;
using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;
using Io.Cucumber.Messages;
using Io.Cucumber.Messages.Types;
using Reqnroll.Tracing;
using Reqnroll.Plugins;
using Reqnroll.UnitTestProvider;
using Reqnroll.Time;
using Cucumber.Messages;
using Reqnroll.Bindings;
using System.Reflection;
using System.Collections.Concurrent;
using System.Linq;
using System.Diagnostics;

namespace Reqnroll.CucumberMesssages
{
    public class CucumberMessagePublisher : ICucumberMessagePublisher, IRuntimePlugin
    {
        private ICucumberMessageBroker broker;
        private IObjectContainer objectContainer;
        private ConcurrentDictionary<string, FeatureEventProcessor> featureProcessorsByFeatureName = new();

        public CucumberMessagePublisher(ICucumberMessageBroker CucumberMessageBroker, IObjectContainer objectContainer)
        {
 //           Debugger.Launch();
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
            var enabled = featureStartedEvent.FeatureContext.FeatureInfo.FeatureCucumberMessages.Source == null ? false : true;

            var featureProc = new FeatureEventProcessor
            {
                Name = featureName,
                Enabled = enabled
            };

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

            if (!enabled)
                return;

            ProcessEvent(featureStartedEvent, featureName);
        }

        private void FeatureFinishedEventHandler(FeatureFinishedEvent featureFinishedEvent)
        {
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
            var featureName = scenarioStartedEvent.FeatureContext.FeatureInfo.Title;
            ProcessEvent(scenarioStartedEvent, featureName);
        }

        private void ScenarioFinishedEventHandler(ScenarioFinishedEvent scenarioFinishedEvent)
        {
            var featureName = scenarioFinishedEvent.FeatureContext.FeatureInfo.Title;
            ProcessEvent(scenarioFinishedEvent, featureName);
        }

        private void StepStartedEventHandler(StepStartedEvent stepStartedEvent)
        {
            var featureName = stepStartedEvent.FeatureContext.FeatureInfo.Title;
            ProcessEvent(stepStartedEvent, featureName);
        }

        private void StepFinishedEventHandler(StepFinishedEvent stepFinishedEvent)
        {
            var featureName = stepFinishedEvent.FeatureContext.FeatureInfo.Title;
            ProcessEvent(stepFinishedEvent, featureName);
        }

        private void HookBindingStartedEventHandler(HookBindingStartedEvent hookBindingStartedEvent)
        {
            var featureName = hookBindingStartedEvent.ContextManager?.FeatureContext?.FeatureInfo?.Title;
            ProcessEvent(hookBindingStartedEvent, featureName);
        }

        private void HookBindingFinishedEventHandler(HookBindingFinishedEvent hookBindingEvent)
        {
            var featureName = hookBindingEvent.ContextManager?.FeatureContext?.FeatureInfo?.Title;
            ProcessEvent(hookBindingEvent, featureName);
        }

        private void AttachmentAddedEventHandler(AttachmentAddedEvent attachmentAddedEvent)
        {
            var featureName = attachmentAddedEvent.FeatureName;
            ProcessEvent(attachmentAddedEvent, featureName);
        }

        private void OutputAddedEventHandler(OutputAddedEvent outputAddedEvent)
        {
           ProcessEvent(outputAddedEvent, outputAddedEvent.FeatureName);
        }


        private void ProcessEvent(ExecutionEvent anEvent, string featureName)
        {
            var featureProcessor = featureProcessorsByFeatureName[featureName];

            featureProcessor.ProcessEvent(anEvent);
        }
    }
}
