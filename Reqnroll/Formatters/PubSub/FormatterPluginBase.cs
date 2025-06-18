#nullable enable

using Reqnroll.Plugins;
using Reqnroll.UnitTestProvider;
using Reqnroll.Events;
using Reqnroll.Tracing;
using Reqnroll.BoDi;
using System;
using System.Threading.Tasks;
using System.Collections.Concurrent;
using Io.Cucumber.Messages.Types;
using Reqnroll.Formatters.PayloadProcessing.Cucumber;
using Reqnroll.Formatters.Configuration;

namespace Reqnroll.Formatters.PubSub
{
    public abstract class FormatterPluginBase : ICucumberMessageSink, IDisposable, IRuntimePlugin
    {
        private Task? formatterTask;

        internal readonly BlockingCollection<Envelope> _postedMessages = new();
        private ICucumberMessageBroker _broker;
        private IFormattersConfigurationProvider _configurationProvider;
        private Lazy<ITraceListener> traceListener;
        internal readonly string _pluginName;

        internal ITraceListener? trace => traceListener.Value;
        internal IObjectContainer? _testThreadObjectContainer;

        public string Name { get => _pluginName; }


        public FormatterPluginBase(IFormattersConfigurationProvider configurationProvider, ICucumberMessageBroker broker, string pluginName)
        {
            _broker = broker;
            _configurationProvider = configurationProvider;
            traceListener = new Lazy<ITraceListener>(() => _testThreadObjectContainer!.Resolve<ITraceListener>());
            _pluginName = pluginName;
        }

        public void Initialize(RuntimePluginEvents runtimePluginEvents, RuntimePluginParameters runtimePluginParameters, UnitTestProviderConfiguration unitTestProviderConfiguration)
        {
            _broker.RegisterSink(this);

            LaunchSinkAsync();

            runtimePluginEvents.CustomizeTestThreadDependencies += (sender, args) =>
            {
                _testThreadObjectContainer = args.ObjectContainer;
            };
        }

        internal async Task CloseAsync()
        {
            _postedMessages.CompleteAdding();
            if (formatterTask != null) await formatterTask!;
            formatterTask = null;
        }

        internal void LaunchSinkAsync()
        {
            IFormattersConfigurationProvider config = _configurationProvider;

            if (!config.Enabled)
            {
                _broker.SinkInitialized(this, enabled: false);
                return;
            }
            string formatterConfiguration = config.GetFormatterConfigurationByName(_pluginName);

            if (String.IsNullOrEmpty(formatterConfiguration))
            {
                _broker.SinkInitialized(this, enabled: false);
                return;
            }

            formatterTask = Task.Factory.StartNew(() => ConsumeAndFormatMessagesBackgroundTask(formatterConfiguration, ReportInitialized), TaskCreationOptions.LongRunning);

        }

        private void ReportInitialized(bool status)
        {
            _broker.SinkInitialized(this, enabled: status);
        }

        public async Task PublishAsync(Envelope message)
        {
            _postedMessages.Add(message);

            // IF the publisher sends the TestRunFinished message, then we can safely shut down
            if (message.Content() is TestRunFinished)
                await CloseAsync();
            return;
        }

        internal abstract void ConsumeAndFormatMessagesBackgroundTask(string formatterConfigString, Action<bool> onAfterInitialization);

        private bool disposedValue = false;

        internal virtual void Dispose(bool disposing)
        {
            if (!disposedValue)
            {
                if (disposing)
                {
                    _postedMessages.Dispose();
                }
                disposedValue = true;
            }
        }

        public void Dispose()
        {
            Dispose(disposing: true);
            GC.SuppressFinalize(this);
        }
    }
}
