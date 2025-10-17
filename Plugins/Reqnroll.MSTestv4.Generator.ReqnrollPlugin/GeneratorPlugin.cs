using Reqnroll.Generator.Plugins;
using Reqnroll.Infrastructure;
using Reqnroll.MSTestv4.Generator.ReqnrollPlugin;
using Reqnroll.UnitTestProvider;

[assembly: GeneratorPlugin(typeof(GeneratorPlugin))]

namespace Reqnroll.MSTestv4.Generator.ReqnrollPlugin;

public class GeneratorPlugin : IGeneratorPlugin
{
    public void Initialize(GeneratorPluginEvents generatorPluginEvents, GeneratorPluginParameters generatorPluginParameters, UnitTestProviderConfiguration unitTestProviderConfiguration)
    {
        unitTestProviderConfiguration.UseUnitTestProvider("mstestv4");
    }
}
