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

namespace Reqnroll.CucumberMesssages
{
    public class CucumberMessagePublisher : ICucumberMessagePublisher, IRuntimePlugin
    {
        private ICucumberMessageBroker broker;
        private IObjectContainer objectContainer;
        private ConcurrentDictionary<string, FeatureState> featureStatesByFeatureName = new();

        public CucumberMessagePublisher(ICucumberMessageBroker CucumberMessageBroker, IObjectContainer objectContainer)
        {
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

            var traceListener = objectContainer.Resolve<ITraceListener>();
            traceListener.WriteTestOutput("HookIntoTestThreadExecutionEventPublisher");

            testThreadEventPublisher.AddHandler<FeatureStartedEvent>(FeatureStartedEventHandler);
            testThreadEventPublisher.AddHandler<FeatureFinishedEvent>(FeatureFinishedEventHandler);
            testThreadEventPublisher.AddHandler<ScenarioStartedEvent>(ScenarioStartedEventHandler);
            testThreadEventPublisher.AddHandler<ScenarioFinishedEvent>(ScenarioFinishedEventHandler);
            testThreadEventPublisher.AddHandler<StepStartedEvent>(StepStartedEventHandler);
            testThreadEventPublisher.AddHandler<StepFinishedEvent>(StepFinishedEventHandler);
        }
        private void FeatureStartedEventHandler(FeatureStartedEvent featureStartedEvent)
        {
            var featureName = featureStartedEvent.FeatureContext.FeatureInfo.Title;
            var enabled = featureStartedEvent.FeatureContext.FeatureInfo.FeatureCucumberMessages.Source == null ? false : true;

            var featureState = new FeatureState
            {
                Name = featureName,
                Enabled = enabled
            };

            if (!featureStatesByFeatureName.TryAdd(featureName, featureState))
            {
                // This feature has already been started by another thread (executing a different scenario)
                var featureState_alreadyrunning = featureStatesByFeatureName[featureName];
                featureState_alreadyrunning.workerThreadMarkers.Push(1); // add a marker that this thread is active as well

                // None of the rest of this method should be executed
                return;
            }

            var traceListener = objectContainer.Resolve<ITraceListener>();
            traceListener.WriteTestOutput($"FeatureStartedEventHandler: {featureName}");

            if (!enabled)
                return;

            foreach (Envelope e in featureState.ProcessEvent(featureStartedEvent))
            {
                featureState.Messages.Enqueue(new ReqnrollCucumberMessage { CucumberMessageSource = featureName, Envelope = e });
            }
        }

        private void FeatureFinishedEventHandler(FeatureFinishedEvent featureFinishedEvent)
        {
            var featureName = featureFinishedEvent.FeatureContext.FeatureInfo.Title;
            var featureState = featureStatesByFeatureName[featureName];

            lock (featureState)
            {
                // Remove the worker thread marker for this thread
                featureState.workerThreadMarkers.TryPop(out int result);

                // Check if there are other threads still working on this feature
                if (featureState.workerThreadMarkers.TryPeek(out result))
                {
                    // There are other threads still working on this feature, so we won't publish the TestRunFinished message just yet
                    return;
                }
                featureState.Finished = true;
            }


            if (!featureState.Enabled)
                return;

            foreach (Envelope e in featureState.ProcessEvent(featureFinishedEvent))
            {
                featureState.Messages.Enqueue(new ReqnrollCucumberMessage { CucumberMessageSource = featureName, Envelope = e });
            }

            foreach (var message in featureState.Messages)
            {
                broker.Publish(message);
            }

            broker.Complete(featureName);
        }

        private void ScenarioStartedEventHandler(ScenarioStartedEvent scenarioStartedEvent)
        {
            var featureName = scenarioStartedEvent.FeatureContext.FeatureInfo.Title;
            var featureState = featureStatesByFeatureName[featureName];

            foreach (Envelope e in featureState.ProcessEvent(scenarioStartedEvent))
            {
                featureState.Messages.Enqueue(new ReqnrollCucumberMessage { CucumberMessageSource = featureName, Envelope = e });
            }
        }
        private void ScenarioFinishedEventHandler(ScenarioFinishedEvent scenarioFinishedEvent)
        {
            var featureName = scenarioFinishedEvent.FeatureContext.FeatureInfo.Title;
            var featureState = featureStatesByFeatureName[featureName];
            foreach (Envelope e in featureState.ProcessEvent(scenarioFinishedEvent))
            {
                featureState.Messages.Enqueue(new ReqnrollCucumberMessage { CucumberMessageSource = featureName, Envelope = e });
            }
        }

        private void StepStartedEventHandler(StepStartedEvent stepStartedEvent)
        {
            var featureName = stepStartedEvent.FeatureContext.FeatureInfo.Title;
            var featureState = featureStatesByFeatureName[featureName];
            foreach (Envelope e in featureState.ProcessEvent(stepStartedEvent))
            {
                featureState.Messages.Enqueue(new ReqnrollCucumberMessage { CucumberMessageSource = featureName, Envelope = e });
            }
        }

        private void StepFinishedEventHandler(StepFinishedEvent stepFinishedEvent)
        {
            var featureName = stepFinishedEvent.FeatureContext.FeatureInfo.Title;
            var featureState = featureStatesByFeatureName[featureName];
            foreach (Envelope e in featureState.ProcessEvent(stepFinishedEvent))
            {
                featureState.Messages.Enqueue(new ReqnrollCucumberMessage { CucumberMessageSource = featureName, Envelope = e });
            }
        }


    }
}
