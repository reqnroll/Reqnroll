using Reqnroll.CommonModels;
using Reqnroll.Configuration;
using Reqnroll.EnvironmentAccess;
using System.Diagnostics;
using System.IO;
using System.Text.Json;

namespace Reqnroll.CucumberMessages.Configuration
{
    public class ConfigFile_ConfigurationSource : IConfigurationSource
    {
        private IReqnrollJsonLocator _configFileLocator;

        public ConfigFile_ConfigurationSource(IReqnrollJsonLocator configurationFileLocator)
        {
            _configFileLocator = configurationFileLocator;
        }

        public ConfigurationDTO GetConfiguration()
        {
            var jsonOptions = new JsonSerializerOptions { PropertyNamingPolicy = JsonNamingPolicy.CamelCase };
            jsonOptions.Converters.Add(new IdGenerationStyleEnumConverter());

            var fileName = _configFileLocator.GetReqnrollJsonFilePath();

            ConfigurationDTO configurationDTO = new();
            CucumberMessagesConfiguration section = null;

            if (File.Exists(fileName))
            {
                var jsonFileContent = File.ReadAllText(fileName);
                using JsonDocument reqnrollConfigDoc = JsonDocument.Parse(jsonFileContent);
                if (reqnrollConfigDoc.RootElement.TryGetProperty("cucumberMessagesConfiguration", out JsonElement CMC))
                {
                    section = JsonSerializer.Deserialize<CucumberMessagesConfiguration>(CMC.GetRawText(), jsonOptions);
                }
            }
            if (section != null)
            {
                configurationDTO.Enabled = section.Enabled;
                configurationDTO.BaseDirectory = section.BaseDirectory;
                configurationDTO.OutputDirectory = section.OutputDirectory;
                configurationDTO.OutputFileName = section.OutputFileName;
                configurationDTO.IDGenerationStyle = section.IDGenerationStyle;
                return configurationDTO;
            }
            return null;
        }
    }
}
