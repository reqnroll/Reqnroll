namespace Reqnroll.Configuration
{
    public class DefaultRuntimeConfigurationProvider : IRuntimeConfigurationProvider
    {
        private readonly IConfigurationLoader _configurationLoader;

        public DefaultRuntimeConfigurationProvider(IConfigurationLoader configurationLoader)
        {
            _configurationLoader = configurationLoader;
        }

        public ReqnrollConfiguration LoadConfiguration(ReqnrollConfiguration reqnrollConfiguration)
        {
            return _configurationLoader.Load(reqnrollConfiguration);
        }
    }
}