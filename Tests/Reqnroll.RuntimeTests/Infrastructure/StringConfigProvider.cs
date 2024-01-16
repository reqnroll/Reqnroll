using System;
using Reqnroll.Configuration;
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

            throw new NotSupportedException("app.config tests are not supported");
        }
    }
}