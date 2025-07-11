using Xunit;
using Reqnroll.Generator.UnitTestConverter;
using FluentAssertions;
using Reqnroll.Configuration;
using Reqnroll.Generator.CodeDom;
using Reqnroll.Generator.Generation;
using Reqnroll.Generator.UnitTestProvider;
using Reqnroll.Tracing;

namespace Reqnroll.GeneratorTests
{
    
    public class FeatureGeneratorProviderTests
    {
        private static UnitTestFeatureGeneratorProvider CreateUnitTestFeatureGeneratorProvider()
        {
            Configuration.ReqnrollConfiguration generatorReqnrollConfiguration = ConfigurationLoader.GetDefault();
            CodeDomHelper codeDomHelper = new CodeDomHelper(CodeDomProviderLanguage.CSharp);
            UnitTestFeatureGenerator unitTestFeatureGenerator = new UnitTestFeatureGenerator(
                new NUnit3TestGeneratorProvider(codeDomHelper), codeDomHelper, generatorReqnrollConfiguration, new DecoratorRegistryStub());

            return new UnitTestFeatureGeneratorProvider(unitTestFeatureGenerator);
        }

        [Fact]
        public void Should_UnitTestFeatureGeneratorProvider_have_low_priority()
        {
            var generatorProvider = CreateUnitTestFeatureGeneratorProvider();
            generatorProvider.Priority.Should().Be(int.MaxValue);
        }

        [Fact]
        public void Should_UnitTestFeatureGeneratorProvider_be_able_to_generate_anything()
        {
            var generatorProvider = CreateUnitTestFeatureGeneratorProvider();
            var anyFeature = ParserHelper.CreateAnyDocument();
            generatorProvider.CanGenerate(anyFeature).Should().Be(true);
        }

        [Fact]
        public void Should_UnitTestFeatureGeneratorProvider_create_valid_instance()
        {
            var generatorProvider = CreateUnitTestFeatureGeneratorProvider();
            var anyFeature = ParserHelper.CreateAnyDocument();
            var generator = generatorProvider.CreateGenerator(anyFeature);

            generator.Should().NotBeNull();
        }

        [Fact]
        public void Should_UnitTestFeatureGeneratorProvider_create_UnitTestFeatureGenerator_instance()
        {
            var generatorProvider = CreateUnitTestFeatureGeneratorProvider();
            var anyFeature = ParserHelper.CreateAnyDocument();
            var generator = generatorProvider.CreateGenerator(anyFeature);

            generator.Should().BeOfType<UnitTestFeatureGenerator>();
        }
    }
}
