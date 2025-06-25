#nullable enable

using System;
using System.IO;
using System.Threading.Tasks;
using Cucumber.HtmlFormatter;
using Reqnroll.Formatters.Configuration;
using Reqnroll.Formatters.PayloadProcessing;
using Reqnroll.Formatters.PayloadProcessing.Cucumber;
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

    protected override async Task ConsumeAndWriteToFilesBackgroundTask(string outputPath)
    {
        try
        {
            using var fileStream = File.Create(outputPath, TUNING_PARAM_FILE_WRITE_BUFFER_SIZE);
            await using var htmlWriter = new MessagesToHtmlWriter(
                fileStream,
                async (sw, e) => await sw.WriteAsync(NdjsonSerializer.Serialize(e)));

            foreach (var message in PostedMessages.GetConsumingEnumerable())
            {
                if (message != null)
                {
                    await htmlWriter.WriteAsync(message);
                }
            }
        }
        catch (Exception e)
        {
            Logger.WriteMessage($"Formatter {Name} threw an exception: {e.Message}. No further messages will be added to the generated html file.");
            throw;
        }
    }
}