using Reqnroll.Analytics.UserId;
using Reqnroll.Configuration;
using Reqnroll.Formatters.RuntimeSupport;
using Reqnroll.Utils;
using System;
using System.Text.Json;

namespace Reqnroll.Formatters.Configuration;

public class FileBasedConfigurationResolver : FormattersConfigurationResolverBase, IFileBasedConfigurationResolver
{
    private readonly IReqnrollJsonLocator _configFileLocator;
    private readonly IFileSystem _fileSystem;
    private readonly IFileService _fileService;
    private readonly IFormatterLog _log;

    public FileBasedConfigurationResolver(
        IReqnrollJsonLocator configurationFileLocator,
        IFileSystem fileSystem,
        IFileService fileService,
        IFormatterLog log = null)
    {
        _configFileLocator = configurationFileLocator;
        _fileSystem = fileSystem;
        _fileService = fileService;
        _log = log;
    }

    protected override JsonDocument GetJsonDocument()
    {
        try
        {
            string fileName;
            try
            {
                fileName = _configFileLocator.GetReqnrollJsonFilePath();
            }
            catch (Exception ex)
            {
                _log?.WriteMessage($"Failed to locate Reqnroll JSON file: {ex.Message}");
                return null;
            }

            if (string.IsNullOrWhiteSpace(fileName))
            {
                _log?.WriteMessage("Reqnroll JSON file path is empty");
                return null;
            }

            if (!_fileSystem.FileExists(fileName))
            {
                // This is not necessarily an error, could be a new project without a config file yet
                _log?.WriteMessage($"Reqnroll JSON file not found at: {fileName}");
                return null;
            }

            string jsonFileContent;
            try
            {
                jsonFileContent = _fileService.ReadAllText(fileName);
            }
            catch (Exception ex)
            {
                _log?.WriteMessage($"Failed to read Reqnroll JSON file '{fileName}': {ex.Message}");
                return null;
            }

            if (string.IsNullOrWhiteSpace(jsonFileContent))
            {
                _log?.WriteMessage($"Reqnroll JSON file '{fileName}' is empty");
                return null;
            }

            try
            {
                return JsonDocument.Parse(jsonFileContent, new JsonDocumentOptions
                {
                    CommentHandling = JsonCommentHandling.Skip,
                    AllowTrailingCommas = true // More lenient parsing
                });
            }
            catch (JsonException ex)
            {
                _log?.WriteMessage($"Failed to parse JSON from file '{fileName}': {ex.Message}");
                return null;
            }
        }
        catch (Exception ex)
        {
            // Catch any other unexpected exceptions
            _log?.WriteMessage($"Unexpected error processing configuration file: {ex.Message}");
            return null;
        }
    }
}