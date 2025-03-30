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
    /// <summary>
    /// The MessagesFormatterPlugin is the subscriber to the CucumberMessageBroker. 
    /// It receives Cucumber Messages and writes them to a file.
    /// 
    /// </summary>
    public class MessagesFormatterPlugin : FileWritingFormatterPluginBase
    {

        public MessagesFormatterPlugin(ICucumberMessagesConfiguration configuration) : base(configuration, "messages", ".ndjson", "reqnroll_report.ndjson")
        {
        }

        protected override void ConsumeAndWriteToFilesBackgroundTask(string outputPath)
        {
            byte[] nl = Encoding.UTF8.GetBytes(Environment.NewLine);

            using var fileStream = File.Create(outputPath, TUNING_PARAM_FILE_WRITE_BUFFER_SIZE);

            foreach (var message in _postedMessages.GetConsumingEnumerable())
            {
                if (message != null)
                {
                    NdjsonSerializer.SerializeToStream(fileStream!, message);

                    // Write a newline after each message, except for the last one
                    if (message.TestRunFinished == null)
                        fileStream!.Write(nl, 0, nl.Length);
                }
            }
        }
    }
}
