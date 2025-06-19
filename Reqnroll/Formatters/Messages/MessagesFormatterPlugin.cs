#nullable enable

using System;
using System.IO;
using System.Text;
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
    public MessagesFormatterPlugin(IFormattersConfigurationProvider configurationProvider, ICucumberMessageBroker broker, IFormatterLog logger, IFileSystem fileSystem) : base(configurationProvider, broker, logger, "messages", ".ndjson", "reqnroll_report.ndjson", fileSystem)
    {
    }

    protected override void ConsumeAndWriteToFilesBackgroundTask(string outputPath)
    {
        var newLineBytes = Encoding.UTF8.GetBytes(Environment.NewLine);

        using var fileStream = File.Create(outputPath, TUNING_PARAM_FILE_WRITE_BUFFER_SIZE);

        foreach (var message in PostedMessages.GetConsumingEnumerable())
        {
            if (message != null)
            {
                NdjsonSerializer.SerializeToStream(fileStream, message);

                // Write a newline after each message, except for the last one
                if (message.TestRunFinished == null)
                    fileStream.Write(newLineBytes, 0, newLineBytes.Length);
            }
        }
    }
}