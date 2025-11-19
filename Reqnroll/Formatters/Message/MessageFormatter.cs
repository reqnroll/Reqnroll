#nullable enable

using System;
using System.IO;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Io.Cucumber.Messages.Types;
using Reqnroll.Formatters.Configuration;
using Reqnroll.Formatters.PayloadProcessing;
using Reqnroll.Formatters.RuntimeSupport;
using Reqnroll.Utils;

namespace Reqnroll.Formatters.Message;

/// <summary>
/// Produces a Cucumber Messages output (.ndjson) file.
/// </summary>
public class MessageFormatter : FileWritingFormatterBase
{
    private readonly byte[] _newLineBytes = Encoding.UTF8.GetBytes(Environment.NewLine);

    public MessageFormatter(IFormattersConfigurationProvider configurationProvider, IFormatterLog logger, IFileSystem fileSystem) : base(configurationProvider, logger, fileSystem, "message", ".ndjson", "reqnroll_report.ndjson")
    {
    }

    protected override void OnTargetFileStreamInitialized(Stream targetFileStream)
    {
        //nop
    }

    protected override void OnTargetFileStreamDisposing()
    {
        //nop
    }

    protected override async Task WriteToFile(Envelope envelope, CancellationToken cancellationToken)
    {
        if (TargetFileStream != null)
        {
            await NdjsonSerializer.SerializeToStreamAsync(TargetFileStream, envelope);

            // Write a newline after each message, except for the last one
            if (envelope.TestRunFinished == null)
                await TargetFileStream.WriteAsync(_newLineBytes, 0, _newLineBytes.Length, cancellationToken);
        }
    }
}