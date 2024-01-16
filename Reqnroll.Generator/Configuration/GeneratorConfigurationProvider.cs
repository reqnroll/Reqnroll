using Reqnroll.Configuration;
using Reqnroll.Generator.Interfaces;

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
    }
}