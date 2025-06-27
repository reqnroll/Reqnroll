#nullable enable

using System;
using System.Collections.Generic;
using System.IO;
using System.Text.Json;
using System.Threading.Tasks;
using Reqnroll.Formatters.Configuration;
using Reqnroll.Formatters.ExecutionTracking;
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

    protected override async Task ConsumeAndFormatMessagesBackgroundTask(IDictionary<string, object> formatterConfiguration, Action<bool> onInitialized)
    {
        var defaultBaseDirectory = ".";

        var outputFilePath = ConfiguredOutputFilePath(formatterConfiguration);
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
            Logger.WriteMessage($"Path of configured formatter output file: {outputPath} is invalid or missing. Formatter {Name} will be disabled.");
            return;
        }
        if (!_fileSystem.DirectoryExists(baseDirectory))
        {
            try
            {
                _fileSystem.CreateDirectory(baseDirectory);
            }
            catch (Exception e)
            {
                onInitialized(false);
                Logger.WriteMessage($"An exception {e.Message} occurred creating the destination directory({baseDirectory} for Formatter {Name}. The formatter will be disabled.");
                return;
            }
        }
        onInitialized(true);
        Logger.WriteMessage($"Formatter {Name} initialized to write to: {outputPath}.");
        await ConsumeAndWriteToFilesBackgroundTask(outputPath).ConfigureAwait(false);
    }

    protected abstract Task ConsumeAndWriteToFilesBackgroundTask(string outputPath);

    protected virtual string ConfiguredOutputFilePath(IDictionary<string, object> formatterConfiguration)
    {
        string outputFilePath = string.Empty;
        if (formatterConfiguration.TryGetValue("outputFilePath", out var outputPathElement))
        {
            outputFilePath = outputPathElement?.ToString() ?? string.Empty; // Ensure null-coalescing to handle possible null values.
        }
        return outputFilePath;
    }
}