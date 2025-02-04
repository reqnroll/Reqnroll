using Reqnroll.Generator.Plugins;
using Reqnroll.Infrastructure;
using Reqnroll.UnitTestProvider;

[assembly:GeneratorPlugin(typeof(Reqnroll.xUnit.Generator.ReqnrollPlugin.GeneratorPlugin))]

namespace Reqnroll.xUnit.Generator.ReqnrollPlugin
{
    public class GeneratorPlugin : IGeneratorPlugin
    {
        public void Initialize(GeneratorPluginEvents generatorPluginEvents, GeneratorPluginParameters generatorPluginParameters, UnitTestProviderConfiguration unitTestProviderConfiguration)
        {
            unitTestProviderConfiguration.UseUnitTestProvider("xunit"); //todo make a constant
        }
    }
}
