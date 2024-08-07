using Reqnroll.CucumberMesssages;
using Reqnroll.Plugins;
using Reqnroll.UnitTestProvider;
using Io.Cucumber.Messages;
using Cucumber.Messages;
using Io.Cucumber.Messages.Types;
using System.Reflection;
using Reqnoll.CucumberMessage.FileSink.ReqnrollPlugin;

[assembly: RuntimePlugin(typeof(FileSinkPlugin))]

namespace Reqnoll.CucumberMessage.FileSink.ReqnrollPlugin
{
    public class FileSinkPlugin : CucumberMessageSinkBase
    {
        new public void Initialize(RuntimePluginEvents runtimePluginEvents, RuntimePluginParameters runtimePluginParameters, UnitTestProviderConfiguration unitTestProviderConfiguration)
        {
            base.Initialize(runtimePluginEvents, runtimePluginParameters, unitTestProviderConfiguration);

            Task.Run(() => ConsumeAndWriteToFiles());
        }

        private async Task ConsumeAndWriteToFiles()
        {
            await foreach (var message in Consume())
            {
                var featureName = message.CucumberMessageSource;
                if (message.Envelope != null)
                {
                    Write(featureName, Serialize(message.Envelope));
                }
                else
                {
                    CloseFeatureStream(featureName);
                }
            }
        }

        private Dictionary<string, StreamWriter> fileStreams = new();
        private string baseDirectory = Path.Combine(Directory.GetCurrentDirectory(), "CucumberMessages");

        private string Serialize(Envelope message)
        {
            return NdjsonSerializer.Serialize(message);
        }
        private void Write(string featureName, string cucumberMessage)
        {
            if (!fileStreams.ContainsKey(featureName))
            {
                fileStreams[featureName] = File.CreateText(Path.Combine(baseDirectory, $"{featureName}.ndjson"));
            }
            fileStreams[featureName].WriteLine(cucumberMessage);
        }

        private void CloseFeatureStream(string featureName)
        {
            fileStreams[featureName].Close();
            fileStreams.Remove(featureName);
        }
    }
}
