using Io.Cucumber.Messages.Types;
using Reqnroll.Bindings.Reflection;
using Reqnroll.BoDi;
using Reqnroll.Formatters.RuntimeSupport;
using Reqnroll.Plugins;
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
    public CucumberMessageBroker(IFormatterLog formatterLog, IObjectContainer globalObjectContainer)
    {
        //(_publisher as INotifyPublisherReady).Initialized += CucumberMessagePublisher_Initialized;
        _logger = formatterLog;
        _globalcontainer = globalObjectContainer;
        _numberOfSinksExpected = new Lazy<int>(FindContainerRegisteredSinks);
    }

    private int FindContainerRegisteredSinks()
    {
        // Calculate the number of Sinks that should have registered by looking at IRuntimePlugin registrations that also implement the ICucumberMessageSink interface
        var plugins = _globalcontainer.ResolveAll<IRuntimePlugin>();
        var sinks = plugins.Where(p => typeof(ICucumberMessageSink).IsAssignableFrom(p.GetType())).ToList();
        return sinks.Count;
    }

    private void CheckInitializationStatus()
    {
        // If all known sinks have registered 
        // The system is enabled if we have at least one registered sink that is IsEnabled
        if (_numberOfSinksInitialized == _numberOfSinksExpected.Value)
        {
            _logger.WriteMessage($"DEBUG: Formatters - Broker: Initialization complete. Enabled status is: {IsEnabled}");
        }
    }

    public bool IsEnabled
    {
        get
        {
            return (_numberOfSinksInitialized == _numberOfSinksExpected.Value && _numberOfSinksInitialized > 0) ? true : false;
        }
    }

    // This is the number of sinks that we expect to register. This number is determined by the number of sinks that add themselves to the global container during plugin startup.
    private Lazy<int> _numberOfSinksExpected;

    // As sinks are initialized, this number is incremented. When we reach the expected number of sinks, then we know that all have initialized
    // and the Broker can be IsEnabled.
    private int _numberOfSinksInitialized = 0;
    private IFormatterLog _logger;
    private IObjectContainer _globalcontainer;

    // This holds the list of registered and enabled sinks to which messages will be routed.
    // Using a concurrent collection as the sinks may be registering in parallel threads
    private readonly ConcurrentDictionary<string, ICucumberMessageSink> _registeredSinks = new();

    // This method is called by the sinks during plugin Initialize().
    public void SinkInitialized(ICucumberMessageSink formatterSink, bool enabled)
    {
        if (enabled)
            _registeredSinks.TryAdd(formatterSink.Name, formatterSink);

        Interlocked.Increment(ref _numberOfSinksInitialized);
        CheckInitializationStatus();
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
            catch (System.Exception e)
            {
                _logger.WriteMessage($"Formatters Broker: Exception thrown by Formatter Plugin {sink.Name}: {e.Message}");
            }
        }
    }

}