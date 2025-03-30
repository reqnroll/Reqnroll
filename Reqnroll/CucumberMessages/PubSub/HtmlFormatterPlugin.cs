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
    public class HtmlFormatterPlugin : FileWritingFormatterPluginBase
    {
        public HtmlFormatterPlugin(ICucumberMessagesConfiguration configuration) : base(configuration, "html", ".html", "reqnroll_report.html")
        {
        }

        protected override void ConsumeAndWriteToFilesBackgroundTask(string outputPath)
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
    }
}
