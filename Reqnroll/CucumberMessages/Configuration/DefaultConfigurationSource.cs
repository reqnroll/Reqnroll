using Reqnroll.EnvironmentAccess;
using System;
using System.Collections.Generic;
using System.Text;

namespace Reqnroll.CucumberMessages.Configuration
{
    internal class DefaultConfigurationSource : IConfigurationSource
    {
        private IEnvironmentWrapper _environmentWrapper;

        public DefaultConfigurationSource(IEnvironmentWrapper environmentWrapper)
        {
            _environmentWrapper = environmentWrapper;
        }
        public ConfigurationDTO GetConfiguration()
        {
            var res = new ConfigurationDTO();
            string defaultOutputDirectory = _environmentWrapper.GetCurrentDirectory();
            string defaultOutputFileName = "reqnroll_report.ndjson";

            var defaultProfile = new Profile("DEFAULT", defaultOutputDirectory, string.Empty, defaultOutputFileName, IDGenerationStyle.UUID);
            res.FileOutputEnabled = false;
            res.Profiles.Add(defaultProfile);
            return res;
        }
    }
}
