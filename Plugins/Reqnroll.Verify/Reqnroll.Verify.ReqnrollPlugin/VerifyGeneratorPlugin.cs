using Reqnroll.Verify.ReqnrollPlugin;
using Reqnroll.Generator.Plugins;
using Reqnroll.Generator.UnitTestConverter;
using Reqnroll.Infrastructure;
using Reqnroll.UnitTestProvider;

[assembly:GeneratorPlugin(typeof(VerifyGeneratorPlugin))]

namespace Reqnroll.Verify.ReqnrollPlugin
{
    public class VerifyGeneratorPlugin : IGeneratorPlugin
    {
        public void Initialize(GeneratorPluginEvents generatorPluginEvents, GeneratorPluginParameters generatorPluginParameters,
            UnitTestProviderConfiguration unitTestProviderConfiguration)
        {
            generatorPluginEvents.RegisterDependencies += (sender, args) =>
            {
                args.ObjectContainer.RegisterTypeAs<VerifyDecorator, ITestClassDecorator>("verify");
            };
        }
    }
}
