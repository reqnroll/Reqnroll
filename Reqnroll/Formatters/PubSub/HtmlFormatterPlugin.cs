#nullable enable

using Reqnroll.Plugins;
using Reqnroll.UnitTestProvider;
using Reqnroll.Events;
using Reqnroll.BoDi;
using System;
using System.Threading.Tasks;
using System.IO;
using System.Linq;
using Reqnroll.Formatters.PayloadProcessing;
using System.Text;
using System.Collections.Concurrent;
using Io.Cucumber.Messages.Types;
using Cucumber.HtmlFormatter;
using Reqnroll.Utils;
using Reqnroll.Formatters.Configuration;


namespace Reqnroll.Formatters.PubSub
{
    /// <summary>
    /// This Cucumber Messages plugin recieves the generated Cucumber Messages and writes each Feature to disk as an HTML report
    /// (uses the Cucumber HtmlFormatter and react components to generate the html)
    /// </summary>
    public class HtmlFormatterPlugin : FileWritingFormatterPluginBase
    {
        public HtmlFormatterPlugin(IFormattersConfiguration configuration, ICucumberMessageBroker broker, IFileSystem fileSystem) : base(configuration, broker, "html", ".html", "reqnroll_report.html", fileSystem)
        {
        }

        internal override void ConsumeAndWriteToFilesBackgroundTask(string outputPath)
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
