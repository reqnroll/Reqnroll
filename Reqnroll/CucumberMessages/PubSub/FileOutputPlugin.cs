#nullable enable

using Reqnroll.CucumberMessages;
using Reqnroll.Plugins;
using Reqnroll.UnitTestProvider;
using Io.Cucumber.Messages.Types;
using System.Reflection;
using Reqnroll.Events;
using System.Collections.Concurrent;
using System.Text.Json;
using Reqnroll.Tracing;
using Reqnroll.BoDi;
using System;
using System.Threading.Tasks;
using System.IO;
using System.Linq;
using Reqnroll.EnvironmentAccess;
using Reqnroll.CommonModels;
using System.Diagnostics;
using Reqnroll.CucumberMessages.Configuration;
using Reqnroll.CucumberMessages.PubSub;
using Reqnroll.CucumberMessages.PayloadProcessing;
using System.Runtime.InteropServices.ComTypes;


namespace Reqnoll.CucumberMessage.FileSink.ReqnrollPlugin
{
    public class FileOutputPlugin : ICucumberMessageSink, IDisposable, IRuntimePlugin
    {
        private Task? fileWritingTask;

        //Thread safe collections to hold:
        // 1. Inbound Cucumber Messages - BlockingCollection<Cucumber Message>
        private readonly BlockingCollection<ReqnrollCucumberMessage> postedMessages = new();

        private ICucumberConfiguration _configuration;
        private Lazy<ITraceListener> traceListener;
        private ITraceListener? trace => traceListener.Value;
        private IObjectContainer? testThreadObjectContainer;
        private IObjectContainer? globalObjectContainer;


        public FileOutputPlugin(ICucumberConfiguration configuration)
        {
            _configuration = configuration;
            traceListener = new Lazy<ITraceListener>(() => testThreadObjectContainer!.Resolve<ITraceListener>());
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
            // Dispose will call CloseFileSink and CloseStream.
            // The former will shut down the message pipe and wait for the writer to complete.
            // The latter will close down the file stream.
            Dispose(true);
        }
        private void CloseFileSink()
        {
            if (disposedValue) return;
            trace?.WriteTestOutput("FileOutputPlugin Closing File Sink long running thread.");
            postedMessages.CompleteAdding();
            fileWritingTask?.Wait();
            fileWritingTask = null;
        }

        private void LaunchFileSink(TestRunStartedEvent testRunStarted)
        {
            ICucumberConfiguration config = _configuration;

            if (!config.Enabled)
            {
                trace!.WriteTestOutput("Cucumber Messages is DISABLED.");
                // By returning here, we don't launch the File writing thread,
                // and this class is not registered as a CucumberMessageSink, which indicates to the Broker that Messages are disabled.
                return;
            }
            string baseDirectory = Path.Combine(config.BaseDirectory, config.OutputDirectory);
            string fileName = SanitizeFileName(config.OutputFileName);

            trace?.WriteToolOutput($"Cuccumber Messages: Starting File Sink long running thread. Writing to: {baseDirectory}");
            fileWritingTask = Task.Factory.StartNew( () =>  ConsumeAndWriteToFilesBackgroundTask(baseDirectory, fileName), TaskCreationOptions.LongRunning);
            globalObjectContainer!.RegisterInstanceAs<ICucumberMessageSink>(this, "CucumberMessages_FileOutputPlugin", true);
        }

        public void Publish(ReqnrollCucumberMessage message)
        {
            //var contentType = message.Envelope == null ? "End of Messages Marker" : message.Envelope.Content().GetType().Name;
            //trace?.WriteTestOutput($"FileOutputPlugin Publish. Cucumber Message: {message.CucumberMessageSource}: {contentType}");
            postedMessages.Add(message);
        }

        private void ConsumeAndWriteToFilesBackgroundTask(string baseDirectory, string fileName)
        {
            var fileStream = File.CreateText(Path.Combine(baseDirectory, fileName));


            foreach (var message in postedMessages.GetConsumingEnumerable())
            {
                var featureName = message.CucumberMessageSource;

                if (message.Envelope != null)
                {
                    var cm = Serialize(message.Envelope);
                    //trace?.WriteTestOutput($"FileOutputPlugin ConsumeAndWriteToFiles. Cucumber Message: {featureName}: {cm.Substring(0, 20)}");
                    Write(fileStream, cm);
                }
            }

            CloseStream(fileStream);
        }


        private string Serialize(Envelope message)
        {
            return NdjsonSerializer.Serialize(message);
        }
        private void Write(StreamWriter fileStream, string cucumberMessage)
        {
            try
            {
                trace?.WriteTestOutput($"FileOutputPlugin Write. Cucumber Message: {cucumberMessage.Substring(0, 20)}");
                fileStream!.WriteLine(cucumberMessage);
            }
            catch (System.Exception ex)
            {
                trace?.WriteTestOutput($"FileOutputPlugin Write. Exception: {ex.Message}");
            }
        }

        private void CloseStream(StreamWriter fileStream)
        {
            trace?.WriteTestOutput($"FileOutputPlugin Closing File Stream.");
            fileStream?.Flush();
            fileStream?.Close();
            fileStream?.Dispose();
        }
        private bool disposedValue = false;

        protected virtual void Dispose(bool disposing)
        {
            if (!disposedValue)
            {
                if (disposing)
                {
                    CloseFileSink();
                    postedMessages.Dispose();
                }
                disposedValue = true;
            }
        }

        public void Dispose()
        {
            // Do not change this code. Put cleanup code in 'Dispose(bool disposing)' method
            Dispose(disposing: true);
            GC.SuppressFinalize(this);
        }
        public static string SanitizeFileName(string input)
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
