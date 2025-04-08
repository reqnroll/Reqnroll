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
        public IDictionary<string, string> GetConfiguration()
        {
            return new Dictionary<string, string>();
        }
    }
}
