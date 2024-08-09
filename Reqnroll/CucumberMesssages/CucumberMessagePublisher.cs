using Reqnroll.BoDi;
using Reqnroll.Events;
using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;
using Io.Cucumber.Messages;
using Io.Cucumber.Messages.Types;
using Reqnroll.Tracing;

namespace Reqnroll.CucumberMesssages
{
    public class CucumberMessagePublisher : ICucumberMessagePublisher
    {
        private ICucumberMessageBroker broker;
        private IObjectContainer objectContainer;
        private bool initialized = false;

        public CucumberMessagePublisher(ICucumberMessageBroker CucumberMessageBroker, IObjectContainer objectContainer)
        { 
            this.objectContainer = objectContainer;
            broker = CucumberMessageBroker;
        }

        public void HookIntoTestThreadExecutionEventPublisher(ITestThreadExecutionEventPublisher testThreadEventPublisher)
        {
            if (initialized)
            {
                return;
            }
            initialized = true;

            var traceListener = objectContainer.Resolve<ITraceListener>();
            traceListener.WriteTestOutput("HookIntoTestThreadExecutionEventPublisher");

            testThreadEventPublisher.AddHandler<FeatureStartedEvent>(FeatureStartedEventHandler);
            testThreadEventPublisher.AddHandler<FeatureFinishedEvent>(FeatureFinishedEventHandler);
        }

        private void FeatureFinishedEventHandler(FeatureFinishedEvent featureFinishedEvent)
        {
            var featureName = featureFinishedEvent.FeatureContext.FeatureInfo.Title;
            broker.CompleteAsync(featureName);
        }

        private void FeatureStartedEventHandler(FeatureStartedEvent featureStartedEvent)
        {
            var featureName = featureStartedEvent.FeatureContext.FeatureInfo.Title;

            var traceListener = objectContainer.Resolve<ITraceListener>();
            traceListener.WriteTestOutput($"FeatureStartedEventHandler: {featureName}");

           broker.PublishAsync(new ReqnrollCucumberMessage
            {
                CucumberMessageSource = featureName,
                Envelope = Envelope.Create(new TestCaseStarted(1, "1", "2", "0", new Timestamp(1, 1)))
            });
        }
    }
}
