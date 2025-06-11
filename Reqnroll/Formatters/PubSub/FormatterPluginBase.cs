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
    public abstract class FormatterPluginBase : ICucumberMessageSink, IDisposable, IRuntimePlugin, IAsyncExecutionEventListener
    {
        private Task? formatterTask;

        internal readonly BlockingCollection<Envelope> _postedMessages = new();
        private ICucumberMessageBroker _broker;
        private IFormattersConfiguration _configuration;
        private Lazy<ITraceListener> traceListener;
        internal readonly string _pluginName;

        internal ITraceListener? trace => traceListener.Value;
        internal IObjectContainer? _testThreadObjectContainer;

        public FormatterPluginBase(IFormattersConfiguration configuration, ICucumberMessageBroker broker, string pluginName)
        {
            _broker = broker;
            _configuration = configuration;
            traceListener = new Lazy<ITraceListener>(() => _testThreadObjectContainer!.Resolve<ITraceListener>());
            _pluginName = pluginName;
        }

        public void Initialize(RuntimePluginEvents runtimePluginEvents, RuntimePluginParameters runtimePluginParameters, UnitTestProviderConfiguration unitTestProviderConfiguration)
        {
            runtimePluginEvents.CustomizeGlobalDependencies += (sender, args) =>
            {
                var globalObjectContainer = args.ObjectContainer;

                // The act of registering itself with the container serves as a marker to the Broker that it should expect to hear from it via the RegisterSink method
                globalObjectContainer!.RegisterInstanceAs<ICucumberMessageSink>(this, _pluginName, true);
            };

            runtimePluginEvents.CustomizeTestThreadDependencies += (sender, args) =>
            {
                _testThreadObjectContainer = args.ObjectContainer;
                var testThreadExecEventPublisher = _testThreadObjectContainer.Resolve<ITestThreadExecutionEventPublisher>();
                testThreadExecEventPublisher.AddListener(this);
            };
        }


        public async Task OnEventAsync(IExecutionEvent executionEvent)
        {
            switch (executionEvent)
            {
                case TestRunStartedEvent testRunStartedEvent:
                    await LaunchFileSinkAsync();
                    break;
                default:
                    break;
            }
        }

        internal async Task CloseAsync()
        {
            _postedMessages.CompleteAdding();
            if (formatterTask != null) await formatterTask!;
            formatterTask = null;
        }

        internal async Task LaunchFileSinkAsync()
        {
            IFormattersConfiguration config = _configuration;

            if (!config.Enabled)
            {
                return;
            }
            string formatterConfiguration = config.GetFormatterConfigurationByName(_pluginName);

            if (String.IsNullOrEmpty(formatterConfiguration))
                return;

            formatterTask = Task.Factory.StartNew(() => ConsumeAndFormatMessagesBackgroundTask(formatterConfiguration), TaskCreationOptions.LongRunning);

            await _broker.RegisterSinkAsync(this);
        }

        public async Task PublishAsync(Envelope message)
        {
            _postedMessages.Add(message);

            // IF the publisher sends the TestRunFinished message, then we can safely shut down
            if (message.Content() is TestRunFinished)
                await CloseAsync();
            return ;
        }

        internal abstract void ConsumeAndFormatMessagesBackgroundTask(string formatterConfigString);

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
