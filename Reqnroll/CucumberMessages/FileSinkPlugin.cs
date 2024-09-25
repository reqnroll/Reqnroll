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


namespace Reqnoll.CucumberMessage.FileSink.ReqnrollPlugin
{
    public class FileSinkPlugin : ICucumberMessageSink, IDisposable, IRuntimePlugin
    {
        private const string CUCUMBERMESSAGESCONFIGURATIONFILENAME = "CucumberMessages.configuration.json";
        private const string CUCUMBERMESSAGES_ENABLE_ENVIRONMENT_VARIABLE = "REQNROLL_CUCUMBER_MESSAGES_ENABLED";
        private Task? fileWritingTask;
        private object _lock = new();

        //Thread safe collections to hold:
        // 1. Inbound Cucumber Messages - BlockingCollection<Cucumber Message>
        // 2. Dictionary of Feature Streams (Key: Feature Name, Value: StreamWriter)
        private readonly BlockingCollection<ReqnrollCucumberMessage> postedMessages = new();
        private readonly ConcurrentDictionary<string, StreamWriter> fileStreams = new();
        private FileSinkConfiguration? configuration;
        private string baseDirectory = "";
        private Lazy<ITraceListener>? traceListener;
        private ITraceListener? trace => traceListener?.Value;
        private IObjectContainer? objectContainer;

        public FileSinkPlugin()
        {
            traceListener = new Lazy<ITraceListener>(() => objectContainer!.Resolve<ITraceListener>());
        }

        public void Initialize(RuntimePluginEvents runtimePluginEvents, RuntimePluginParameters runtimePluginParameters, UnitTestProviderConfiguration unitTestProviderConfiguration)
        {

            runtimePluginEvents.RegisterGlobalDependencies += (sender, args) => args.ObjectContainer.RegisterInstanceAs<ICucumberMessageSink>(this);

            runtimePluginEvents.CustomizeTestThreadDependencies += (sender, args) =>
            {
                var testThreadExecutionEventPublisher = args.ObjectContainer.Resolve<ITestThreadExecutionEventPublisher>();
                objectContainer = args.ObjectContainer;
                testThreadExecutionEventPublisher.AddHandler<TestRunStartedEvent>(LaunchFileSink);
                testThreadExecutionEventPublisher.AddHandler<TestRunFinishedEvent>(CloseFileSink);
            };
        }
        private void CloseFileSink(TestRunFinishedEvent @event)
        {
            trace?.WriteTestOutput("FileSinkPlugin Closing File Sink long running thread.");
            postedMessages.CompleteAdding();
            fileWritingTask?.Wait();
            fileWritingTask = null;
        }

        private void LaunchFileSink(TestRunStartedEvent testRunStarted)
        {
            bool environmentEnabled = "true".Equals(Environment.GetEnvironmentVariable(CUCUMBERMESSAGES_ENABLE_ENVIRONMENT_VARIABLE), StringComparison.InvariantCultureIgnoreCase);
            bool environmentLocationSpecified = !String.IsNullOrEmpty(FileSinkConfiguration.REQNROLL_CUCUMBER_MESSAGES_OUTPUT_DIRECTORY_ENVIRONMENT_VARIABLE);
            if (File.Exists(CUCUMBERMESSAGESCONFIGURATIONFILENAME))
            {
                configuration = JsonSerializer.Deserialize<FileSinkConfiguration>(File.ReadAllText(CUCUMBERMESSAGESCONFIGURATIONFILENAME), new JsonSerializerOptions { PropertyNamingPolicy = JsonNamingPolicy.CamelCase })!;
            }
            else if (environmentEnabled && environmentLocationSpecified)
                configuration = new FileSinkConfiguration(true);
            else configuration = new FileSinkConfiguration(false);
            if (!configuration.FileSinkEnabled)
            {
                trace?.WriteTestOutput("FileSinkPlugin LaunchFileSink. Cucumber Messages is DISABLED.");
                return;
            }

            baseDirectory = configuration.ConfiguredOutputDirectory(trace);

            trace?.WriteTestOutput("FileSinkPlugin Starting File Sink long running thread.");
            fileWritingTask = Task.Factory.StartNew(async () => await ConsumeAndWriteToFiles(), TaskCreationOptions.LongRunning);
        }

        public void Publish(ReqnrollCucumberMessage message)
        {
            var contentType = message.Envelope == null ? "End of Messages Marker" : message.Envelope.Content().GetType().Name;
            trace?.WriteTestOutput($"FileSinkPlugin Publish. Cucumber Message: {message.CucumberMessageSource}: {contentType}");
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
                    trace?.WriteTestOutput($"FileSinkPlugin ConsumeAndWriteToFiles. Cucumber Message: {message.CucumberMessageSource}: {cm.Substring(0, 20)}");
                    await Write(featureName, cm);
                }
                else
                {
                    trace?.WriteTestOutput($"FileSinkPlugin ConsumeAndWriteToFiles. End of Messages Marker Received.");
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
                            fileStreams[featureName] = File.CreateText(Path.Combine(baseDirectory, $"{featureName}.ndjson"));
                        }
                    }
                }
                trace?.WriteTestOutput($"FileSinkPlugin Write. Writing to: {featureName}. Cucumber Message: {featureName}: {cucumberMessage.Substring(0, 20)}");
                await fileStreams[featureName].WriteLineAsync(cucumberMessage);
            }
            catch (System.Exception ex)
            {
                trace?.WriteTestOutput($"FileSinkPlugin Write. Exception: {ex.Message}");
            }
        }

        private void CloseFeatureStream(string featureName)
        {
            trace?.WriteTestOutput($"FileSinkPlugin CloseFeatureStream. Closing: {featureName}.");
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
    }
}
