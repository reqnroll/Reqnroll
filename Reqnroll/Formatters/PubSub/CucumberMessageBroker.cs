using Io.Cucumber.Messages.Types;
using Reqnroll.Analytics;
using Reqnroll.Formatters.RuntimeSupport;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;

namespace Reqnroll.Formatters.PubSub;

/// <summary>
/// Cucumber Message implementation is a simple Pub/Sub implementation.
/// This broker mediates between the (singleton) CucumberMessagePublisher and (one or more) Formatters.
/// The pub/sub mechanism is considered to be turned "OFF" if no sinks are registered or if none of them are configured.
/// </summary>
public class CucumberMessageBroker : ICucumberMessageBroker
{
    // This is the list of sinks registered in the container. Not all may be enabled/configured.
    private readonly List<ICucumberMessageFormatter> _registeredFormatters = new();

    // As sinks are initialized, this number is incremented. When we reach the expected number of sinks, then we know that all have initialized
    // and the Broker can be IsEnabled.
    private int _numberOfFormattersInitialized = 0;
    private readonly IFormatterLog _logger;
    private readonly IAnalyticsRuntimeTelemetryService _telemetryService;

    // This holds the list of registered and enabled sinks to which messages will be routed.
    // Using a concurrent collection as the sinks may be registering in parallel threads
    private readonly ConcurrentDictionary<string, ICucumberMessageFormatter> _activeFormatters = new();


    public CucumberMessageBroker(IFormatterLog formatterLog, IDictionary<string, ICucumberMessageFormatter> containerRegisteredFormatters, IAnalyticsRuntimeTelemetryService telemetryService)
    {
        _logger = formatterLog;
        _telemetryService = telemetryService;
        _registeredFormatters.AddRange(containerRegisteredFormatters.Values);
    }

    public void Initialize()
    {
        foreach (var formatter in _registeredFormatters)
        {
            formatter.LaunchFormatter(this);
        }
    }

    // This method is called by the sinks during formatter LaunchFormatter().
    public void FormatterInitialized(ICucumberMessageFormatter formatter, bool enabled)
    {
        if (enabled)
            _activeFormatters.TryAdd(formatter.Name, formatter);

        Interlocked.Increment(ref _numberOfFormattersInitialized);
        CheckInitializationStatus();
    }

    public bool IsEnabled => HaveAllFormattersRegisteredAndInitialized() && _activeFormatters.Count > 0;

    private void CheckInitializationStatus()
    {
        // If all known formatters have registered 
        // The system is enabled if we have at least one registered formatter that is IsEnabled
        if (HaveAllFormattersRegisteredAndInitialized())
        {
            SendTelemetryEvents();
            _logger.WriteMessage($"DEBUG: Formatters - Broker: Initialization complete. Enabled status is: {IsEnabled}");
        }
    }

    private void SendTelemetryEvents()
    {
        if (_telemetryService == null) return;

        foreach (var formatter in _activeFormatters)
        {
            _telemetryService.SendFeatureUseEvent(
                ReqnrollFeatureUseEvent.FeatureNames.Formatter,
                properties: new Dictionary<string, string>
                {
                    { ReqnrollFeatureUseEvent.FormatterNameProperty, formatter.Key }
                }
            );
        }
    }

    private bool HaveAllFormattersRegisteredAndInitialized()
    {
        return _numberOfFormattersInitialized == _registeredFormatters.Count;
    }

    public async Task PublishAsync(Envelope message)
    {
        foreach (var formatter in _activeFormatters.Values)
        {
            // Will catch and swallow any exceptions thrown by sinks so that all get a chance to process each message.
            try
            {
                await formatter.PublishAsync(message);
            }
            catch (System.Exception e)
            {
                _logger.WriteMessage($"Formatters Broker: Exception thrown by Formatter Plugin {formatter.Name}: {e.Message}");
            }
        }
    }

}