using Reqnroll.Analytics.UserId;
using Reqnroll.CommonModels;
using Reqnroll.Configuration;
using Reqnroll.EnvironmentAccess;
using Reqnroll.Utils;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Text.Json;

namespace Reqnroll.Formatters.Configuration
{
    public class FileBasedConfigurationResolver : IFormattersConfigurationResolver
    {
        private const string FORMATTERS_KEY = "formatters";
        private IReqnrollJsonLocator _configFileLocator;
        private IFileSystem _fileSystem;
        private IFileService _fileService;

        public FileBasedConfigurationResolver(IReqnrollJsonLocator configurationFileLocator, IFileSystem fileSystem, IFileService fileService)
        {
            _configFileLocator = configurationFileLocator;
            _fileSystem = fileSystem;
            _fileService = fileService;
        }

        public IDictionary<string, string> Resolve()
        {
            var jsonOptions = new JsonSerializerOptions
            {
                PropertyNamingPolicy = JsonNamingPolicy.CamelCase, 
                ReadCommentHandling = JsonCommentHandling.Skip
            };

            var fileName = _configFileLocator.GetReqnrollJsonFilePath();

            var result = new Dictionary<string, string>();

            if (_fileSystem.FileExists(fileName))
            {
                var jsonFileContent = _fileService.ReadAllText(fileName);
                using JsonDocument reqnrollConfigDoc = JsonDocument.Parse(jsonFileContent, new JsonDocumentOptions()
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
}
