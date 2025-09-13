using System.CodeDom;
using System.Globalization;
using System.IO;
using System.Linq;
using FluentAssertions;
using Reqnroll.Configuration;
using Reqnroll.Generator;
using Reqnroll.Generator.CodeDom;
using Reqnroll.Generator.Interfaces;
using Reqnroll.Generator.UnitTestConverter;
using Reqnroll.Generator.UnitTestProvider;
using Reqnroll.Parser;
using Reqnroll.TUnit.Generator.ReqnrollPlugin;
using Reqnroll.UnitTestProvider;
using Xunit;

namespace Reqnroll.GeneratorTests.UnitTestProvider
{
    public class TUnitTestGeneratorProviderTests
    {
        private const string FeatureTemplate = "Feature: Sample feature file\n\nScenario: Simple scenario\n    Given there is something";

        [Fact]
        public void TUnitTestGeneratorProvider_FeatureTag_ShouldAddNotInParallelAttributeToClass()
        {
            var feature = "@nonparallelizable\n" + FeatureTemplate;
            var code = GenerateCodeNamespaceFromFeature(feature, addNonParallelizableMarkerForTags: new[] { "nonparallelizable" });
            code.Class().CustomAttributes().Should().Contain(a => a.Name == "TUnit.Core.NotInParallelAttribute");
        }

        [Fact]
        public void TUnitTestGeneratorProvider_ScenarioTagOnly_ShouldAddNotInParallelAttributeToMethod()
        {
            var feature = "Feature: Sample feature file\n\n@nonparallelizable\nScenario: Isolated scenario\n    Given there is something";
            var code = GenerateCodeNamespaceFromFeature(feature, addNonParallelizableMarkerForTags: new[] { "nonparallelizable" });
            code.Class().CustomAttributes().Should().NotContain(a => a.Name == "TUnit.Core.NotInParallelAttribute");
            var method = code.Class().Members().Single(m => m.Name == "IsolatedScenario");
            method.CustomAttributes().Should().Contain(a => a.Name == "TUnit.Core.NotInParallelAttribute");
        }

        private CodeNamespace GenerateCodeNamespaceFromFeature(string featureSource, string[] addNonParallelizableMarkerForTags)
        {
            using var reader = new StringReader(featureSource);
            var parser = new ReqnrollGherkinParser(new CultureInfo("en-US"));
            var document = parser.Parse(reader, new ReqnrollDocumentLocation("test.feature"));
            var featureGenerator = CreateFeatureGenerator(addNonParallelizableMarkerForTags);
            return featureGenerator.GenerateUnitTestFixture(document, "TestClassName", "Target.Namespace").CodeNamespace;
        }

        private IFeatureGenerator CreateFeatureGenerator(string[] addNonParallelizableMarkerForTags)
        {
            var container = new GeneratorContainerBuilder().CreateContainer(
                new ReqnrollConfigurationHolder(ConfigSource.Default, null),
                new ProjectSettings(),
                Enumerable.Empty<GeneratorPluginInfo>());

            container.RegisterTypeAs<TUnitTestGeneratorProvider, IUnitTestGeneratorProvider>("tunit");

            var unitTestProviderConfig = container.Resolve<UnitTestProviderConfiguration>();
            unitTestProviderConfig.UseUnitTestProvider("tunit");

            container.RegisterInstanceAs<IUnitTestGeneratorProvider>(new TUnitTestGeneratorProvider(new CodeDomHelper(CodeDomProviderLanguage.CSharp)));

            var config = container.Resolve<ReqnrollConfiguration>();
            config.AddNonParallelizableMarkerForTags = addNonParallelizableMarkerForTags;

            return container.Resolve<UnitTestFeatureGeneratorProvider>().CreateGenerator(ParserHelper.CreateAnyDocument());
        }
    }
}
