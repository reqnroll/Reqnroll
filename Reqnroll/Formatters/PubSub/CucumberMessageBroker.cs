using Io.Cucumber.Messages.Types;
using Reqnroll.BoDi;
using Reqnroll.Formatters.PayloadProcessing.Cucumber;
using Reqnroll.Plugins;
using Reqnroll.Tracing;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Security.Authentication.ExtendedProtection;
using System.Text;
using System.Threading.Tasks;


namespace Reqnroll.Formatters.PubSub
{

    public interface ICucumberMessageBroker
    {
        bool Enabled { get; }
        Task PublishAsync(Envelope featureMessages);
        Task RegisterSinkAsync(ICucumberMessageSink formatterPluginBase);

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

        private Lazy<int> ResolvedSinks;
        private IList<ICucumberMessageSink> _registeredSinks = new List<ICucumberMessageSink>();

        public event AsyncEventHandler<BrokerReadyEventArgs> BrokerReadyEvent;

        public CucumberMessageBroker(IObjectContainer objectContainer)
        {
            _objectContainer = objectContainer;
            Enabled = false;

            // We want to know how many Sinks exist in the container, but we can't enumerate them now (during this constructor)
            ResolvedSinks =  new Lazy<int>( () => _objectContainer.ResolveAll<ICucumberMessageSink>().Count());
        }

        // This method is called by the sinks during TestRunStarted event handling. By then, all Sinks will have registered themselves in the object container
        // (which happened during Plugin Initialize() )
        public async Task RegisterSinkAsync(ICucumberMessageSink formatterPluginBase)
        {
            _registeredSinks.Add(formatterPluginBase);

            // If all known sinks have registered then we can consider the broker Enabled
            if (_registeredSinks.Count == ResolvedSinks.Value)
            {
                Enabled = true;
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
            foreach (var sink in _registeredSinks)
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
