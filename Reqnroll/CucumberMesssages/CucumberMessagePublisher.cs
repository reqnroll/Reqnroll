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

namespace Reqnroll.CucumberMesssages
{
    public class CucumberMessagePublisher : ICucumberMessagePublisher, IRuntimePlugin
    {
        private ICucumberMessageBroker broker;
        private IObjectContainer objectContainer;
        //private bool initialized = false;

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
        }


        private void FeatureFinishedEventHandler(FeatureFinishedEvent featureFinishedEvent)
        {
            var featureName = featureFinishedEvent.FeatureContext.FeatureInfo.Title;

            var ts = objectContainer.Resolve<IClock>().GetNowDateAndTime();

            broker.Publish(new ReqnrollCucumberMessage
            {
                CucumberMessageSource = featureName,
                Envelope = Envelope.Create(new TestRunFinished(null, true, Converters.ToTimestamp(ts), null))
            });

            broker.Complete(featureName);
        }

        private void FeatureStartedEventHandler(FeatureStartedEvent featureStartedEvent)
        {
            var featureName = featureStartedEvent.FeatureContext.FeatureInfo.Title;

            var traceListener = objectContainer.Resolve<ITraceListener>();
            traceListener.WriteTestOutput($"FeatureStartedEventHandler: {featureName}");

            var ts = objectContainer.Resolve<IClock>().GetNowDateAndTime();

            broker.Publish(new ReqnrollCucumberMessage
            {
                CucumberMessageSource = featureName,
                Envelope = Envelope.Create(new TestRunStarted(Converters.ToTimestamp(ts)))
            });
        }
    }
}
