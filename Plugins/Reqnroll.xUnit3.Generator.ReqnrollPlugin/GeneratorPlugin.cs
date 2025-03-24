using Reqnroll.Generator.Plugins;
using Reqnroll.Infrastructure;
using Reqnroll.UnitTestProvider;

[assembly:GeneratorPlugin(typeof(Reqnroll.xUnit3.Generator.ReqnrollPlugin.GeneratorPlugin))]

namespace Reqnroll.xUnit3.Generator.ReqnrollPlugin;

public class GeneratorPlugin: IGeneratorPlugin
{
    public void Initialize(GeneratorPluginEvents generatorPluginEvents, GeneratorPluginParameters generatorPluginParameters, UnitTestProviderConfiguration unitTestProviderConfiguration)
    {
        unitTestProviderConfiguration.UseUnitTestProvider(XUnit3UnitTestProviderName);
    }

    private const string XUnit3UnitTestProviderName = "xunit3";
}
