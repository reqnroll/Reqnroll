#nullable enable

using Cucumber.HtmlFormatter;
using Io.Cucumber.Messages.Types;
using Reqnroll.Formatters.Configuration;
using Reqnroll.Formatters.PayloadProcessing;
using Reqnroll.Formatters.RuntimeSupport;
using Reqnroll.Utils;
using System;
using System.IO;
using System.Threading;
using System.Threading.Tasks;

namespace Reqnroll.Formatters.Html;

/// <summary>
/// Produces an HTML report file based on https://github.com/cucumber/html-formatter/ and https://github.com/cucumber/react-components.
/// </summary>
public class HtmlFormatter : FileWritingFormatterBase
{
    private MessagesToHtmlWriter? _htmlWriter;

    protected HtmlFormatter(IFormattersConfigurationProvider configurationProvider, IFormatterLog logger, IFileSystem fileSystem, string pluginName) : 
        base(configurationProvider, logger, fileSystem, pluginName, ".html", "reqnroll_report.html")
    {
    }

    public HtmlFormatter(IFormattersConfigurationProvider configurationProvider, IFormatterLog logger, IFileSystem fileSystem) : 
        this(configurationProvider, logger, fileSystem, "html")
    {
    }

    protected virtual MessagesToHtmlWriter CreateMessagesToHtmlWriter(Stream stream, Func<StreamWriter, Envelope, Task> asyncStreamSerializer) 
        => new(stream, asyncStreamSerializer);

    protected override void OnTargetFileStreamInitialized(Stream targetFileStream)
    {
        _htmlWriter = CreateMessagesToHtmlWriter(
            targetFileStream,
            async (sw, e) => await sw.WriteAsync(NdjsonSerializer.Serialize(e)));
    }

    protected override void OnTargetFileStreamDisposing()
    {
        _htmlWriter?.Dispose();
        _htmlWriter = null;
    }

    protected override async Task FlushTargetFileStream(CancellationToken cancellationToken)
    {
        if (_htmlWriter != null)
        {
            await _htmlWriter.DisposeAsync();
            _htmlWriter = null;
            // We should not call base.FlushTargetFileStream here because the HtmlWriter already disposed the stream
            // and the stream's flush operation would throw an exception.
        }
        else
        {
            await base.FlushTargetFileStream(cancellationToken);
        }
    }

    protected override async Task WriteToFile(Envelope envelope, CancellationToken cancellationToken)
    {
        if (_htmlWriter != null)
        {
            await _htmlWriter.WriteAsync(envelope);
        }
    }
}