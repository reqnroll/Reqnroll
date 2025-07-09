using Reqnroll.Generator.Plugins;
using Reqnroll.Generator.UnitTestProvider;
using Reqnroll.Infrastructure;
using Reqnroll.UnitTestProvider;

[assembly: GeneratorPlugin(typeof(Reqnroll.TUnit.Generator.ReqnrollPlugin.GeneratorPlugin))]

namespace Reqnroll.TUnit.Generator.ReqnrollPlugin
{
    public class GeneratorPlugin : IGeneratorPlugin
    {
        private const string TUnit = "tunit";

        public void Initialize(
            GeneratorPluginEvents generatorPluginEvents,
            GeneratorPluginParameters generatorPluginParameters,
            UnitTestProviderConfiguration unitTestProviderConfiguration)
        {
            generatorPluginEvents.RegisterDependencies += (sender, args) =>
            {
                args.ObjectContainer.RegisterTypeAs<TUnitTestGeneratorProvider, IUnitTestGeneratorProvider>(TUnit);
            };

            unitTestProviderConfiguration.UseUnitTestProvider(TUnit);
        }
    }
}
