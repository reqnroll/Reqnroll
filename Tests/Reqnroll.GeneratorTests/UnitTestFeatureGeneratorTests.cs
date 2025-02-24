using System.CodeDom;
using System.Collections.Generic;
using System.Linq;
using Reqnroll.BoDi;
using NSubstitute;
using Xunit;
using Reqnroll.Generator;
using Reqnroll.Generator.Interfaces;
using Reqnroll.Generator.UnitTestConverter;
using Reqnroll.Generator.UnitTestProvider;
using FluentAssertions;
using Reqnroll.Configuration;
using Reqnroll.Parser;

namespace Reqnroll.GeneratorTests
{
    public abstract class UnitTestFeatureGeneratorTestsBase
    {
        protected UnitTestFeatureGeneratorTestsBase()
        {
            SetupInternal();
        }

        protected IUnitTestGeneratorProvider UnitTestGeneratorProviderMock { get; private set; }
        protected IObjectContainer Container { get; private set; }

        protected virtual void SetupInternal()
        {
            Container = new GeneratorContainerBuilder().CreateContainer(new ReqnrollConfigurationHolder(ConfigSource.Default, null), new ProjectSettings(), Enumerable.Empty<GeneratorPluginInfo>());
            UnitTestGeneratorProviderMock = Substitute.For<IUnitTestGeneratorProvider>();
            Container.RegisterInstanceAs(UnitTestGeneratorProviderMock);
        }

        protected IFeatureGenerator CreateUnitTestFeatureGenerator()
        {
            return Container.Resolve<UnitTestFeatureGeneratorProvider>().CreateGenerator(ParserHelper.CreateAnyDocument());
        }

        protected void GenerateFeature(IFeatureGenerator generator, ReqnrollDocument document)
        {
            generator.GenerateUnitTestFixture(document, "dummy", "dummyNS");
        }
    }

    
    public class UnitTestFeatureGeneratorTests : UnitTestFeatureGeneratorTestsBase
    {
        [Fact]
        public void Should_pass_feature_tags_as_test_class_category()
        {
            var generator = CreateUnitTestFeatureGenerator();
            string[] generatedCats = new string[0];
            UnitTestGeneratorProviderMock.When(m => m.SetTestClassCategories(Arg.Any<TestClassGenerationContext>(), Arg.Any<IEnumerable<string>>()))
                .Do((args) => generatedCats = args.Arg<IEnumerable<string>>().ToArray());

            var theDocument = ParserHelper.CreateDocument(new string[] { "foo", "bar" });

            GenerateFeature(generator, theDocument);

            generatedCats.Should().Equal(new string[] {"foo", "bar"});
        }

        [Fact]
        public void Should_pass_scenario_tags_as_test_method_category()
        {
            var generator = CreateUnitTestFeatureGenerator();
            string[] generatedCats = new string[0];
            UnitTestGeneratorProviderMock.When(m => m.SetTestMethodCategories(Arg.Any<TestClassGenerationContext>(), Arg.Any<CodeMemberMethod>(), Arg.Any<IEnumerable<string>>()))
                                         .Do((args) => generatedCats = args.Arg<IEnumerable<string>>().ToArray());

            var theFeature = ParserHelper.CreateDocument(scenarioTags: new []{ "foo", "bar"});

            GenerateFeature(generator, theFeature);

            generatedCats.Should().Equal(new string[] {"foo", "bar"});
        }

        [Fact]
        public void Should_not_pass_feature_tags_as_test_method_category()
        {
            var generator = CreateUnitTestFeatureGenerator();
            string[] generatedCats = new string[0];
            UnitTestGeneratorProviderMock.When(m => m.SetTestMethodCategories(Arg.Any<TestClassGenerationContext>(), Arg.Any<CodeMemberMethod>(), Arg.Any<IEnumerable<string>>()))
                                         .Do((args) => generatedCats = args.Arg<IEnumerable<string>>().ToArray());

            var theFeature = ParserHelper.CreateDocument(tags: new []{ "featuretag"}, scenarioTags: new[] { "foo", "bar" });

            GenerateFeature(generator, theFeature);

            generatedCats.Should().Equal(new string[] {"foo", "bar"});
        }

        [Fact]
        public void Should_not_pass_decorated_feature_tag_as_test_class_category()
        {
            var decoratorMock = DecoratorRegistryTests.CreateTestClassTagDecoratorMock("decorated");
            Container.RegisterInstanceAs(decoratorMock, "decorated");

            var generator = CreateUnitTestFeatureGenerator();

            var theFeature = ParserHelper.CreateDocument(new string[] { "decorated", "other" });

            GenerateFeature(generator, theFeature);

            UnitTestGeneratorProviderMock.Received().SetTestClassCategories(Arg.Any<TestClassGenerationContext>(), Arg.Is<IEnumerable<string>>(cats => !cats.Contains("decorated")));
        }

        [Fact]
        public void Should_not_pass_decorated_scenario_tag_as_test_method_category()
        {
            var decoratorMock = DecoratorRegistryTests.CreateTestMethodTagDecoratorMock("decorated");
            Container.RegisterInstanceAs(decoratorMock, "decorated");

            var generator = CreateUnitTestFeatureGenerator();

            var theFeature = ParserHelper.CreateDocument(scenarioTags: new[] { "decorated", "other" });

            GenerateFeature(generator, theFeature);

            UnitTestGeneratorProviderMock.Received().SetTestMethodCategories(Arg.Any<TestClassGenerationContext>(), Arg.Any<CodeMemberMethod>(), Arg.Is<IEnumerable<string>>(cats => !cats.Contains("decorated")));
        }
    }
}
