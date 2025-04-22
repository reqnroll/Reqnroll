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

            string outputFilePath = ParseConfigurationString(messagesConfiguration, _pluginName);

            if (String.IsNullOrEmpty(outputFilePath))
                outputFilePath = $".\\{_defaultFileName}";

            string baseDirectory = Path.GetDirectoryName(outputFilePath);
            if (!string.IsNullOrEmpty(baseDirectory))
            {
                baseDirectory = SanitizeDirectoryName(baseDirectory);
                if (!Directory.Exists(baseDirectory))
                {
                    Directory.CreateDirectory(baseDirectory);
                }
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

        private string ParseConfigurationString(string messagesConfiguration, string pluginName)
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
        private static string SanitizeDirectoryName(string input)
        {
            if (string.IsNullOrEmpty(input))
                return string.Empty;

            // Get invalid characters for directory names
            char[] invalidChars = Path.GetInvalidPathChars().Concat(Path.GetInvalidFileNameChars()).ToArray();

            // Check if the path starts with a drive identifier (e.g., "C:\")
            string driveIdentifier = string.Empty;
            if (input.Length > 1 && input[1] == ':' && char.IsLetter(input[0]))
            {
                driveIdentifier = input.Substring(0, 2); // Extract the drive identifier (e.g., "C:")
                input = input.Substring(2); // Remove the drive identifier from the rest of the path
            }

            // Split the remaining path into segments and sanitize each segment
            var sanitizedSegments = input.Split(Path.DirectorySeparatorChar, Path.AltDirectorySeparatorChar)
                                          .Select(segment =>
                                          {
                                              string sanitized = new string(segment.Select(c => invalidChars.Contains(c) ? '_' : c).ToArray());
                                              return sanitized.Trim().Trim('.');
                                          });

            // Recombine the sanitized segments into a valid path
            string sanitizedPath = string.Join(Path.DirectorySeparatorChar.ToString(), sanitizedSegments);

            // Reattach the drive identifier if it exists
            if (!string.IsNullOrEmpty(driveIdentifier))
            {
                sanitizedPath = driveIdentifier + Path.DirectorySeparatorChar + sanitizedPath;
            }

            // Ensure the sanitized path is not empty
            return string.IsNullOrEmpty(sanitizedPath) ? "_" : sanitizedPath;
        }
    }
}
