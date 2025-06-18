using Io.Cucumber.Messages.Types;
using System;
using System.Collections.Concurrent;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace Reqnroll.Formatters.PubSub;

/// <summary>
/// Cucumber Message implementation is a simple Pub/Sub implementation.
/// This broker mediates between the (singleton) CucumberMessagePublisher and (one or more) CucumberMessageSinks.
/// The pub/sub mechanism is considered to be turned "OFF" if no sinks are registered.
/// </summary>
public class CucumberMessageBroker : ICucumberMessageBroker
{
    public bool Enabled { get; private set; }

    // This is the number of sinks that we expect to register. This number is determined by the number of sinks that add themselves to the global container during plugin startup.
    private int _numberOfSinksExpected;

    // As sinks are initialized, this number is incremented. When we reach the expected number of sinks, then we know that all have initialized
    // and the Broker can be Enabled.
    private int _numberOfSinksInitialized = 0;

    // This holds the list of registered and enabled sinks to which messages will be routed.
    // Using a concurrent collection as the sinks may be registering in parallel threads
    private readonly ConcurrentDictionary<string, ICucumberMessageSink> _registeredSinks = new();

    // This event gets fired when all Sinks have registered and indicates to the Publisher that it can start Publishing messages.
    public event EventHandler<BrokerReadyEventArgs> BrokerReadyEvent;

    public CucumberMessageBroker()
    {
        Enabled = false;
    }

    // This method is called by the sinks during their plugin initialization. This tells the broker how many plugin sinks to expect.
    public void RegisterSink(ICucumberMessageSink sink)
    {
        Interlocked.Increment(ref _numberOfSinksExpected);
    }

    // This method is called by the sinks during TestRunStarted event handling. By then, all sinks will have registered themselves in the object container,
    // which happened during plugin Initialize().
    public void SinkInitialized(ICucumberMessageSink formatterSink, bool enabled)
    {
        if (enabled) 
            _registeredSinks.TryAdd(formatterSink.Name, formatterSink);

        Interlocked.Increment(ref _numberOfSinksInitialized);

        // If all known sinks have registered then we can inform the Publisher
        // The system is enabled if we have at least one registered sink that is Enabled
        if (_numberOfSinksInitialized == _numberOfSinksExpected)
        {
            Enabled = _registeredSinks.Values.Count > 0;
            RaiseBrokerReadyEvent();
        }
    }

    private void RaiseBrokerReadyEvent()
    {
        if (BrokerReadyEvent is null)
            return;

        foreach (var subscriber in BrokerReadyEvent.GetInvocationList().OfType<EventHandler<BrokerReadyEventArgs>>())
        {
            subscriber.Invoke(this, new BrokerReadyEventArgs());
        }
    }

    public async Task PublishAsync(Envelope message)
    {
        foreach (var sink in _registeredSinks.Values)
        {
            // Will catch and swallow any exceptions thrown by sinks so that all get a chance to process each message.
            try
            {
                await sink.PublishAsync(message);
            }
            catch 
            {
                // TODO: What should be done here? Log the exception to the tool output?
            }
        }
    }

}