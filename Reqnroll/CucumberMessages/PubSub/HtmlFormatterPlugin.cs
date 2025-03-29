#nullable enable

using Reqnroll.Plugins;
using Reqnroll.UnitTestProvider;
using Reqnroll.Events;
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
using Cucumber.HtmlFormatter;


namespace Reqnroll.CucumberMessages.PubSub
{
    public class HtmlFormatterPlugin : ICucumberMessageSink, IDisposable, IRuntimePlugin
    {
        private ICucumberMessagesConfiguration _configuration;
        private ObjectContainer? _globalObjectContainer;
        private ObjectContainer? _testThreadObjectContainer;
        private readonly BlockingCollection<Envelope> _postedMessages = new();
        private const int TUNING_PARAM_FILE_WRITE_BUFFER_SIZE = 65536;
        private Task? fileWritingTask;


        public HtmlFormatterPlugin(ICucumberMessagesConfiguration configuration)
        {
            _configuration = configuration;
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

        public void Dispose()
        {
            // Do not change this code. Put cleanup code in 'Dispose(bool disposing)' method
            Dispose(disposing: true);
            GC.SuppressFinalize(this);
        }

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

        private void LaunchFileSink(TestRunStartedEvent testRunStartedEvent)
        {
            ICucumberMessagesConfiguration config = _configuration;

            if (!config.Enabled)
            {
                // By returning here, we don't launch the File writing thread,
                // and this class is not registered as a CucumberMessageSink, which indicates to the Broker that Messages are disabled.
                return;
            }
            string messagesConfiguration = config.FormatterConfiguration("html");

            // If the formatter has not been configured in, then treat it as DISABLED.
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
                outputFilePath = ".\\reqnroll_report.html";

            string baseDirectory = Path.GetDirectoryName(outputFilePath);
            if (!Directory.Exists(baseDirectory))
            {
                Directory.CreateDirectory(baseDirectory);
            }

            string fileName = SanitizeFileName(Path.GetFileName(outputFilePath));
            if (!fileName.EndsWith(".html", StringComparison.OrdinalIgnoreCase))
            {
                fileName += ".html";
            }

            string outputPath = Path.Combine(baseDirectory, fileName);
            fileWritingTask = Task.Factory.StartNew(() => ConsumeAndWriteToFilesBackgroundTask(outputPath), TaskCreationOptions.LongRunning);

            _globalObjectContainer!.RegisterInstanceAs<ICucumberMessageSink>(this, "html", true);
        }
        private void ConsumeAndWriteToFilesBackgroundTask(string outputPath)
        {
            using var fileStream = File.Create(outputPath, TUNING_PARAM_FILE_WRITE_BUFFER_SIZE);
            using var htmlWriter = new MessagesToHtmlWriter(fileStream,
                    (StreamWriter sw, Envelope e) => { sw.Write(NdjsonSerializer.Serialize(e)); }
                );

            foreach (var message in _postedMessages.GetConsumingEnumerable())
            {
                if (message != null)
                {
                    htmlWriter.Write(message);
                }
            }

        }

        public async Task PublishAsync(Envelope message)
        {
            await Task.Run(() => _postedMessages.Add(message));
        }

        private static string SanitizeFileName(string input)
        {
            if (string.IsNullOrEmpty(input))
                return string.Empty;

            // Get the invalid characters for file names
            char[] invalidChars = Path.GetInvalidFileNameChars();

            // Replace invalid characters with underscores
            string sanitized = new string(input.Select(c => invalidChars.Contains(c) ? '_' : c).ToArray());

            // Remove leading and trailing spaces and dots
            sanitized = sanitized.Trim().Trim('.');

            // Ensure the filename is not empty after sanitization
            if (string.IsNullOrEmpty(sanitized))
                return "_";

            // Truncate the filename if it's too long (255 characters is a common limit)
            const int maxLength = 255;
            if (sanitized.Length > maxLength)
                sanitized = sanitized.Substring(0, maxLength);

            return sanitized;
        }

    }
}
