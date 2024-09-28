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


namespace Reqnoll.CucumberMessage.FileSink.ReqnrollPlugin
{
    public class FileOutputPlugin : ICucumberMessageSink, IDisposable, IRuntimePlugin
    {
        private const string CUCUMBERMESSAGESCONFIGURATIONFILENAME = "CucumberMessages.configuration.json";
        private Task? fileWritingTask;
        private object _lock = new();

        //Thread safe collections to hold:
        // 1. Inbound Cucumber Messages - BlockingCollection<Cucumber Message>
        // 2. Dictionary of Feature Streams (Key: Feature Name, Value: StreamWriter)
        private readonly BlockingCollection<ReqnrollCucumberMessage> postedMessages = new();
        private readonly ConcurrentDictionary<string, StreamWriter> fileStreams = new();
        private string baseDirectory = "";
        private Lazy<ITraceListener> traceListener;
        private ITraceListener? trace => traceListener.Value;
        private IObjectContainer? testThreadObjectContainer;
        private IObjectContainer? globalObjectContainer;


        public FileOutputPlugin()
        {
            traceListener = new Lazy<ITraceListener>(() => testThreadObjectContainer!.Resolve<ITraceListener>());
            //Debugger.Launch();
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
                testThreadExecutionEventPublisher.AddHandler<TestRunFinishedEvent>(CloseFileSink);
            };
        }
        private void CloseFileSink(TestRunFinishedEvent @event)
        {
            trace?.WriteTestOutput("FileOutputPlugin Closing File Sink long running thread.");
            postedMessages.CompleteAdding();
            fileWritingTask?.Wait();
            fileWritingTask = null;
        }

        private void LaunchFileSink(TestRunStartedEvent testRunStarted)
        {
            var environment = testThreadObjectContainer!.Resolve<IEnvironmentWrapper>();
            var enabledResult = environment.GetEnvironmentVariable(CucumberConfiguration.REQNROLL_CUCUMBERMESSAGES_ENABLE_ENVIRONMENT_VARIABLE);
            var enabled = enabledResult is Success<string> ? ((Success<string>)enabledResult).Result : "TRUE";
            bool isMessagesEnabled = "true".Equals(enabled, StringComparison.InvariantCultureIgnoreCase);

            CucumberOutputConfiguration configurationDTO;
            if (File.Exists(CUCUMBERMESSAGESCONFIGURATIONFILENAME))
            {
                configurationDTO = JsonSerializer.Deserialize<CucumberOutputConfiguration>(File.ReadAllText(CUCUMBERMESSAGESCONFIGURATIONFILENAME), new JsonSerializerOptions { PropertyNamingPolicy = JsonNamingPolicy.CamelCase })!;
            }
            else 
                configurationDTO = new CucumberOutputConfiguration(isMessagesEnabled);

            if (!configurationDTO.FileOutputEnabled)
            {
                trace!.WriteTestOutput("FileOutputPlugin LaunchFileSink. Cucumber Messages is DISABLED.");
                return;
            }

            var configuration = new CucumberConfiguration(trace, environment);
            baseDirectory = configuration.ConfigureOutputDirectory(configurationDTO);
            trace!.WriteTestOutput($"FileOutputPlugin LaunchFileSink. Cucumber Messages is ENABLED. Base Directory: {baseDirectory}");
            trace?.WriteTestOutput("FileOutputPlugin Starting File Sink long running thread.");
            fileWritingTask = Task.Factory.StartNew(async () => await ConsumeAndWriteToFiles(), TaskCreationOptions.LongRunning);
            globalObjectContainer!.RegisterInstanceAs<ICucumberMessageSink>(this, "CucumberMessages_FileOutputPlugin", true);
        }

        public void Publish(ReqnrollCucumberMessage message)
        {
            var contentType = message.Envelope == null ? "End of Messages Marker" : message.Envelope.Content().GetType().Name;
            //trace?.WriteTestOutput($"FileOutputPlugin Publish. Cucumber Message: {message.CucumberMessageSource}: {contentType}");
            postedMessages.Add(message);
        }

        private async Task ConsumeAndWriteToFiles()
        {
            foreach (var message in postedMessages.GetConsumingEnumerable())
            {
                var featureName = message.CucumberMessageSource;

                if (message.Envelope != null)
                {
                    var cm = Serialize(message.Envelope);
                    //trace?.WriteTestOutput($"FileOutputPlugin ConsumeAndWriteToFiles. Cucumber Message: {message.CucumberMessageSource}: {cm.Substring(0, 20)}");
                    await Write(featureName, cm);
                }
                else
                {
                    //trace?.WriteTestOutput($"FileOutputPlugin ConsumeAndWriteToFiles. End of Messages Marker Received.");
                    CloseFeatureStream(featureName);
                }
            }
        }


        private string Serialize(Envelope message)
        {
            return NdjsonSerializer.Serialize(message);
        }
        private async Task Write(string featureName, string cucumberMessage)
        {
            try
            {
                if (!fileStreams.ContainsKey(featureName))
                {
                    lock (_lock)
                    {
                        if (!fileStreams.ContainsKey(featureName))
                        {
                            fileStreams[featureName] = File.CreateText(Path.Combine(baseDirectory, SanitizeFileName($"{featureName}.ndjson")));
                        }
                    }
                }
                trace?.WriteTestOutput($"FileOutputPlugin Write. Writing to: {SanitizeFileName($"{featureName}.ndjson")}. Cucumber Message: {featureName}: {cucumberMessage.Substring(0, 20)}");
                await fileStreams[featureName].WriteLineAsync(cucumberMessage);
            }
            catch (System.Exception ex)
            {
                trace?.WriteTestOutput($"FileOutputPlugin Write. Exception: {ex.Message}");
            }
        }

        private void CloseFeatureStream(string featureName)
        {
            trace?.WriteTestOutput($"FileOutputPlugin CloseFeatureStream. Closing: {featureName}.");
            fileStreams[featureName].Close();
            fileStreams.TryRemove(featureName, out var _);
        }
        private bool disposedValue;

        protected virtual void Dispose(bool disposing)
        {
            if (!disposedValue)
            {
                if (disposing)
                {
                    CloseFileSink(new TestRunFinishedEvent());
                    postedMessages.Dispose();
                    foreach (var stream in fileStreams.Values)
                    {
                        stream.Close();
                        stream.Dispose();
                    };
                    fileStreams.Clear();
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
