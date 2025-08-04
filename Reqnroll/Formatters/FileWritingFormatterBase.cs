﻿#nullable enable

using Io.Cucumber.Messages.Types;
using Reqnroll.Formatters.Configuration;
using Reqnroll.Formatters.RuntimeSupport;
using Reqnroll.Utils;
using System;
using System.Collections.Generic;
using System.IO;
using System.Threading;
using System.Threading.Tasks;

namespace Reqnroll.Formatters;

/// <summary>
/// Formatter plugin base that receives Cucumber messages and let the implementor process them and generate a single output file.
/// </summary>
public abstract class FileWritingFormatterBase : FormatterBase
{
    private readonly string _defaultFileExtension;
    private readonly string _defaultFileName;
    private readonly IFileSystem _fileSystem;
    protected Stream? TargetFileStream { get; private set; } = null;

    protected FileWritingFormatterBase(
        IFormattersConfigurationProvider configurationProvider,
        IFormatterLog logger,
        IFileSystem fileSystem,
        string pluginName,
        string defaultFileExtension,
        string defaultFileName) : base(configurationProvider, logger, pluginName)
    {
        _defaultFileExtension = defaultFileExtension;
        _defaultFileName = defaultFileName;
        _fileSystem = fileSystem;
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

        if (Path.GetExtension(fileName).IsNullOrEmpty())
        {
            fileName += _defaultFileExtension;
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
            catch (System.Exception e)
            {
                onInitialized(false);
                Logger.WriteMessage($"An exception {e.Message} occurred creating the destination directory({baseDirectory} for Formatter {Name}. The formatter will be disabled.");
                return;
            }
        }

        FinalizeInitialization(outputPath, formatterConfiguration, onInitialized);
        Logger.WriteMessage($"Formatter {Name} initialized to write to: {outputPath}.");
    }

    protected override async Task ConsumeAndFormatMessagesBackgroundTask(CancellationToken cancellationToken)
    {
        if (TargetFileStream == null)
        {
            Logger.WriteMessage($"Formatter {Name} closing because the filestream is not open.");
            return;
        }
        try
        {
            await foreach (var message in PostedMessages.Reader.ReadAllAsync(cancellationToken))
            {
                if (cancellationToken.IsCancellationRequested)
                {
                    Logger.WriteMessage($"Formatter {Name} has been cancelled.");
                    break;
                }
                await WriteToFile(message, cancellationToken);                
            }
        }
        catch (OperationCanceledException)
        {
            Logger.WriteMessage($"Formatter {Name} has been cancelled.");
            await OnCancellation();
        }
        catch (System.Exception e)
        {
            Logger.WriteMessage($"Formatter {Name} threw an exception: {e.Message}. No further messages will be processed.");
            throw;
        }
        finally
        {
            Closed = true;
            try
            {
                await FlushTargetFileStream(cancellationToken);
            }
            catch (System.Exception e)
            {
                Logger.WriteMessage($"Formatter {Name} file stream flush threw an exception: {e.Message}.");
            }
        }
    }

    protected virtual void FinalizeInitialization(string outputPath, IDictionary<string, object> formatterConfiguration, Action<bool> onInitialized)
    {
        try
        {
            TargetFileStream = CreateTargetFileStream(outputPath);
            OnTargetFileStreamInitialized(TargetFileStream);
            onInitialized(true);
            Logger.WriteMessage($"Formatter {Name} opened file stream.");
        }
        catch
        {
            Logger.WriteMessage($"Formatter {Name} closing because of an exception opening the file stream.");
            onInitialized(false);
        }
    }

    protected virtual Stream CreateTargetFileStream(string outputPath) => 
        File.Create(outputPath, TUNING_PARAM_FILE_WRITE_BUFFER_SIZE);

    protected abstract void OnTargetFileStreamInitialized(Stream targetFileStream);
    protected abstract void OnTargetFileStreamDisposing();
    protected abstract Task WriteToFile(Envelope envelope, CancellationToken cancellationToken);

    protected virtual string ConfiguredOutputFilePath(IDictionary<string, object> formatterConfiguration)
    {
        string outputFilePath = string.Empty;
        if (formatterConfiguration.TryGetValue("outputFilePath", out var outputPathElement))
        {
            outputFilePath = outputPathElement?.ToString() ?? string.Empty; // Ensure null-coalescing to handle possible null values.
        }
        return outputFilePath;
    }

    protected virtual Task OnCancellation() => Task.CompletedTask;

    protected virtual async Task FlushTargetFileStream(CancellationToken cancellationToken)
    {
        if (TargetFileStream != null)
            await TargetFileStream.FlushAsync(cancellationToken);
    }

    private void DisposeFileStream()
    {
        OnTargetFileStreamDisposing();
        TargetFileStream?.Close();
        TargetFileStream?.Dispose();
    }

    public override void Dispose()
    {
        DisposeFileStream();
        base.Dispose();
    }
}