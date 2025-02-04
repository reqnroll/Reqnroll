using System.Linq;
using Reqnroll.Configuration;
using Reqnroll.Generator;
using Reqnroll.Generator.CodeDom;
using Reqnroll.Generator.Generation;
using Reqnroll.Generator.Interfaces;
using Reqnroll.Generator.UnitTestConverter;
using Reqnroll.Generator.UnitTestProvider;
using Reqnroll.Tracing;

namespace Reqnroll.GeneratorTests
{
    public static class IUnitTestGeneratorProviderExtensions
    {
        public static UnitTestFeatureGenerator CreateUnitTestConverter(this IUnitTestGeneratorProvider testGeneratorProvider)
        {
            var codeDomHelper = new CodeDomHelper(CodeDomProviderLanguage.CSharp);

            var runtimeConfiguration = ConfigurationLoader.GetDefault();
            runtimeConfiguration.AllowRowTests = true;
            runtimeConfiguration.AllowDebugGeneratedFiles = true;

            return new UnitTestFeatureGenerator(testGeneratorProvider, codeDomHelper, runtimeConfiguration, new DecoratorRegistryStub(), new DefaultListener(), new SimpleCucumberMessagesConfiguration());
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