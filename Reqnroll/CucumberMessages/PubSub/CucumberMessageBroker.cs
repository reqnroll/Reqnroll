using Reqnroll.BoDi;
using Reqnroll.CucumberMessages.PayloadProcessing.Cucumber;
using Reqnroll.Tracing;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Security.Authentication.ExtendedProtection;
using System.Text;
using System.Threading.Tasks;


namespace Reqnroll.CucumberMessages.PubSub
{

    public interface ICucumberMessageBroker
    {
        bool Enabled { get; }
        void Publish(ReqnrollCucumberMessage featureMessages);
    }

    public class CucumberMessageBroker : ICucumberMessageBroker
    {
        private IObjectContainer _objectContainer;

        public bool Enabled => RegisteredSinks.Value.ToList().Count > 0;

        private Lazy<IEnumerable<ICucumberMessageSink>> RegisteredSinks;

        public CucumberMessageBroker(IObjectContainer objectContainer)
        {
            _objectContainer = objectContainer;
            RegisteredSinks = new Lazy<IEnumerable<ICucumberMessageSink>>(() => _objectContainer.ResolveAll<ICucumberMessageSink>());
        }
        public void Publish(ReqnrollCucumberMessage message)
        {
            var _traceListener = _objectContainer.Resolve<ITraceListener>();

            foreach (var sink in RegisteredSinks.Value)
            {
                _traceListener.WriteTestOutput($"Broker publishing {message.CucumberMessageSource}: {message.Envelope.Content()}");

                sink.Publish(message);
            }
        }

    }
}
