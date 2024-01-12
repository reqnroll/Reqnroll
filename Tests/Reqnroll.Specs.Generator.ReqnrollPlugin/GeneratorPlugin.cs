using Reqnroll.Generator.Plugins;
using Reqnroll.Generator.UnitTestConverter;
using Reqnroll.Infrastructure;
using Reqnroll.Specs.Generator.ReqnrollPlugin;
using Reqnroll.UnitTestProvider;

[assembly: GeneratorPlugin(typeof(GeneratorPlugin))]


namespace Reqnroll.Specs.Generator.ReqnrollPlugin
{
    public class GeneratorPlugin : IGeneratorPlugin
    {
        public void Initialize(GeneratorPluginEvents generatorPluginEvents, GeneratorPluginParameters generatorPluginParameters, UnitTestProviderConfiguration unitTestProviderConfiguration)
        {
            //unitTestProviderConfiguration.UseUnitTestProvider("specs-multiple-configurations");
            generatorPluginEvents.RegisterDependencies += GeneratorPluginEvents_RegisterDependencies;
            generatorPluginEvents.CustomizeDependencies += GeneratorPluginEvents_CustomizeDependencies;
        }

        private void GeneratorPluginEvents_CustomizeDependencies(object sender, CustomizeDependenciesEventArgs e)
        {
            e.ObjectContainer.RegisterTypeAs<MultiFeatureGeneratorProvider, IFeatureGeneratorProvider>("specs-multiple-configurations");
        }

        private void GeneratorPluginEvents_RegisterDependencies(object sender, RegisterDependenciesEventArgs e)
        {
        }
    }
}
