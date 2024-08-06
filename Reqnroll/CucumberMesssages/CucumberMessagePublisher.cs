using Reqnroll.BoDi;
using Reqnroll.Events;
using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;
using Io.Cucumber.Messages;
using Io.Cucumber.Messages.Types;

namespace Reqnroll.CucumberMesssages
{
    public class CucumberMessagePublisher
    {
        private ICucumberMessageBroker broker;

        public CucumberMessagePublisher(ITestThreadExecutionEventPublisher eventSource, ICucumberMessageBroker CucumberMessageBroker)
        {
            broker = CucumberMessageBroker;
            eventSource.AddHandler<FeatureStartedEvent>(FeatureStartedEventHandler);
            eventSource.AddHandler<FeatureFinishedEvent>(FeatureFinishedEventHandler);
        }

        private void FeatureFinishedEventHandler(FeatureFinishedEvent featureFinishedEvent)
        {
            var featureName = featureFinishedEvent.FeatureContext.FeatureInfo.FolderPath;
            Task.Run(() => broker.CompleteAsync(featureName));
        }

        private void FeatureStartedEventHandler(FeatureStartedEvent featureStartedEvent)
        {
            var featureName = featureStartedEvent.FeatureContext.FeatureInfo.FolderPath;
            Task.Run(() => broker.PublishAsync(new ReqnrollCucumberMessage
            {
                CucumberMessageSource = featureName,
                Envelope = Envelope.Create(new TestCaseStarted(1, "1", "2", "0", new Timestamp(1, 1)))
            }));
        }
    }
}
