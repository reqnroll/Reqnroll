using Reqnroll.Configuration;
using Reqnroll.Configuration.JsonConfig;

namespace Reqnroll.Specs.Drivers
{
    public class JsonConfigurationParserDriver
    {
        private readonly JsonConfigurationLoader _jsonConfigurationLoader;

        public JsonConfigurationParserDriver(JsonConfigurationLoader jsonConfigurationLoader)
        {
            _jsonConfigurationLoader = jsonConfigurationLoader;
        }

        public ReqnrollConfiguration ParseReqnrollSection(string reqnrollJsonConfig)
        {
            var reqnrollConfiguration = _jsonConfigurationLoader.LoadJson(ConfigurationLoader.GetDefault(), reqnrollJsonConfig);
            return reqnrollConfiguration;
        }
    }
}
