using System;
using System.Collections.Generic;
using System.Linq;

namespace Reqnroll.CucumberMessages.Configuration
{
    /// <summary>
    /// This class is responsible for determining the configuration of the Cucumber Messages subsystem.
    /// It is wired into the object container as a singleton and is a dependency of the PubSub classes.
    /// 
    /// When any consumer of this class asks for one of the properties of ICucumberConfiguration,
    /// the class will resolve the configuration (only once).
    /// 
    /// One or more profiles may be read from the configuration file. (FileBasedConfigurationResolver)
    /// Then Environmment Variable Overrides are applied.
    /// </summary>
    public class CucumberConfiguration : ICucumberMessagesConfiguration
    {
        public static ICucumberMessagesConfiguration Current { get; private set; }
        public bool Enabled => _runtimeEnablementOverrideFlag && _resolvedConfiguration.Value.Enabled;

        private IList<ICucumberMessagesConfigurationResolverBase> _resolvers;
        private Lazy<ResolvedConfiguration> _resolvedConfiguration;
        private IEnvVariableEnableFlagParser _envVariableEnableFlagParser;
        private bool _runtimeEnablementOverrideFlag = true;

        public CucumberConfiguration(IDictionary<string, ICucumberMessagesConfigurationResolver> resolvers, ICucumberMessagesEnvironmentOverrideConfigurationResolver environmentOverrideConfigurationResolver, IEnvVariableEnableFlagParser envVariableEnableFlagParser)
        {
            _resolvers = resolvers.Values.Cast<ICucumberMessagesConfigurationResolverBase>().ToList();
            _resolvers.Add(environmentOverrideConfigurationResolver);
            _resolvedConfiguration = new Lazy<ResolvedConfiguration>(ResolveConfiguration);
            _envVariableEnableFlagParser = envVariableEnableFlagParser;
            Current = this;
        }

        #region Override API
        public void SetEnabled(bool value)
        {
            _runtimeEnablementOverrideFlag = value;
        }
        #endregion

        public string GetFormatterConfigurationByName(string formatterName)
        {
            var config = _resolvedConfiguration.Value;
            if (config.Formatters.TryGetValue(formatterName, out var formatterConfig))
                return formatterConfig;
            else return String.Empty;
        }

        private ResolvedConfiguration ResolveConfiguration()
        {
            var combinedConfig = new Dictionary<string, string>();

            foreach (var resolver in _resolvers)
            {
                foreach (var entry in resolver.Resolve())
                {
                    combinedConfig[entry.Key] = entry.Value;
                }
            }
            bool enabled = combinedConfig.Count > 0 && _envVariableEnableFlagParser.Parse();

            return new ResolvedConfiguration
            {
                Formatters = combinedConfig,
                Enabled = enabled
            };
        }
    }
}

