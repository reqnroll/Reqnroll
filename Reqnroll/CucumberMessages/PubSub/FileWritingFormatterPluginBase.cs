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
using Reqnroll.Utils;

namespace Reqnroll.CucumberMessages.PubSub
{
    public abstract class FileWritingFormatterPluginBase : ICucumberMessageSink, IDisposable, IRuntimePlugin
    {
        private Task? fileWritingTask;

        internal readonly BlockingCollection<Envelope> _postedMessages = new();
        private readonly string _defaultFileType;
        private readonly string _defaultFileName;
        private readonly IFileSystem _fileSystem;
        private ICucumberMessagesConfiguration _configuration;
        private Lazy<ITraceListener> traceListener;
        private readonly string _pluginName;

        private ITraceListener? trace => traceListener.Value;
        private IObjectContainer? _testThreadObjectContainer;
        private IObjectContainer? _globalObjectContainer;

        public FileWritingFormatterPluginBase(ICucumberMessagesConfiguration configuration, string pluginName, string defaultFileType, string defaultFileName, IFileSystem fileSystem)
        {
            _configuration = configuration;
            traceListener = new Lazy<ITraceListener>(() => _testThreadObjectContainer!.Resolve<ITraceListener>());
            _pluginName = pluginName;
            _defaultFileType = defaultFileType;
            _defaultFileName = defaultFileName;
            _fileSystem = fileSystem;
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

        protected const int TUNING_PARAM_FILE_WRITE_BUFFER_SIZE = 65536;
        internal void LaunchFileSink(TestRunStartedEvent testRunStarted)
        {
            ICucumberMessagesConfiguration config = _configuration;

            if (!config.Enabled)
            {
                return;
            }
            string messagesConfiguration = config.GetFormatterConfigurationByName(_pluginName);

            if (String.IsNullOrEmpty(messagesConfiguration))
                return;

            string outputFilePath = ParseConfigurationString(messagesConfiguration, _pluginName);

            if (String.IsNullOrEmpty(outputFilePath))
                outputFilePath = $".{Path.DirectorySeparatorChar}{_defaultFileName}";

            string baseDirectory = Path.GetDirectoryName(outputFilePath);
            var validFile = FileFilter.GetValidFiles([outputFilePath]).Count == 1;
            if (string.IsNullOrEmpty(baseDirectory) || !validFile)
            {
                throw new InvalidOperationException($"Path of configured output Messages file: {outputFilePath} is invalid or missing.");
            }

            if (!_fileSystem.DirectoryExists(baseDirectory))
            {
                _fileSystem.CreateDirectory(baseDirectory);
            }


            string fileName = Path.GetFileName(outputFilePath);
            if (!fileName.EndsWith(_defaultFileType, StringComparison.OrdinalIgnoreCase))
            {
                fileName += _defaultFileType;
            }

            string outputPath = Path.Combine(baseDirectory, fileName);
            fileWritingTask = Task.Factory.StartNew(() => ConsumeAndWriteToFilesBackgroundTask(outputPath), TaskCreationOptions.LongRunning);

            _globalObjectContainer!.RegisterInstanceAs<ICucumberMessageSink>(this, _pluginName, true);
        }

        public async Task PublishAsync(Envelope message)
        {
            await Task.Run(() => _postedMessages.Add(message));
        }

        internal abstract void ConsumeAndWriteToFilesBackgroundTask(string outputPath);

        private bool disposedValue = false;

        internal virtual void Dispose(bool disposing)
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

        internal string ParseConfigurationString(string messagesConfiguration, string pluginName)
        {
            if (string.IsNullOrWhiteSpace(messagesConfiguration))
                return string.Empty;

            try
            {
                using var document = System.Text.Json.JsonDocument.Parse(messagesConfiguration);
                if (document.RootElement.TryGetProperty("outputFilePath", out var outputFilePathElement))
                {
                    return outputFilePathElement.GetString() ?? string.Empty;
                }
            }
            catch (System.Text.Json.JsonException)
            {
                trace?.WriteToolOutput($"Configuration of ${pluginName} is invalid: ${messagesConfiguration}");
            }

            return string.Empty;
        }
    }
}
