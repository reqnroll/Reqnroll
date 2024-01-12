using System.Collections.Generic;
using Reqnroll.Configuration;
using Reqnroll.Generator.Interfaces;
using Reqnroll.Plugins;

namespace Reqnroll.Generator.Configuration
{
    public class GeneratorConfigurationProvider : IGeneratorConfigurationProvider
    {
        private readonly IConfigurationLoader _configurationLoader;

        public GeneratorConfigurationProvider(IConfigurationLoader configurationLoader)
        {
            _configurationLoader = configurationLoader;
        }

        public virtual ReqnrollConfiguration LoadConfiguration(ReqnrollConfiguration reqnrollConfiguration, ReqnrollConfigurationHolder reqnrollConfigurationHolder)
        {
            return _configurationLoader.Load(reqnrollConfiguration, reqnrollConfigurationHolder);
        }

        public ReqnrollConfiguration LoadConfiguration(ReqnrollConfiguration reqnrollConfiguration)
        {
            return _configurationLoader.Load(reqnrollConfiguration);
        }
        
        internal virtual void UpdateConfiguration(ReqnrollProjectConfiguration configuration, ConfigurationSectionHandler reqnrollConfigSection)
        {
            configuration.ReqnrollConfiguration = _configurationLoader.Update(configuration.ReqnrollConfiguration, reqnrollConfigSection);
        }

    }
}