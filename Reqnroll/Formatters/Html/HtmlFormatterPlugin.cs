#nullable enable

using Cucumber.HtmlFormatter;
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
    private FileStream? _fileStream;
    private MessagesToHtmlWriter? _htmlWriter;

    protected override void FinalizeInitialization(string outputPath, IDictionary<string, object> formatterConfiguration, Action<bool> onInitialized)
    {
        try
        {
            _fileStream = File.Create(outputPath, TUNING_PARAM_FILE_WRITE_BUFFER_SIZE);
            _htmlWriter = new MessagesToHtmlWriter(
                _fileStream,
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

    protected override async Task ConsumeAndWriteToFilesBackgroundTask(string outputPath, CancellationToken cancellationToken)
    {
        if (_fileStream == null || _htmlWriter == null)
        {
            Logger.WriteMessage($"Formatter {Name} closing because the filestream is not open.");
            return;
        }

        try
        {
            foreach (var message in PostedMessages.GetConsumingEnumerable())
            {
                if (cancellationToken.IsCancellationRequested)
                {
                    Logger.WriteMessage($"Formatter {Name} has been cancelled.");
                    _closed = true;
                    await _fileStream.FlushAsync();
                    break;
                }

                if (message != null)
                {
                    await _htmlWriter.WriteAsync(message);
                }
            }
            await _fileStream.FlushAsync();
        }
        catch (Exception e)
        {
            Logger.WriteMessage($"Formatter {Name} threw an exception: {e.Message}. No further messages will be added to the generated html file.");
            throw;
        }
    }

    public override void Dispose()
    {
        _htmlWriter?.Dispose();
        _fileStream?.Close();
        _fileStream?.Dispose();
        base.Dispose();
    }
}