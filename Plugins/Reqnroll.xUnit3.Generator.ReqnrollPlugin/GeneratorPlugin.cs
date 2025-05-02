using Reqnroll.Generator.Plugins;
using Reqnroll.Generator.UnitTestProvider;
using Reqnroll.Infrastructure;
using Reqnroll.UnitTestProvider;

[assembly: GeneratorPlugin(typeof(Reqnroll.xUnit3.Generator.ReqnrollPlugin.GeneratorPlugin))]
namespace Reqnroll.xUnit3.Generator.ReqnrollPlugin;

public class GeneratorPlugin : IGeneratorPlugin
{
    private const string XUnit3UnitTestProviderName = "xunit3";

    public void Initialize(GeneratorPluginEvents generatorPluginEvents, GeneratorPluginParameters generatorPluginParameters, UnitTestProviderConfiguration unitTestProviderConfiguration)
    {
        unitTestProviderConfiguration.UseUnitTestProvider(XUnit3UnitTestProviderName);
        generatorPluginEvents.RegisterDependencies += GeneratorPluginEvents_RegisterDependencies;
    }

    private void GeneratorPluginEvents_RegisterDependencies(object sender, RegisterDependenciesEventArgs args)
    {
        args.ObjectContainer.RegisterTypeAs<XUnit3TestGeneratorProvider, IUnitTestGeneratorProvider>(XUnit3UnitTestProviderName);
    }
}
