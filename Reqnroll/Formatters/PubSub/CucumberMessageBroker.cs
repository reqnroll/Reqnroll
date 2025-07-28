using Io.Cucumber.Messages.Types;
using Reqnroll.Formatters.RuntimeSupport;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;

namespace Reqnroll.Formatters.PubSub;

/// <summary>
/// Cucumber Message implementation is a simple Pub/Sub implementation.
/// This broker mediates between the (singleton) CucumberMessagePublisher and (one or more) CucumberMessageSinks.
/// The pub/sub mechanism is considered to be turned "OFF" if no sinks are registered or if none of them are configured.
/// </summary>
public class CucumberMessageBroker : ICucumberMessageBroker
{
    // This is the list of sinks registered in the container. Not all may be enabled/configured.
    private readonly List<ICucumberMessageFormatter> _registeredFormatters = new();

    // As sinks are initialized, this number is incremented. When we reach the expected number of sinks, then we know that all have initialized
    // and the Broker can be IsEnabled.
    private int _numberOfSinksInitialized = 0;
    private readonly IFormatterLog _logger;

    // This holds the list of registered and enabled sinks to which messages will be routed.
    // Using a concurrent collection as the sinks may be registering in parallel threads
    private readonly ConcurrentDictionary<string, ICucumberMessageFormatter> _activeSinks = new();


    public CucumberMessageBroker(IFormatterLog formatterLog, IDictionary<string, ICucumberMessageFormatter> containerRegisteredFormatters)
    {
        _logger = formatterLog;
        _registeredFormatters.AddRange(containerRegisteredFormatters.Values);
    }

    public void Initialize()
    {
        foreach (var formatter in _registeredFormatters)
        {
            formatter.LaunchSink(this);
        }
    }

    // This method is called by the sinks during sink LaunchSink().
    public void SinkInitialized(ICucumberMessageFormatter formatterSink, bool enabled)
    {
        if (enabled)
            _activeSinks.TryAdd(formatterSink.Name, formatterSink);

        Interlocked.Increment(ref _numberOfSinksInitialized);
        CheckInitializationStatus();
    }

    public bool IsEnabled => HaveAllSinksRegistered() && _activeSinks.Count > 0;

    private void CheckInitializationStatus()
    {
        // If all known sinks have registered 
        // The system is enabled if we have at least one registered sink that is IsEnabled
        if (HaveAllSinksRegistered())
        {
            _logger.WriteMessage($"DEBUG: Formatters - Broker: Initialization complete. Enabled status is: {IsEnabled}");
        }
    }

    private bool HaveAllSinksRegistered()
    {
        return _numberOfSinksInitialized == _registeredFormatters.Count;
    }

    public async Task PublishAsync(Envelope message)
    {
        foreach (var sink in _activeSinks.Values)
        {
            // Will catch and swallow any exceptions thrown by sinks so that all get a chance to process each message.
            try
            {
                await sink.PublishAsync(message);
            }
            catch (System.Exception e)
            {
                _logger.WriteMessage($"Formatters Broker: Exception thrown by Formatter Plugin {sink.Name}: {e.Message}");
            }
        }
    }

}