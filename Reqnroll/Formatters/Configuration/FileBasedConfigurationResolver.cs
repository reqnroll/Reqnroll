using Reqnroll.Analytics.UserId;
using Reqnroll.Configuration;
using Reqnroll.Utils;
using System.Collections.Generic;
using System.Text.Json;

namespace Reqnroll.Formatters.Configuration;

public class FileBasedConfigurationResolver : IFormattersConfigurationResolver
{
    private const string FORMATTERS_KEY = "formatters";
    private readonly IReqnrollJsonLocator _configFileLocator;
    private readonly IFileSystem _fileSystem;
    private readonly IFileService _fileService;

    public FileBasedConfigurationResolver(IReqnrollJsonLocator configurationFileLocator, IFileSystem fileSystem, IFileService fileService)
    {
        _configFileLocator = configurationFileLocator;
        _fileSystem = fileSystem;
        _fileService = fileService;
    }

    public IDictionary<string, string> Resolve()
    {
        var fileName = _configFileLocator.GetReqnrollJsonFilePath();

        var result = new Dictionary<string, string>();

        if (_fileSystem.FileExists(fileName))
        {
            var jsonFileContent = _fileService.ReadAllText(fileName);
            using JsonDocument reqnrollConfigDoc = JsonDocument.Parse(jsonFileContent, new JsonDocumentOptions
            {
                CommentHandling = JsonCommentHandling.Skip
            });
            if (reqnrollConfigDoc.RootElement.TryGetProperty(FORMATTERS_KEY, out JsonElement formatters))
            {
                foreach(JsonProperty jsonProperty in formatters.EnumerateObject())
                {
                    result.Add(jsonProperty.Name, jsonProperty.Value.GetRawText());
                }
            }
        }
        return result;
    }
}