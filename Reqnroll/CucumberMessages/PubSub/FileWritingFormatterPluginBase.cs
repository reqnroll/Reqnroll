#nullable enable

using Reqnroll.Plugins;
using Reqnroll.UnitTestProvider;
using Reqnroll.Events;
using Reqnroll.Tracing;
using Reqnroll.BoDi;
using System;
using System.Threading.Tasks;
using System.IO;
using System.Linq;
using Reqnroll.CucumberMessages.Configuration;
using Reqnroll.CucumberMessages.PayloadProcessing;
using System.Text;
using System.Collections.Concurrent;
using Io.Cucumber.Messages.Types;

namespace Reqnroll.CucumberMessages.PubSub
{
    public abstract class FileWritingFormatterPluginBase : ICucumberMessageSink, IDisposable, IRuntimePlugin
    {
        private Task? fileWritingTask;

        protected readonly BlockingCollection<Envelope> _postedMessages = new();
        private readonly string _defaultFileType;
        private readonly string _defaultFileName;
        private ICucumberMessagesConfiguration _configuration;
        private Lazy<ITraceListener> traceListener;
        private readonly string _pluginName;

        private ITraceListener? trace => traceListener.Value;
        private IObjectContainer? testThreadObjectContainer;
        private IObjectContainer? globalObjectContainer;

        public FileWritingFormatterPluginBase(ICucumberMessagesConfiguration configuration, string pluginName, string defaultFileType, string defaultFileName)
        {
            _configuration = configuration;
            traceListener = new Lazy<ITraceListener>(() => testThreadObjectContainer!.Resolve<ITraceListener>());
            _pluginName = pluginName;
            _defaultFileType = defaultFileType;
            _defaultFileName = defaultFileName;
        }

        public void Initialize(RuntimePluginEvents runtimePluginEvents, RuntimePluginParameters runtimePluginParameters, UnitTestProviderConfiguration unitTestProviderConfiguration)
        {
            runtimePluginEvents.CustomizeGlobalDependencies += (sender, args) =>
            {
                globalObjectContainer = args.ObjectContainer;
            };

            runtimePluginEvents.CustomizeTestThreadDependencies += (sender, args) =>
            {
                var testThreadExecutionEventPublisher = args.ObjectContainer.Resolve<ITestThreadExecutionEventPublisher>();
                testThreadObjectContainer = args.ObjectContainer;
                testThreadExecutionEventPublisher.AddHandler<TestRunStartedEvent>(LaunchFileSink);
                testThreadExecutionEventPublisher.AddHandler<TestRunFinishedEvent>(Close);
            };
        }

        private void Close(TestRunFinishedEvent @event)
        {
            Dispose(true);
        }

        protected const int TUNING_PARAM_FILE_WRITE_BUFFER_SIZE = 65536;
        private void LaunchFileSink(TestRunStartedEvent testRunStarted)
        {
            ICucumberMessagesConfiguration config = _configuration;

            if (!config.Enabled)
            {
                return;
            }
            string messagesConfiguration = config.FormatterConfiguration(_pluginName);

            if (String.IsNullOrEmpty(messagesConfiguration))
                return;

            string outputFilePath = String.Empty;
            int colonIndex = messagesConfiguration.IndexOf(':');
            if (colonIndex != -1)
            {
                int firstQuoteIndex = messagesConfiguration.IndexOf('"', colonIndex);
                if (firstQuoteIndex != -1)
                {
                    int secondQuoteIndex = messagesConfiguration.IndexOf('"', firstQuoteIndex + 1);
                    if (secondQuoteIndex != -1)
                    {
                        outputFilePath = messagesConfiguration.Substring(firstQuoteIndex + 1, secondQuoteIndex - firstQuoteIndex - 1);
                    }
                }
            }
            if (String.IsNullOrEmpty(outputFilePath))
                outputFilePath = $".\\{_defaultFileName}";

            string baseDirectory = Path.GetDirectoryName(outputFilePath);
            if (!Directory.Exists(baseDirectory))
            {
                Directory.CreateDirectory(baseDirectory);
            }

            string fileName = SanitizeFileName(Path.GetFileName(outputFilePath));
            if (!fileName.EndsWith(_defaultFileType, StringComparison.OrdinalIgnoreCase))
            {
                fileName += _defaultFileType;
            }

            string outputPath = Path.Combine(baseDirectory, fileName);
            fileWritingTask = Task.Factory.StartNew(() => ConsumeAndWriteToFilesBackgroundTask(outputPath), TaskCreationOptions.LongRunning);

            globalObjectContainer!.RegisterInstanceAs<ICucumberMessageSink>(this, _pluginName, true);
        }

        public async Task PublishAsync(Envelope message)
        {
            await Task.Run(() => _postedMessages.Add(message));
        }

        protected abstract void ConsumeAndWriteToFilesBackgroundTask(string outputPath);

        private bool disposedValue = false;

        protected virtual void Dispose(bool disposing)
        {
            if (!disposedValue)
            {
                if (disposing)
                {
                    _postedMessages.CompleteAdding();
                    fileWritingTask?.Wait();
                    fileWritingTask = null;
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

        private static string SanitizeFileName(string input)
        {
            if (string.IsNullOrEmpty(input))
                return string.Empty;

            char[] invalidChars = Path.GetInvalidFileNameChars();
            string sanitized = new string(input.Select(c => invalidChars.Contains(c) ? '_' : c).ToArray());
            sanitized = sanitized.Trim().Trim('.');
            if (string.IsNullOrEmpty(sanitized))
                return "_";
            const int maxLength = 255;
            if (sanitized.Length > maxLength)
                sanitized = sanitized.Substring(0, maxLength);

            return sanitized;
        }
    }
}
