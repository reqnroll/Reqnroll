using System.IO;
using System.Text.Json;

namespace Reqnroll.CucumberMessages.Configuration
{
    internal class RCM_ConfigFile_ConfigurationSource : IConfigurationSource
    {
        private const string CUCUMBERMESSAGESCONFIGURATIONFILENAME = "CucumberMessages.configuration.json";

        public ConfigurationDTO GetConfiguration()
        {
            var jsonOptions = new JsonSerializerOptions { PropertyNamingPolicy = JsonNamingPolicy.CamelCase };
            jsonOptions.Converters.Add(new IdGenerationStyleEnumConverter());

            ConfigurationDTO configurationDTO = null;
            if (File.Exists(CUCUMBERMESSAGESCONFIGURATIONFILENAME))
            {
                configurationDTO = JsonSerializer.Deserialize<ConfigurationDTO>(File.ReadAllText(CUCUMBERMESSAGESCONFIGURATIONFILENAME), 
                                        jsonOptions);
            }
            return configurationDTO;
        }
    }
}
