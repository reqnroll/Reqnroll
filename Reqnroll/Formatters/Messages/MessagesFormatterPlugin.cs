#nullable enable

using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Reqnroll.Formatters.Configuration;
using Reqnroll.Formatters.PayloadProcessing;
using Reqnroll.Formatters.PubSub;
using Reqnroll.Formatters.RuntimeSupport;
using Reqnroll.Utils;

namespace Reqnroll.Formatters.Messages;

/// <summary>
/// Produces a Cucumber Messages output (.ndjson) file.
/// </summary>
public class MessagesFormatterPlugin : FileWritingFormatterPluginBase
{
    FileStream? _fileStream;
    public MessagesFormatterPlugin(IFormattersConfigurationProvider configurationProvider, ICucumberMessageBroker broker, IFormatterLog logger, IFileSystem fileSystem) : base(configurationProvider, broker, logger, "messages", ".ndjson", "reqnroll_report.ndjson", fileSystem)
    {
    }
    protected override void FinalizeInitialization(string outputPath, IDictionary<string, object> formatterConfiguration, Action<bool> onInitialized)
    {
        try
        {
            _fileStream = File.Create(outputPath, TUNING_PARAM_FILE_WRITE_BUFFER_SIZE);
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
        var newLineBytes = Encoding.UTF8.GetBytes(Environment.NewLine);

        if (_fileStream == null)
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
                    await NdjsonSerializer.SerializeToStreamAsync(_fileStream, message);

                    // Write a newline after each message, except for the last one
                    if (message.TestRunFinished == null)
                        await _fileStream.WriteAsync(newLineBytes, 0, newLineBytes.Length);
                }
            }
            await _fileStream.FlushAsync();
        }
        catch (Exception e)
        {
            Logger.WriteMessage($"Formatter {Name} threw an exception: {e.Message}. No further messages will be processed.");
            throw;
        }
        finally
        {
            _fileStream?.Close();
        }
    }
}