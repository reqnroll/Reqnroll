using Io.Cucumber.Messages.Types;
using Reqnroll.BoDi;
using Reqnroll.Formatters.PayloadProcessing.Cucumber;
using Reqnroll.Plugins;
using Reqnroll.Tracing;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Security.Authentication.ExtendedProtection;
using System.Text;
using System.Threading;
using System.Threading.Tasks;


namespace Reqnroll.Formatters.PubSub
{

    public interface ICucumberMessageBroker
    {
        bool Enabled { get; }
        Task PublishAsync(Envelope featureMessages);

        void RegisterSink(ICucumberMessageSink sink);
        void SinkInitialized(ICucumberMessageSink formatterSink, bool enabled);

        public event EventHandler<BrokerReadyEventArgs> BrokerReadyEvent;
    }

    public class BrokerReadyEventArgs
    {
    }

    /// <summary>
    /// Cucumber Message implementation is a simple Pub/Sub implementation.
    /// This broker mediates between the (singleton) CucumberMessagePublisher and (one or more) CucumberMessageSinks
    /// 
    /// The pub/sub mechanism is considered to be turned "OFF" if no sinks are registered
    /// </summary>
    public class CucumberMessageBroker : ICucumberMessageBroker
    {
        private IObjectContainer _objectContainer;

        public bool Enabled { get; private set; }

        // This is the number of sinks that we expect to register. This number is determined by the number of sinks that add themselves to the global container during plugin startup.
        private int NumberOfSinksExpected;

        // As sinks are initialized, this number is incremented. When we reach the expected number of sinks, then we know that all have initialized
        // and the Broker can be Enabled.
        private int NumberOfSinksInitialized = 0;

        // This holds the list of regiestered and Enabled sinks to which Messages will be routed.
        // Using a Concurrent collection as the sinks may be registering in parallel threads
        private ConcurrentDictionary<string, ICucumberMessageSink> _registeredSinks = new();

        // This event gets fired when all Sinks have registered and indicates to the Publisher that it can start Publishing messages.
        public event EventHandler<BrokerReadyEventArgs> BrokerReadyEvent;

        public CucumberMessageBroker(IObjectContainer objectContainer)
        {
            _objectContainer = objectContainer;
            Enabled = false;
        }

        // This method is called by the sinks during their Plugin initialization. This tells the broker how many plugin Sinks to expect.
        public void RegisterSink(ICucumberMessageSink sink)
        {
            Interlocked.Increment(ref NumberOfSinksExpected);
        }

        // This method is called by the sinks during TestRunStarted event handling. By then, all Sinks will have registered themselves in the object container
        // (which happened during Plugin Initialize() )
        public void SinkInitialized(ICucumberMessageSink formatterSink, bool enabled)
        {
            if (enabled) 
                _registeredSinks.TryAdd(formatterSink.Name, formatterSink);

            Interlocked.Increment(ref NumberOfSinksInitialized);

            // If all known sinks have registered then we can inform the Publisher
            // The system is enabled if we have at least one registered sink that is Enabled
            if (NumberOfSinksInitialized == NumberOfSinksExpected)
            {
                Enabled = _registeredSinks.Values.Count > 0;
                RaiseBrokerReadyEvent();
            }
        }

        private void RaiseBrokerReadyEvent()
        {
            if (BrokerReadyEvent is null)
                return;
            foreach (var subscriber in BrokerReadyEvent.GetInvocationList())
            {
                var Subscriber = (EventHandler<BrokerReadyEventArgs>)subscriber;
                Subscriber.Invoke(this, new BrokerReadyEventArgs());
            }
        }

        public async Task PublishAsync(Envelope message)
        {
            foreach (var sink in _registeredSinks.Values)
            {
                // Will catch and swallow any exceptions thrown by sinks so that all get a chance to process each message
                try
                {
                    await sink.PublishAsync(message);
                }
                catch {
                // TODO: What should be done here? Log the exception to the tool output?
                }
            }
        }

    }
}
