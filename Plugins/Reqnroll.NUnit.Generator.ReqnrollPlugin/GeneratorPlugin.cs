using Reqnroll.Generator.Plugins;
using Reqnroll.Infrastructure;
using Reqnroll.UnitTestProvider;

[assembly:GeneratorPlugin(typeof(Reqnroll.NUnit.Generator.ReqnrollPlugin.GeneratorPlugin))]

namespace Reqnroll.NUnit.Generator.ReqnrollPlugin
{
    public class GeneratorPlugin : IGeneratorPlugin
    {
        public void Initialize(GeneratorPluginEvents generatorPluginEvents, GeneratorPluginParameters generatorPluginParameters, UnitTestProviderConfiguration unitTestProviderConfiguration)
        {
            unitTestProviderConfiguration.UseUnitTestProvider("nunit");
        }
    }
}
