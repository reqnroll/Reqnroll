using Reqnroll.CucumberMessages;
using Reqnroll.Plugins;
using Reqnroll.UnitTestProvider;
using Io.Cucumber.Messages;
using Cucumber.Messages;
using Io.Cucumber.Messages.Types;
using System.Reflection;
using Reqnoll.CucumberMessage.FileSink.ReqnrollPlugin;
using System.Diagnostics;
using Reqnroll.Events;
using System.Collections.Concurrent;
using System.Text.Json;

[assembly: RuntimePlugin(typeof(FileSinkPlugin))]

namespace Reqnoll.CucumberMessage.FileSink.ReqnrollPlugin
{
    public class FileSinkPlugin : ICucumberMessageSink, IDisposable, IRuntimePlugin
    {
        private const string CUCUMBERMESSAGESCONFIGURATIONFILENAME = "CucumberMessages.configuration.json";
        private Task? fileWritingTask;

        //Thread safe collections to hold:
        // 1. Inbound Cucumber Messages - BlockingCollection<Cucumber Message>
        // 2. Dictionary of Feature Streams (Key: Feature Name, Value: StreamWriter)
        private object _lock = new();
        private readonly BlockingCollection<ReqnrollCucumberMessage> postedMessages = new();
        private readonly ConcurrentDictionary<string, StreamWriter> fileStreams = new();
        private FileSinkConfiguration? configuration;
        private string baseDirectory = "";

        public FileSinkPlugin()
        {
        }

        public void Initialize(RuntimePluginEvents runtimePluginEvents, RuntimePluginParameters runtimePluginParameters, UnitTestProviderConfiguration unitTestProviderConfiguration)
        {

            configuration = JsonSerializer.Deserialize<FileSinkConfiguration>(File.ReadAllText(CUCUMBERMESSAGESCONFIGURATIONFILENAME), new JsonSerializerOptions { PropertyNamingPolicy = JsonNamingPolicy.CamelCase })!;
            if (!configuration!.FileSinkEnabled)
                return;

            baseDirectory = ProcessConfiguration(configuration);

            runtimePluginEvents.RegisterGlobalDependencies += (sender, args) => args.ObjectContainer.RegisterInstanceAs<ICucumberMessageSink>(this);

            runtimePluginEvents.CustomizeTestThreadDependencies += (sender, args) =>
            {
                var testThreadExecutionEventPublisher = args.ObjectContainer.Resolve<ITestThreadExecutionEventPublisher>();
                testThreadExecutionEventPublisher.AddHandler<TestRunStartedEvent>(LaunchFileSink);
                testThreadExecutionEventPublisher.AddHandler<TestRunFinishedEvent>(CloseFileSink);
            };
        }

        private string ProcessConfiguration(FileSinkConfiguration configuration)
        {
            var activeDestination = configuration.Destinations.Where(d => d.Enabled).FirstOrDefault();

            if (activeDestination != null)
            {
                var basePath = Path.Combine(activeDestination.BasePath, activeDestination.OutputDirectory);
                if (!Directory.Exists(basePath))
                {
                    lock(_lock)
                    {
                        if (!Directory.Exists(basePath))
                            Directory.CreateDirectory(basePath);
                    }
                }

                return basePath;
            }
            else
            {
                return Assembly.GetExecutingAssembly().Location;
            }
        }

        private void CloseFileSink(TestRunFinishedEvent @event)
        {
            postedMessages.CompleteAdding();
            fileWritingTask?.Wait();
            fileWritingTask = null;
        }

        private void LaunchFileSink(TestRunStartedEvent testRunStarted)
        {
            fileWritingTask = Task.Factory.StartNew(async () => await ConsumeAndWriteToFiles(), TaskCreationOptions.LongRunning);
        }

        public void Publish(ReqnrollCucumberMessage message)
        {
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
                    await Write(featureName, cm);
                }
                else
                {
                    CloseFeatureStream(featureName);
                }
            }
        }

        private bool disposedValue;

        private string Serialize(Envelope message)
        {
            return NdjsonSerializer.Serialize(message);
        }
        private async Task Write(string featureName, string cucumberMessage)
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
            await fileStreams[featureName].WriteLineAsync(cucumberMessage);
        }

        private void CloseFeatureStream(string featureName)
        {
            fileStreams[featureName].Close();
            fileStreams.TryRemove(featureName, out var _);
        }

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
