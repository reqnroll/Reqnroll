using Reqnroll.UnitTestProvider;

namespace Reqnroll.Generator.Plugins
{
    public interface IGeneratorPlugin
    {
        void Initialize(GeneratorPluginEvents generatorPluginEvents, GeneratorPluginParameters generatorPluginParameters, UnitTestProviderConfiguration unitTestProviderConfiguration);
    }
}