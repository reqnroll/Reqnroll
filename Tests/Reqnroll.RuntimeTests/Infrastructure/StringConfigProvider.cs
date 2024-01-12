using System;
using System.Xml;
using Reqnroll.Configuration;
using Reqnroll.Configuration.AppConfig;
using Reqnroll.Configuration.JsonConfig;

namespace Reqnroll.RuntimeTests.Infrastructure
{
    internal class StringConfigProvider : IRuntimeConfigurationProvider
    {
        private readonly string configFileContent;

        private bool IsJsonConfig => configFileContent != null && configFileContent.StartsWith("{");

        public StringConfigProvider(string configContent)
        {
            configFileContent = configContent;
        }

        public ReqnrollConfiguration LoadConfiguration(ReqnrollConfiguration reqnrollConfiguration)
        {
            if (IsJsonConfig)
            {
                var jsonConfigurationLoader = new JsonConfigurationLoader();

                return jsonConfigurationLoader.LoadJson(reqnrollConfiguration, configFileContent);
            }

            ConfigurationSectionHandler section = GetSection();

            var runtimeConfigurationLoader = new AppConfigConfigurationLoader();

            return runtimeConfigurationLoader.LoadAppConfig(reqnrollConfiguration, section);
        }

        private ConfigurationSectionHandler GetSection()
        {
            XmlDocument configDocument = new XmlDocument();
            configDocument.LoadXml(configFileContent);

            var reqnrollNode = configDocument.SelectSingleNode("/configuration/reqnroll");
            if (reqnrollNode == null)
                throw new InvalidOperationException("invalid config file content");

            return ConfigurationSectionHandler.CreateFromXml(reqnrollNode);
        }
    }
}