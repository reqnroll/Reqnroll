#nullable enable

using System.IO;
using Cucumber.HtmlFormatter;
using Reqnroll.Formatters.Configuration;
using Reqnroll.Formatters.PayloadProcessing;
using Reqnroll.Formatters.PubSub;
using Reqnroll.Formatters.RuntimeSupport;
using Reqnroll.Utils;

namespace Reqnroll.Formatters.Html;

/// <summary>
/// Produces an HTML report file based on https://github.com/cucumber/html-formatter/ and https://github.com/cucumber/react-components.
/// </summary>
public class HtmlFormatterPlugin : FileWritingFormatterPluginBase
{
    public HtmlFormatterPlugin(IFormattersConfigurationProvider configurationProvider, ICucumberMessageBroker broker, IFormatterLog logger, IFileSystem fileSystem) : base(configurationProvider, broker, logger, "html", ".html", "reqnroll_report.html", fileSystem)
    {
    }

    protected override void ConsumeAndWriteToFilesBackgroundTask(string outputPath)
    {
        using var fileStream = File.Create(outputPath, TUNING_PARAM_FILE_WRITE_BUFFER_SIZE);
        using var htmlWriter = new MessagesToHtmlWriter(
            fileStream,
            (sw, e) => sw.Write(NdjsonSerializer.Serialize(e)));

        foreach (var message in PostedMessages.GetConsumingEnumerable())
        {
            if (message != null)
            {
                htmlWriter.Write(message);
            }
        }
    }
}