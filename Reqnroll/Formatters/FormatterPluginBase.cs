#nullable enable

using System;
using System.Collections.Concurrent;
using System.Threading.Tasks;
using Io.Cucumber.Messages.Types;
using Reqnroll.BoDi;
using Reqnroll.Formatters.Configuration;
using Reqnroll.Formatters.PayloadProcessing.Cucumber;
using Reqnroll.Formatters.PubSub;
using Reqnroll.Plugins;
using Reqnroll.Tracing;
using Reqnroll.UnitTestProvider;

namespace Reqnroll.Formatters;

public abstract class FormatterPluginBase : ICucumberMessageSink, IDisposable, IRuntimePlugin
{
    private Task? _formatterTask;

    private readonly ICucumberMessageBroker _broker;
    private readonly IFormattersConfigurationProvider _configurationProvider;
    private readonly Lazy<ITraceListener> _traceListener;

    protected readonly BlockingCollection<Envelope> PostedMessages = new();

    private IObjectContainer? _testThreadObjectContainer;
    private bool _isDisposed = false;

    public string PluginName { get; }

    string ICucumberMessageSink.Name => PluginName;
    protected ITraceListener? Trace => _traceListener.Value;

    protected FormatterPluginBase(IFormattersConfigurationProvider configurationProvider, ICucumberMessageBroker broker, string pluginName)
    {
        _broker = broker;
        _configurationProvider = configurationProvider;
        _traceListener = new Lazy<ITraceListener>(() => _testThreadObjectContainer!.Resolve<ITraceListener>());
        PluginName = pluginName;
    }

    public void Initialize(RuntimePluginEvents runtimePluginEvents, RuntimePluginParameters runtimePluginParameters, UnitTestProviderConfiguration unitTestProviderConfiguration)
    {
        _broker.RegisterSink(this);

        LaunchSinkAsync();

        runtimePluginEvents.CustomizeTestThreadDependencies += (_, args) =>
        {
            _testThreadObjectContainer = args.ObjectContainer;
        };
    }

    internal async Task CloseAsync()
    {
        PostedMessages.CompleteAdding();
        if (_formatterTask != null) await _formatterTask;
        _formatterTask = null;
    }

    internal void LaunchSinkAsync()
    {
        bool IsFormatterEnabled(out string configuration)
        {
            configuration = null!;
            return _configurationProvider.Enabled && !string.IsNullOrEmpty(configuration = _configurationProvider.GetFormatterConfigurationByName(PluginName));
        }

        if (!IsFormatterEnabled(out var formatterConfiguration))
        {
            _broker.SinkInitialized(this, enabled: false);
            return;
        }

        _formatterTask = Task.Factory.StartNew(() => ConsumeAndFormatMessagesBackgroundTask(formatterConfiguration, ReportInitialized), TaskCreationOptions.LongRunning);
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
            await CloseAsync();
    }

    protected abstract void ConsumeAndFormatMessagesBackgroundTask(string formatterConfigString, Action<bool> onAfterInitialization);

    public void Dispose()
    {
        if (!_isDisposed)
        {
            PostedMessages.Dispose();
            _isDisposed = true;
        }
    }
}