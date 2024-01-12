using Reqnroll.Configuration;
using Reqnroll.Configuration.AppConfig;

namespace Reqnroll.Specs.Drivers
{
    public class XmlConfigurationParserDriver
    {
        private readonly AppConfigConfigurationLoader _appConfigConfigurationLoader;

        public XmlConfigurationParserDriver(AppConfigConfigurationLoader appConfigConfigurationLoader)
        {
            _appConfigConfigurationLoader = appConfigConfigurationLoader;
        }

        public ReqnrollConfiguration ParseReqnrollSection(string reqnrollSection)
        {
            var configSection = ConfigurationSectionHandler.CreateFromXml(reqnrollSection);

            var reqnrollConfiguration = _appConfigConfigurationLoader.LoadAppConfig(ConfigurationLoader.GetDefault(), configSection);
            return reqnrollConfiguration;
        }
    }
}
