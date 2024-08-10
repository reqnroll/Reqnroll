using Reqnroll.CucumberMesssages;
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

[assembly: RuntimePlugin(typeof(FileSinkPlugin))]

namespace Reqnoll.CucumberMessage.FileSink.ReqnrollPlugin
{
    //TODO: Add support for Reqnroll Configuration to initialize the FileSinkPlugin by specifying the path to the base directory.

    public class FileSinkPlugin : ICucumberMessageSink, IDisposable, IRuntimePlugin
    {
        private Task? fileWritingTask;

        //Thread safe collections to hold:
        // 1. Inbound Cucumber Messages - BlockingCollection<Cucumber Message>
        // 2. Dictionary of Feature Streams (Key: Feature Name, Value: StreamWriter)

        private readonly BlockingCollection<ReqnrollCucumberMessage> postedMessages = new();
        private readonly ConcurrentDictionary<string, StreamWriter> fileStreams = new();

        public FileSinkPlugin()
        {
            //Debugger.Launch();
        }

        public void Initialize(RuntimePluginEvents runtimePluginEvents, RuntimePluginParameters runtimePluginParameters, UnitTestProviderConfiguration unitTestProviderConfiguration)
        {

            runtimePluginEvents.RegisterGlobalDependencies += (sender, args) => args.ObjectContainer.RegisterInstanceAs<ICucumberMessageSink>(this);

            runtimePluginEvents.CustomizeTestThreadDependencies += (sender, args) =>
            {
                var testThreadExecutionEventPublisher = args.ObjectContainer.Resolve<ITestThreadExecutionEventPublisher>();
                testThreadExecutionEventPublisher.AddHandler<TestRunStartedEvent>(LaunchFileSink);
                testThreadExecutionEventPublisher.AddHandler<TestRunFinishedEvent>(CloseFileSink);
            };
        }

        private void CloseFileSink(TestRunFinishedEvent @event)
        {
            postedMessages.CompleteAdding();
            fileWritingTask?.Wait();
            fileWritingTask = null;
        }

        private void LaunchFileSink(TestRunStartedEvent testRunStarted)
        {
            Console.WriteLine("LaunchFileSink called");

            if (!Directory.Exists(baseDirectory))
            {
                Directory.CreateDirectory(baseDirectory);
            }

            fileWritingTask = Task.Factory.StartNew(async () => await ConsumeAndWriteToFiles(), TaskCreationOptions.LongRunning);
        }

        public void Publish(ReqnrollCucumberMessage message)
        {
            postedMessages.Add(message);
        }

        private async Task ConsumeAndWriteToFiles()
        {
            Console.WriteLine("ConsumeAndWriteToFiles called");

            foreach (var message in postedMessages.GetConsumingEnumerable())
            {
                var featureName = message.CucumberMessageSource;

                Console.WriteLine("ConsumeAndWriteToFiles: " + featureName);
                if (message.Envelope != null)
                {
                    var cm = Serialize(message.Envelope);
                    Console.WriteLine("ConsumeAndWriteToFiles: " + cm);
                    await Write(featureName, cm);
                }
                else
                {
                    CloseFeatureStream(featureName);
                }
            }
        }

        private string baseDirectory = Path.Combine("C:\\Users\\clrud\\source\\repos\\scratch", "CucumberMessages");
        private bool disposedValue;

        private string Serialize(Envelope message)
        {
            return NdjsonSerializer.Serialize(message);
        }
        private async Task Write(string featureName, string cucumberMessage)
        {

            if (!fileStreams.ContainsKey(featureName))
            {
                fileStreams[featureName] = File.CreateText(Path.Combine(baseDirectory, $"{featureName}.ndjson"));
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
