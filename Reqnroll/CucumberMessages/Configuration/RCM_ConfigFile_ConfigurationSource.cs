using Reqnroll.CommonModels;
using Reqnroll.EnvironmentAccess;
using System.IO;
using System.Text.Json;

namespace Reqnroll.CucumberMessages.Configuration
{
    public class RCM_ConfigFile_ConfigurationSource : IConfigurationSource
    {
        private const string CUCUMBERMESSAGESCONFIGURATIONFILENAME = "cucumberMessages.configuration.json";
        private IEnvironmentWrapper _environmentWrapper;

        public RCM_ConfigFile_ConfigurationSource(IEnvironmentWrapper environmentWrapper)
        {
            _environmentWrapper = environmentWrapper;
        }

        public ConfigurationDTO GetConfiguration()
        {
            var jsonOptions = new JsonSerializerOptions { PropertyNamingPolicy = JsonNamingPolicy.CamelCase };
            jsonOptions.Converters.Add(new IdGenerationStyleEnumConverter());

            var fileNameOverridden = _environmentWrapper.GetEnvironmentVariable(CucumberConfigurationConstants.REQNROLL_CUCUMBER_MESSAGES_CONFIGURATION_FILE_OVERRIDE_ENVIRONMENT_VARIABLE);

            var fileName = fileNameOverridden is Success<string> ? ((Success<string>)fileNameOverridden).Result : CUCUMBERMESSAGESCONFIGURATIONFILENAME;

            ConfigurationDTO configurationDTO = null;
            if (File.Exists(fileName))
            {
                configurationDTO = JsonSerializer.Deserialize<ConfigurationDTO>(File.ReadAllText(fileName), jsonOptions);
            }
            return configurationDTO;
        }
    }
}
