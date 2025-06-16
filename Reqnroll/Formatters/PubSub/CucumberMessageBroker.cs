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
        Task RegisterEnabledSinkAsync(ICucumberMessageSink formatterSink);
        Task RegisterDisabledSinkAsync(ICucumberMessageSink formatterSink);

        public event AsyncEventHandler<BrokerReadyEventArgs> BrokerReadyEvent;
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
        private Lazy<int> NumberOfSinksExpected;

        // As sinks register, this number is incremented. When we reach the expected number of sinks, then we know that all have registered
        // and the Broker can be Enabled.
        private int NumberOfSinksRegistered = 0;

        // This holds the list of regiestered and Enabled sinks to which Messages will be routed.
        // Using a Concurrent collection as the sinks may be registering in parallel threads
        private ConcurrentDictionary<string, ICucumberMessageSink> _registeredSinks = new();

        // This event gets fired when all Sinks have registered and indicates to the Publisher that it can start Publishing messages.
        public event AsyncEventHandler<BrokerReadyEventArgs> BrokerReadyEvent;

        public CucumberMessageBroker(IObjectContainer objectContainer)
        {
            _objectContainer = objectContainer;
            Enabled = false;

            // We want to know how many Sinks exist in the container, but we can't enumerate them now (during this constructor)
            NumberOfSinksExpected =  new Lazy<int>( () => _objectContainer.ResolveAll<ICucumberMessageSink>().Count());
        }

        // This method is called by the sinks during TestRunStarted event handling. By then, all Sinks will have registered themselves in the object container
        // (which happened during Plugin Initialize() )
        public async Task RegisterEnabledSinkAsync(ICucumberMessageSink formatterSink)
        {
            _registeredSinks.TryAdd(formatterSink.Name, formatterSink);

            await RegisterSink();
        }

        public async Task RegisterDisabledSinkAsync(ICucumberMessageSink formatterSink)
        {
            await RegisterSink();
        }

        private async Task RegisterSink()
        {
            Interlocked.Increment(ref NumberOfSinksRegistered);

            // If all known sinks have registered then we can inform the Publisher
            // The system is enabled if we have at least one registered sink that is Enabled
            if (NumberOfSinksRegistered == NumberOfSinksExpected.Value)
            {
                Enabled = _registeredSinks.Values.Count > 0;
                await RaiseBrokerReadyEvent();
            }
        }

        private async Task RaiseBrokerReadyEvent()
        {
            if (BrokerReadyEvent is null)
                return;
            foreach (var subscriber in BrokerReadyEvent.GetInvocationList())
            {
                var asyncSubscriber = (AsyncEventHandler<BrokerReadyEventArgs>)subscriber;
                await asyncSubscriber.Invoke(this, new BrokerReadyEventArgs());
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
