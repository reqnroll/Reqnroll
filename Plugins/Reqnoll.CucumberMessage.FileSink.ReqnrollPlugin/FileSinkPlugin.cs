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

[assembly: RuntimePlugin(typeof(FileSinkPlugin))]

namespace Reqnoll.CucumberMessage.FileSink.ReqnrollPlugin
{
    public class FileSinkPlugin : ICucumberMessageSink, IDisposable, IRuntimePlugin
    {
        private Task? fileWritingTask;

        private CucumberMessageSinkBase sinkBase = new CucumberMessageSinkBase();

        public FileSinkPlugin()
        {
            Debugger.Launch();
        }

        public void Initialize(RuntimePluginEvents runtimePluginEvents, RuntimePluginParameters runtimePluginParameters, UnitTestProviderConfiguration unitTestProviderConfiguration)
        {

            sinkBase.Initialize(runtimePluginEvents, runtimePluginParameters, unitTestProviderConfiguration);
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
            fileWritingTask?.Wait();
            fileWritingTask = null;
        }

        private void LaunchFileSink(TestRunStartedEvent testRunStarted)
        {
            Console.WriteLine( "LaunchFileSink called" );
            fileWritingTask = Task.Factory.StartNew(() => ConsumeAndWriteToFiles(), TaskCreationOptions.LongRunning);
        }

        private async Task ConsumeAndWriteToFiles()
        {
            Console.WriteLine( "ConsumeAndWriteToFiles called" );

            await foreach (var message in sinkBase.Consume())
            {
                var featureName = message.CucumberMessageSource;

                Console.WriteLine( "ConsumeAndWriteToFiles: " + featureName );
                if (message.Envelope != null)
                {
                    var cm = Serialize(message.Envelope);
                    Console.WriteLine("ConsumeAndWriteToFiles: " + cm);
                    Write(featureName, cm);
                }
                else
                {
                    CloseFeatureStream(featureName);
                }
            }
        }

        private Dictionary<string, StreamWriter> fileStreams = new();
        private string baseDirectory = Path.Combine("C:\\Users\\clrud\\source\\repos\\scratch", "CucumberMessages");
        private bool disposedValue;

        private string Serialize(Envelope message)
        {
            return NdjsonSerializer.Serialize(message);
        }
        private void Write(string featureName, string cucumberMessage)
        {
            string appName = Process.GetCurrentProcess().ProcessName;
            string appDirectory = Path.Combine(baseDirectory, appName);

            if (!fileStreams.ContainsKey(featureName))
            {
                fileStreams[featureName] = File.CreateText(Path.Combine(appDirectory, $"{featureName}.ndjson"));
            }
            fileStreams[featureName].WriteLine(cucumberMessage);
        }

        private void CloseFeatureStream(string featureName)
        {
            fileStreams[featureName].Close();
            fileStreams.Remove(featureName);
        }

        protected virtual void Dispose(bool disposing)
        {
            if (!disposedValue)
            {
                if (disposing)
                {
                    CloseFileSink(new TestRunFinishedEvent());
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

        public Task Publish(ReqnrollCucumberMessage message)
        {
            return sinkBase.Publish(message);
        }
    }
}
