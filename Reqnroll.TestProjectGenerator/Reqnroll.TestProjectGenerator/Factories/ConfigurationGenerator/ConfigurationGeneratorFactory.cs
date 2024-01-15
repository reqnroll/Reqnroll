using System;

namespace Reqnroll.TestProjectGenerator.Factories.ConfigurationGenerator
{
    public class ConfigurationGeneratorFactory
    {
        private readonly AppConfigGenerator _appConfigGenerator;
        private readonly JsonConfigGenerator _jsonConfigGenerator;
        private readonly NoneConfigGenerator _noneConfigGenerator;

        public ConfigurationGeneratorFactory(AppConfigGenerator appConfigGenerator, JsonConfigGenerator jsonConfigGenerator, NoneConfigGenerator noneConfigGenerator)
        {
            _appConfigGenerator = appConfigGenerator;
            _jsonConfigGenerator = jsonConfigGenerator;
            _noneConfigGenerator = noneConfigGenerator;
        }
        public IConfigurationGenerator FromConfigurationFormat(ConfigurationFormat configurationFormat)
        {
            switch (configurationFormat)
            {
                case ConfigurationFormat.Config: return _appConfigGenerator;
                case ConfigurationFormat.Json: return _jsonConfigGenerator;
                case ConfigurationFormat.None: return _noneConfigGenerator;
                default: throw new ArgumentOutOfRangeException(nameof(configurationFormat));
            }
        }
    }
}