#nullable enable

using Reqnroll.Formatters.PayloadProcessing;
using Reqnroll.Formatters.Configuration;
using Reqnroll.Utils;
using System;
using System.IO;
using System.Text;


namespace Reqnroll.Formatters.PubSub
{
    /// <summary>
    /// The MessagesFormatterPlugin is the subscriber to the CucumberMessageBroker. 
    /// It receives Cucumber Messages and writes them to a file.
    /// 
    /// </summary>
    public class MessagesFormatterPlugin : FileWritingFormatterPluginBase
    {

        public MessagesFormatterPlugin(IFormattersConfiguration configuration, ICucumberMessageBroker broker, IFileSystem fileSystem) : base(configuration, broker, "messages", ".ndjson", "reqnroll_report.ndjson", fileSystem)
        {
        }

        internal override void ConsumeAndWriteToFilesBackgroundTask(string outputPath)
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
