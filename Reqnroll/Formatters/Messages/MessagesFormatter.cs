#nullable enable

using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Io.Cucumber.Messages.Types;
using Reqnroll.Formatters.Configuration;
using Reqnroll.Formatters.PayloadProcessing;
using Reqnroll.Formatters.PubSub;
using Reqnroll.Formatters.RuntimeSupport;
using Reqnroll.Utils;

namespace Reqnroll.Formatters.Messages;

/// <summary>
/// Produces a Cucumber Messages output (.ndjson) file.
/// </summary>
public class MessagesFormatter : FileWritingFormatterBase, IDisposable
{
    private byte[] _newLineBytes = Encoding.UTF8.GetBytes(Environment.NewLine);

    public MessagesFormatter(IFormattersConfigurationProvider configurationProvider, IFormatterLog logger, IFileSystem fileSystem) : base(configurationProvider, logger, "messages", ".ndjson", "reqnroll_report.ndjson", fileSystem)
    {
    }
    protected override void FinalizeInitialization(string outputPath, IDictionary<string, object> formatterConfiguration, Action<bool> onInitialized)
    {
        try
        {
            FileStreamTarget = File.Create(outputPath, TUNING_PARAM_FILE_WRITE_BUFFER_SIZE);
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
        if (FileStreamTarget != null)
        {
            await NdjsonSerializer.SerializeToStreamAsync(FileStreamTarget, envelope);

            // Write a newline after each message, except for the last one
            if (envelope!.TestRunFinished == null)
                await FileStreamTarget.WriteAsync(_newLineBytes, 0, _newLineBytes.Length, cancellationToken);
        }
    }

    protected override async Task OnCancellation() { await Task.CompletedTask; }

    public override void Dispose()
    {
        FileStreamTarget?.Close();
        FileStreamTarget?.Dispose();
        base.Dispose();
    }
}