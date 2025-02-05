using Reqnroll.EnvironmentAccess;
using System;
using System.Collections.Generic;
using System.Text;

namespace Reqnroll.CucumberMessages.Configuration
{

    /// <summary>
    /// Defaults are:
    /// - FileOutputEnabled = false
    /// </summary>
    internal class DefaultConfigurationSource : IConfigurationSource
    {
        private readonly IEnvironmentWrapper _environmentWrapper;

        public DefaultConfigurationSource(IEnvironmentWrapper environmentWrapper)
        {
            _environmentWrapper = environmentWrapper;
        }
        public ConfigurationDTO GetConfiguration()
        {
            var res = new ConfigurationDTO();
            string defaultOutputDirectory = _environmentWrapper.GetCurrentDirectory();
            string defaultOutputFileName = "reqnroll_report.ndjson";

            res.Enabled = false;
            res.BaseDirectory = defaultOutputDirectory;
            res.OutputDirectory = String.Empty;
            res.OutputFileName = defaultOutputFileName;
            res.IDGenerationStyle = IDGenerationStyle.UUID;
            return res;
        }
    }
}
