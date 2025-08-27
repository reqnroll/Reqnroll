using System.CodeDom;
using System.Collections.Generic;
using System.Linq;
using Reqnroll.BoDi;
using Moq;
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

        protected Mock<IUnitTestGeneratorProvider> UnitTestGeneratorProviderMock { get; private set; }
        protected IObjectContainer Container { get; private set; }

        protected virtual void SetupInternal()
        {
            Container = new GeneratorContainerBuilder().CreateContainer(new ReqnrollConfigurationHolder(ConfigSource.Default, null), new ProjectSettings(), Enumerable.Empty<GeneratorPluginInfo>());
            UnitTestGeneratorProviderMock = new Mock<IUnitTestGeneratorProvider>();
            Container.RegisterInstanceAs(UnitTestGeneratorProviderMock.Object);
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
            UnitTestGeneratorProviderMock.Setup(ug => ug.SetTestClassCategories(It.IsAny<TestClassGenerationContext>(), It.IsAny<IEnumerable<string>>()))
                .Callback((TestClassGenerationContext ctx, IEnumerable<string> cats) => generatedCats = cats.ToArray());

            var theDocument = ParserHelper.CreateDocument(new string[] { "foo", "bar" });

            GenerateFeature(generator, theDocument);

            generatedCats.Should().Equal(new string[] {"foo", "bar"});
        }

        [Fact]
        public void Should_pass_scenario_tags_as_test_method_category()
        {
            var generator = CreateUnitTestFeatureGenerator();
            string[] generatedCats = new string[0];
            UnitTestGeneratorProviderMock.Setup(ug => ug.SetTestMethodCategories(It.IsAny<TestClassGenerationContext>(), It.IsAny<CodeMemberMethod>(), It.IsAny<IEnumerable<string>>()))
                .Callback((TestClassGenerationContext ctx, CodeMemberMethod _, IEnumerable<string> cats) => generatedCats = cats.ToArray());

            var theFeature = ParserHelper.CreateDocument(scenarioTags: new []{ "foo", "bar"});

            GenerateFeature(generator, theFeature);

            generatedCats.Should().Equal(new string[] {"foo", "bar"});
        }

        [Fact]
        public void Should_pass_scenario_tags_as_test_method_categories_of_scenarioOutline()
        {
            var generator = CreateUnitTestFeatureGenerator();
            List<string> generatedCats = new();
            UnitTestGeneratorProviderMock.Setup(ug => ug.SetTestMethodCategories(It.IsAny<TestClassGenerationContext>(), It.IsAny<CodeMemberMethod>(), It.IsAny<IEnumerable<string>>()))
                .Callback((TestClassGenerationContext ctx, CodeMemberMethod _, IEnumerable<string> cats) => recordTestMethodCategories( cats, generatedCats));

            var theFeature = ParserHelper.CreateDocumentWithScenarioOutline(scenarioOutlineTags: new[] { "foo", "bar" });

            GenerateFeature(generator, theFeature);

            generatedCats.Should().Equal(new string[] { "foo", "bar" });

            void recordTestMethodCategories(IEnumerable<string> cats, List<string> generatedCats)
            {
                generatedCats.AddRange(cats);
            }

        }


        [Fact]
        public void Should_pass_rule_tags_as_test_method_category()
        {
            var generator = CreateUnitTestFeatureGenerator();
            string[] generatedCats = new string[0];
            UnitTestGeneratorProviderMock.Setup(ug => ug.SetTestMethodCategories(It.IsAny<TestClassGenerationContext>(), It.IsAny<CodeMemberMethod>(), It.IsAny<IEnumerable<string>>()))
                .Callback((TestClassGenerationContext ctx, CodeMemberMethod _, IEnumerable<string> cats) => generatedCats = cats.ToArray());

            var theFeature = ParserHelper.CreateDocumentWithRule(ruleTags: new[] { "rule_tag1", "rule_tag2" }, scenarioTags: new[] {"scenarioTag1"});

            GenerateFeature(generator, theFeature);

            generatedCats.Should().BeEquivalentTo(new string[] { "rule_tag1", "rule_tag2", "scenarioTag1" });
        }


        [Fact]
        public void Should_pass_rule_tags_as_test_method_category_of_scenario_outline()
        {
            var generator = CreateUnitTestFeatureGenerator();
            List<string> generatedCats = new List<string>();
            UnitTestGeneratorProviderMock.Setup(ug => ug.SetTestMethodCategories(It.IsAny<TestClassGenerationContext>(), It.IsAny<CodeMemberMethod>(), It.IsAny<IEnumerable<string>>()))
                .Callback((TestClassGenerationContext ctx, CodeMemberMethod _, IEnumerable<string> cats) => recordTestMethodCategories(cats, generatedCats));

            var theFeature = ParserHelper.CreateDocumentWithScenarioOutlineInRule(ruleTags: new[] { "rule_tag1", "rule_tag2" }, scenarioOutlineTags: new[] { "scenarioTag1" });

            GenerateFeature(generator, theFeature);

            generatedCats.Should().BeEquivalentTo(new List<string> { "rule_tag1", "rule_tag2", "scenarioTag1" });

            void recordTestMethodCategories(IEnumerable<string> cats, List<string> generatedCats)
            {
                generatedCats.AddRange(cats);
            }
        }


        [Fact]
        public void Should_not_pass_feature_tags_as_test_method_category()
        {
            var generator = CreateUnitTestFeatureGenerator();
            string[] generatedCats = new string[0];
            UnitTestGeneratorProviderMock.Setup(ug => ug.SetTestMethodCategories(It.IsAny<TestClassGenerationContext>(), It.IsAny<CodeMemberMethod>(), It.IsAny<IEnumerable<string>>()))
                .Callback((TestClassGenerationContext ctx, CodeMemberMethod _, IEnumerable<string> cats) => generatedCats = cats.ToArray());

            var theFeature = ParserHelper.CreateDocument(tags: new []{ "featuretag"}, scenarioTags: new[] { "foo", "bar" });

            GenerateFeature(generator, theFeature);

            generatedCats.Should().Equal(new string[] {"foo", "bar"});
        }

        [Fact]
        public void Should_not_pass_decorated_feature_tag_as_test_class_category()
        {
            var decoratorMock = DecoratorRegistryTests.CreateTestClassTagDecoratorMock("decorated");
            Container.RegisterInstanceAs(decoratorMock.Object, "decorated");

            var generator = CreateUnitTestFeatureGenerator();

            var theFeature = ParserHelper.CreateDocument(new string[] { "decorated", "other" });

            GenerateFeature(generator, theFeature);

            UnitTestGeneratorProviderMock.Verify(ug => ug.SetTestClassCategories(It.IsAny<TestClassGenerationContext>(), It.Is<IEnumerable<string>>(cats => !cats.Contains("decorated"))));
        }

        [Fact]
        public void Should_not_pass_decorated_scenario_tag_as_test_method_category()
        {
            var decoratorMock = DecoratorRegistryTests.CreateTestMethodTagDecoratorMock("decorated");
            Container.RegisterInstanceAs(decoratorMock.Object, "decorated");

            var generator = CreateUnitTestFeatureGenerator();

            var theFeature = ParserHelper.CreateDocument(scenarioTags: new[] { "decorated", "other" });

            GenerateFeature(generator, theFeature);

            UnitTestGeneratorProviderMock.Verify(ug => ug.SetTestMethodCategories(It.IsAny<TestClassGenerationContext>(), It.IsAny<CodeMemberMethod>(), It.Is<IEnumerable<string>>(cats => !cats.Contains("decorated"))));
        }
    }
}
