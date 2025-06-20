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

public abstract class FormatterPluginBase : ICucumberMessageSink, IDisposable, IRuntimePlugin
{
    private Task? _formatterTask;

    private readonly ICucumberMessageBroker _broker;
    private readonly IFormattersConfigurationProvider _configurationProvider;
    private readonly IFormatterLog _logger;
    protected readonly BlockingCollection<Envelope> PostedMessages = new();

    private bool _isDisposed = false;

    public IFormatterLog Logger { get => _logger; }
    public string PluginName { get; }

    string ICucumberMessageSink.Name => PluginName;

    protected FormatterPluginBase(IFormattersConfigurationProvider configurationProvider, ICucumberMessageBroker broker, IFormatterLog logger, string pluginName)
    {
        _broker = broker;
        _configurationProvider = configurationProvider;
        _logger = logger;
        PluginName = pluginName;
    }

    public void Initialize(RuntimePluginEvents runtimePluginEvents, RuntimePluginParameters runtimePluginParameters, UnitTestProviderConfiguration unitTestProviderConfiguration)
    {
        _broker.RegisterSink(this);

        LaunchSinkAsync();
    }

    internal async Task CloseAsync()
    {
        PostedMessages.CompleteAdding();
        if (_formatterTask != null) await _formatterTask.ConfigureAwait(false);
        _formatterTask = null;
    }

    internal void LaunchSinkAsync()
    {
        bool IsFormatterEnabled(out Dictionary<string, string> configuration)
        {
            configuration = null!;
            if (!_configurationProvider.Enabled)
                return false;
            configuration = (Dictionary<string, string>)_configurationProvider.GetFormatterConfigurationByName(PluginName);

            return configuration != null && configuration.Count > 0;
        }

        if (!IsFormatterEnabled(out var formatterConfiguration))
        {
            _broker.SinkInitialized(this, enabled: false);
            return;
        }

        _formatterTask = Task.Run( () =>  ConsumeAndFormatMessagesBackgroundTask(formatterConfiguration, ReportInitialized));
    }

    private void ReportInitialized(bool status)
    {
        _broker.SinkInitialized(this, enabled: status);
    }

    public async Task PublishAsync(Envelope message)
    {
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
            PostedMessages.Dispose();
            _isDisposed = true;
        }
    }
}