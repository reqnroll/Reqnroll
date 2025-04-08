using Reqnroll.CommonModels;
using Reqnroll.Configuration;
using Reqnroll.EnvironmentAccess;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Text.Json;

namespace Reqnroll.CucumberMessages.Configuration
{
    public class ConfigFile_ConfigurationSource : IConfigurationSource
    {
        private const string FORMATTERS_KEY = "formatters";
        private IReqnrollJsonLocator _configFileLocator;

        public ConfigFile_ConfigurationSource(IReqnrollJsonLocator configurationFileLocator)
        {
            _configFileLocator = configurationFileLocator;
        }

        public IDictionary<string, string> GetConfiguration()
        {
            var jsonOptions = new JsonSerializerOptions
            {
                PropertyNamingPolicy = JsonNamingPolicy.CamelCase, 
                ReadCommentHandling = JsonCommentHandling.Skip
            };

            var fileName = _configFileLocator.GetReqnrollJsonFilePath();

            var result = new Dictionary<string, string>();

            if (File.Exists(fileName))
            {
                var jsonFileContent = File.ReadAllText(fileName);
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
