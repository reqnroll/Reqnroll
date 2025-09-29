using Reqnroll.Configuration;
using Reqnroll.Generator.Interfaces;

namespace Reqnroll.Generator.Configuration;

public static class ConfigurationLoaderExtensions
{
    public static ReqnrollProjectConfiguration Load(this IConfigurationLoader configurationLoader, ReqnrollConfigurationHolder configurationHolder)
    {
        ReqnrollProjectConfiguration configuration = new ReqnrollProjectConfiguration();
        configuration.ReqnrollConfiguration = configurationLoader.Load(configuration.ReqnrollConfiguration, configurationHolder);

        return configuration;
    }
}