#nullable enable

using System;
using System.Collections.Generic;
using System.IO;
using System.Threading;
using System.Threading.Tasks;
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
    private string _outputPath;

    protected FileWritingFormatterPluginBase(IFormattersConfigurationProvider configurationProvider, IFormatterLog logger, string pluginName, string defaultFileType, string defaultFileName, IFileSystem fileSystem) : base(configurationProvider, logger, pluginName)
    {
        _defaultFileType = defaultFileType;
        _defaultFileName = defaultFileName;
        _fileSystem = fileSystem;
        _outputPath = String.Empty;
    }

    protected const int TUNING_PARAM_FILE_WRITE_BUFFER_SIZE = 65536;

    public override void LaunchInner(IDictionary<string, object> formatterConfiguration, Action<bool> onInitialized)
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

        _outputPath = Path.Combine(baseDirectory, fileName);
        if (!FileFilter.IsValidFile(_outputPath))
        {
            onInitialized(false);
            Logger.WriteMessage($"Path of configured formatter output file: {_outputPath} is invalid or missing. Formatter {Name} will be disabled.");
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

        FinalizeInitialization(_outputPath, formatterConfiguration, onInitialized);
        Logger.WriteMessage($"Formatter {Name} initialized to write to: {_outputPath}.");
    }
    protected override async Task ConsumeAndFormatMessagesBackgroundTask(CancellationToken cancellationToken)
    {
        await ConsumeAndWriteToFilesBackgroundTask(_outputPath, cancellationToken).ConfigureAwait(false);
    }
    protected abstract void FinalizeInitialization(string outputPath, IDictionary<string, object> formatterConfiguration, Action<bool> onInitialized);
    protected abstract Task ConsumeAndWriteToFilesBackgroundTask(string outputPath, CancellationToken cancellationToken);

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