#nullable enable

using Reqnroll.Plugins;
using Reqnroll.UnitTestProvider;
using Reqnroll.Events;
using Reqnroll.Tracing;
using Reqnroll.BoDi;
using System;
using System.Threading.Tasks;
using Reqnroll.CucumberMessages.Configuration;
using System.Collections.Concurrent;
using Io.Cucumber.Messages.Types;

namespace Reqnroll.CucumberMessages.PubSub
{
    public abstract class FormatterPluginBase : ICucumberMessageSink, IDisposable, IRuntimePlugin
    {
        private Task? formatterTask;

        internal readonly BlockingCollection<Envelope> _postedMessages = new();
        private ICucumberMessagesConfiguration _configuration;
        private Lazy<ITraceListener> traceListener;
        internal readonly string _pluginName;

        internal ITraceListener? trace => traceListener.Value;
        internal IObjectContainer? _testThreadObjectContainer;
        internal IObjectContainer? _globalObjectContainer;

        public FormatterPluginBase(ICucumberMessagesConfiguration configuration, string pluginName)
        {
            _configuration = configuration;
            traceListener = new Lazy<ITraceListener>(() => _testThreadObjectContainer!.Resolve<ITraceListener>());
            _pluginName = pluginName;
        }

        public void Initialize(RuntimePluginEvents runtimePluginEvents, RuntimePluginParameters runtimePluginParameters, UnitTestProviderConfiguration unitTestProviderConfiguration)
        {
            runtimePluginEvents.CustomizeGlobalDependencies += (sender, args) =>
            {
                _globalObjectContainer = args.ObjectContainer;
            };

            runtimePluginEvents.CustomizeTestThreadDependencies += (sender, args) =>
            {
                var testThreadExecutionEventPublisher = args.ObjectContainer.Resolve<ITestThreadExecutionEventPublisher>();
                _testThreadObjectContainer = args.ObjectContainer;

                testThreadExecutionEventPublisher.AddHandler<TestRunStartedEvent>(LaunchFileSink);
                testThreadExecutionEventPublisher.AddHandler<TestRunFinishedEvent>(Close);
            };
        }

        private void Close(TestRunFinishedEvent @event)
        {
            Dispose(true);
        }

        internal void LaunchFileSink(TestRunStartedEvent testRunStarted)
        {
            ICucumberMessagesConfiguration config = _configuration;

            if (!config.Enabled)
            {
                return;
            }
            string formatterConfiguration = config.GetFormatterConfigurationByName(_pluginName);

            if (String.IsNullOrEmpty(formatterConfiguration))
                return;

            formatterTask = Task.Factory.StartNew(() => ConsumeAndFormatMessagesBackgroundTask(formatterConfiguration), TaskCreationOptions.LongRunning);

            _globalObjectContainer!.RegisterInstanceAs<ICucumberMessageSink>(this, _pluginName, true);
        }

        public Task PublishAsync(Envelope message)
        {
            _postedMessages.Add(message);
            return Task.CompletedTask;
        }

        internal abstract void ConsumeAndFormatMessagesBackgroundTask(string formatterConfigString);

        private bool disposedValue = false;

        internal virtual void Dispose(bool disposing)
        {
            if (!disposedValue)
            {
                if (disposing)
                {
                    _postedMessages.CompleteAdding();
                    formatterTask?.Wait();
                    formatterTask = null;
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
