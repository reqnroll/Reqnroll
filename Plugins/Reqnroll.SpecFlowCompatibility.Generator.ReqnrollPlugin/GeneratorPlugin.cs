using Reqnroll.Configuration;
using Reqnroll.Generator.Plugins;
using Reqnroll.Infrastructure;
using Reqnroll.SpecFlowCompatibility.ReqnrollPlugin;
using Reqnroll.UnitTestProvider;

[assembly: GeneratorPlugin(typeof(GeneratorPlugin))]

// ReSharper disable once CheckNamespace
namespace Reqnroll.SpecFlowCompatibility.ReqnrollPlugin;

public class GeneratorPlugin : IGeneratorPlugin
{
    public void Initialize(GeneratorPluginEvents generatorPluginEvents, GeneratorPluginParameters generatorPluginParameters, UnitTestProviderConfiguration unitTestProviderConfiguration)
    {
        generatorPluginEvents.ConfigurationDefaults += (_, args) =>
        {
            var configuration = args.ReqnrollProjectConfiguration.ReqnrollConfiguration;
            if (configuration.ConfigSource == ConfigSource.AppConfig && configuration.ConfigSourceText != null)
            {
                var configSection = ConfigurationSectionHandler.CreateFromXml(configuration.ConfigSourceText);
                var loader = new AppConfig.AppConfigConfigurationLoader();
                loader.UpdateFromAppConfig(configuration, configSection);
            }
        };
    }
}