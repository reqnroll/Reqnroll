#nullable enable

using System;
using System.Collections;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Threading.Tasks;
using Io.Cucumber.Messages.Types;
using Reqnroll.Configuration.JsonConfig;
using Reqnroll.Formatters.Configuration;
using Reqnroll.Formatters.PayloadProcessing.Cucumber;
using Reqnroll.Formatters.PubSub;
using Reqnroll.Formatters.RuntimeSupport;
using Reqnroll.Plugins;
using Reqnroll.UnitTestProvider;

namespace Reqnroll.Formatters;

public abstract class FormatterPluginBase : ICucumberMessageSink, IRuntimePlugin, IDisposable
{
    private Task? _formatterTask;

    private readonly ICucumberMessageBroker _broker;
    private readonly IFormattersConfigurationProvider _configurationProvider;
    private readonly IFormatterLog _logger;
    protected readonly BlockingCollection<Envelope> PostedMessages = new();

    private bool _isDisposed = false;

    public IFormatterLog Logger { get => _logger; }
    private string _pluginName;

    public string Name => _pluginName;

    protected FormatterPluginBase(IFormattersConfigurationProvider configurationProvider, ICucumberMessageBroker broker, IFormatterLog logger, string pluginName)
    {
        _broker = broker;
        _configurationProvider = configurationProvider;
        _logger = logger;
        _pluginName = pluginName;
    }

    public void Initialize(RuntimePluginEvents runtimePluginEvents, RuntimePluginParameters runtimePluginParameters, UnitTestProviderConfiguration unitTestProviderConfiguration)
    {
        _broker.RegisterSink(this);

        LaunchSink();
    }

    internal async Task CloseAsync()
    {
        PostedMessages.CompleteAdding();
        if (_formatterTask != null) await _formatterTask.ConfigureAwait(false);
        _formatterTask = null;
    }

    internal void LaunchSink()
    {
        bool IsFormatterEnabled(out Dictionary<string, string> configuration)
        {
            configuration = null!;
            if (!_configurationProvider.Enabled)
                return false;
            configuration = (Dictionary<string, string>)_configurationProvider.GetFormatterConfigurationByName(_pluginName);

            return configuration != null && configuration.Count > 0;
        }

        if (!IsFormatterEnabled(out var formatterConfiguration))
        {
            _broker.SinkInitialized(this, enabled: false);
            return;
        }

        _formatterTask = Task.Run(() => ConsumeAndFormatMessagesBackgroundTask(formatterConfiguration, ReportInitialized));
    }

    private void ReportInitialized(bool status)
    {
        _broker.SinkInitialized(this, enabled: status);
    }

    public async Task PublishAsync(Envelope message)
    {
        if (_isDisposed || PostedMessages.IsAddingCompleted)
        {
            Logger.WriteMessage($"Cannot add message {message.Content().GetType().Name} to formatter {Name} - formatter is closed and not able to accept additional messages.");
            return;
        }

        PostedMessages.Add(message);

        // If the publisher sends the TestRunFinished message, then we can safely shut down.
        if (message.Content() is TestRunFinished)
            await CloseAsync().ConfigureAwait(false);
    }

    protected abstract Task ConsumeAndFormatMessagesBackgroundTask(IDictionary<string, string> formatterConfigString, Action<bool> onAfterInitialization);

    public void Dispose()
    {
        if (!_isDisposed)
        {
            try
            {
                if (!PostedMessages.IsAddingCompleted)
                    CloseAsync().GetAwaiter().GetResult();
            }
            finally
            {
                PostedMessages.Dispose();
                _isDisposed = true;
                GC.SuppressFinalize(this);
            }
        }
    }
}