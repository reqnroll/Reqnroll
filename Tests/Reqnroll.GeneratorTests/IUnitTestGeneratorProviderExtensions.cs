using System.Linq;
using Reqnroll.Configuration;
using Reqnroll.Generator;
using Reqnroll.Generator.CodeDom;
using Reqnroll.Generator.Generation;
using Reqnroll.Generator.Interfaces;
using Reqnroll.Generator.UnitTestConverter;
using Reqnroll.Generator.UnitTestProvider;

namespace Reqnroll.GeneratorTests
{
    public static class IUnitTestGeneratorProviderExtensions
    {
        public static UnitTestFeatureGenerator CreateUnitTestConverter(this IUnitTestGeneratorProvider testGeneratorProvider, bool disableFriendlyTestNames = ConfigDefaults.DisableFriendlyTestNames)
        {
            var codeDomHelper = new CodeDomHelper(CodeDomProviderLanguage.CSharp);

            var runtimeConfiguration = ConfigurationLoader.GetDefault();
            runtimeConfiguration.AllowRowTests = true;
            runtimeConfiguration.AllowDebugGeneratedFiles = true;
            runtimeConfiguration.DisableFriendlyTestNames = disableFriendlyTestNames;

            return new UnitTestFeatureGenerator(testGeneratorProvider, codeDomHelper, runtimeConfiguration, new DecoratorRegistryStub());
        }

        public static IFeatureGenerator CreateFeatureGenerator(this IUnitTestGeneratorProvider testGeneratorProvider, string[] addNonParallelizableMarkerForTags = null)
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