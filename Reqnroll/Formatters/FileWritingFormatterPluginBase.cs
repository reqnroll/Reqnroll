#nullable enable

using System;
using System.IO;
using Reqnroll.Formatters.Configuration;
using Reqnroll.Formatters.PubSub;
using Reqnroll.Formatters.RuntimeSupport;
using Reqnroll.Utils;

namespace Reqnroll.Formatters;

/// <summary>
/// Formatter plugin base that receives Cucumber messages and let the implementor process them and generate a single output file.
/// </summary>
public abstract class FileWritingFormatterPluginBase : FormatterPluginBase
{
    private readonly string _defaultFileType;
    private readonly string _defaultFileName;
    private readonly IFileSystem _fileSystem;

    protected FileWritingFormatterPluginBase(IFormattersConfigurationProvider configurationProvider, ICucumberMessageBroker broker, IFormatterLog logger, string pluginName, string defaultFileType, string defaultFileName, IFileSystem fileSystem) : base(configurationProvider, broker, logger, pluginName)
    {
        _defaultFileType = defaultFileType;
        _defaultFileName = defaultFileName;
        _fileSystem = fileSystem;
    }

    protected const int TUNING_PARAM_FILE_WRITE_BUFFER_SIZE = 65536;

    protected override void ConsumeAndFormatMessagesBackgroundTask(string formatterConfigurationString, Action<bool> onInitialized)
    {
        var defaultBaseDirectory = ".";

        var outputFilePath = ParseConfiguredOutputFilePath(formatterConfigurationString);
        var fileName = Path.GetFileName(outputFilePath);
        var baseDirectory = Path.GetDirectoryName(outputFilePath) ?? "";

        if (baseDirectory.IsNullOrEmpty())
            baseDirectory = defaultBaseDirectory;

        if (fileName.IsNullOrEmpty())
            fileName = _defaultFileName;

        if (!fileName.EndsWith(_defaultFileType, StringComparison.OrdinalIgnoreCase))
        {
            fileName += _defaultFileType;
        }

        var outputPath = Path.Combine(baseDirectory, fileName);
        if (!FileFilter.IsValidFile(outputPath))
        {
            onInitialized(false);
            throw new InvalidOperationException($"Path of configured formatter output file: {outputPath} is invalid or missing.");
        }

        if (!_fileSystem.DirectoryExists(baseDirectory))
        {
            _fileSystem.CreateDirectory(baseDirectory);
        }

        onInitialized(true);
        ConsumeAndWriteToFilesBackgroundTask(outputPath);
    }

    protected abstract void ConsumeAndWriteToFilesBackgroundTask(string outputPath);

    protected virtual string ParseConfiguredOutputFilePath(string formatterConfiguration)
    {
        if (string.IsNullOrWhiteSpace(formatterConfiguration))
            return string.Empty;

        try
        {
            using var document = System.Text.Json.JsonDocument.Parse(formatterConfiguration);
            if (document.RootElement.TryGetProperty("outputFilePath", out var outputFilePathElement))
            {
                return outputFilePathElement.GetString() ?? string.Empty;
            }
        }
        catch (System.Text.Json.JsonException)
        {
            Trace?.WriteMessage($"Configuration of ${PluginName} is invalid: ${formatterConfiguration}");
        }

        return string.Empty;
    }
}