using System.Collections.Generic;
using Reqnroll.Configuration;
using Reqnroll.Generator.Interfaces;
using Reqnroll.Plugins;

namespace Reqnroll.Generator.Configuration
{
    public interface IGeneratorConfigurationProvider
    {
        ReqnrollConfiguration LoadConfiguration(ReqnrollConfiguration reqnrollConfiguration, ReqnrollConfigurationHolder reqnrollConfigurationHolder);
        ReqnrollConfiguration LoadConfiguration(ReqnrollConfiguration reqnrollConfiguration);
    }

    public static class GeneratorConfigurationProviderExtensions
    {
        public static ReqnrollProjectConfiguration LoadConfiguration(this IGeneratorConfigurationProvider configurationProvider, ReqnrollConfigurationHolder configurationHolder)
        {
            ReqnrollProjectConfiguration configuration = new ReqnrollProjectConfiguration();
            configuration.ReqnrollConfiguration = configurationProvider.LoadConfiguration(configuration.ReqnrollConfiguration, configurationHolder);

            return configuration;
        }
    }
}
