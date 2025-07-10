#nullable enable

using Cucumber.HtmlFormatter;
using Io.Cucumber.Messages.Types;
using Reqnroll.Formatters.Configuration;
using Reqnroll.Formatters.PayloadProcessing;
using Reqnroll.Formatters.PubSub;
using Reqnroll.Formatters.RuntimeSupport;
using Reqnroll.Utils;
using System;
using System.Collections.Generic;
using System.IO;
using System.Threading;
using System.Threading.Tasks;

namespace Reqnroll.Formatters.Html;

/// <summary>
/// Produces an HTML report file based on https://github.com/cucumber/html-formatter/ and https://github.com/cucumber/react-components.
/// </summary>
public class HtmlFormatterPlugin : FileWritingFormatterPluginBase, IDisposable
{

    public HtmlFormatterPlugin(IFormattersConfigurationProvider configurationProvider, IFormatterLog logger, IFileSystem fileSystem) : base(configurationProvider, logger, "html", ".html", "reqnroll_report.html", fileSystem)
    {
    }
    private MessagesToHtmlWriter? _htmlWriter;

    protected override void FinalizeInitialization(string outputPath, IDictionary<string, object> formatterConfiguration, Action<bool> onInitialized)
    {
        try
        {
            FileStreamTarget = File.Create(outputPath, TUNING_PARAM_FILE_WRITE_BUFFER_SIZE);
            _htmlWriter = new MessagesToHtmlWriter(
                FileStreamTarget,
                async (sw, e) => await sw.WriteAsync(NdjsonSerializer.Serialize(e)));
            onInitialized(true);
            Logger.WriteMessage($"Formatter {Name} opened filestream.");
        }
        catch
        {
            Logger.WriteMessage($"Formatter {Name} closing because of an exception opening the filestream.");

            onInitialized(false);
        }
    }

    protected override async Task WriteToFile(Envelope? envelope, CancellationToken cancellationToken)
    {
        if (_htmlWriter != null && envelope != null)
        {
            await _htmlWriter.WriteAsync(envelope);
        }
    }

    public override void Dispose()
    {
        _htmlWriter?.Dispose();
        FileStreamTarget?.Close();
        FileStreamTarget?.Dispose();
        base.Dispose();
    }

    protected override async Task OnCancellation()
    {
        // Ensure that the HTML writer is disposed properly
        _htmlWriter?.Dispose();
        _htmlWriter = null;
        await Task.CompletedTask;
    }
}