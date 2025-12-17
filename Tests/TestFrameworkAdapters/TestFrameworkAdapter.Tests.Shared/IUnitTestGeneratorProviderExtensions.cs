using Reqnroll.Configuration;
using Reqnroll.Generator;
using Reqnroll.Generator.CodeDom;
using Reqnroll.Generator.Generation;
using Reqnroll.Generator.Interfaces;
using Reqnroll.Generator.UnitTestConverter;
using Reqnroll.Generator.UnitTestProvider;
using Reqnroll.GeneratorTests;

namespace Reqnroll.xUnit3.Generator.ReqnrollPluginTests
{
    public static class IUnitTestGeneratorProviderExtensions
    {
        extension(IUnitTestGeneratorProvider testGeneratorProvider)
        {
            public UnitTestFeatureGenerator CreateUnitTestConverter(bool disableFriendlyTestNames = ConfigDefaults.DisableFriendlyTestNames)
            {
                var codeDomHelper = new CodeDomHelper(CodeDomProviderLanguage.CSharp);

                var runtimeConfiguration = ConfigurationLoader.GetDefault();
                runtimeConfiguration.AllowRowTests = true;
                runtimeConfiguration.AllowDebugGeneratedFiles = true;
                runtimeConfiguration.DisableFriendlyTestNames = disableFriendlyTestNames;

                return new UnitTestFeatureGenerator(testGeneratorProvider, codeDomHelper, runtimeConfiguration, new DecoratorRegistryStub());
            }

            public IFeatureGenerator CreateFeatureGenerator(string[]? addNonParallelizableMarkerForTags = null)
            {
                var container = new GeneratorContainerBuilder().CreateContainer(new ReqnrollConfigurationHolder(ConfigSource.Default, null), new ProjectSettings(), Enumerable.Empty<GeneratorPluginInfo>());
                var reqnrollConfiguration = container.Resolve<ReqnrollConfiguration>();
                reqnrollConfiguration.AddNonParallelizableMarkerForTags = addNonParallelizableMarkerForTags;
                container.RegisterInstanceAs(testGeneratorProvider);

                var generator = container.Resolve<UnitTestFeatureGeneratorProvider>().CreateGenerator(ParserHelper.CreateAnyDocument());
                return generator;
            }
        }
    }
}