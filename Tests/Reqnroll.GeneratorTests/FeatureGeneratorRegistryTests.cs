using System.Linq;
using Reqnroll.BoDi;
using FluentAssertions;
using NSubstitute;
using Xunit;
using Reqnroll.Configuration;
using Reqnroll.Generator;
using Reqnroll.Generator.Generation;
using Reqnroll.Generator.Interfaces;
using Reqnroll.Generator.UnitTestConverter;
using Reqnroll.Parser;

namespace Reqnroll.GeneratorTests
{
    
    public class FeatureGeneratorRegistryTests
    {
        private IObjectContainer container;
        
        public FeatureGeneratorRegistryTests()
        {
            container = new GeneratorContainerBuilder().CreateContainer(new ReqnrollConfigurationHolder(ConfigSource.Default, null), new ProjectSettings(), Enumerable.Empty<GeneratorPluginInfo>());
        }

        private FeatureGeneratorRegistry CreateFeatureGeneratorRegistry()
        {
            return container.Resolve<FeatureGeneratorRegistry>();
        }

        [Fact]
        public void Should_create_UnitTestFeatureGenerator_with_default_setup()
        {
            var featureGeneratorRegistry = CreateFeatureGeneratorRegistry();

            var anyDocument = ParserHelper.CreateAnyDocument();
            var generator = featureGeneratorRegistry.CreateGenerator(anyDocument);

            generator.Should().BeOfType<UnitTestFeatureGenerator>();
        }

        [Fact]
        public void Should_use_generic_provider_with_higher_priority()
        {
            var dummyGenerator = Substitute.For<IFeatureGenerator>();

            var genericHighPrioProvider = Substitute.For<IFeatureGeneratorProvider>();
            genericHighPrioProvider.CreateGenerator(Arg.Any<ReqnrollDocument>()).Returns(dummyGenerator);
            genericHighPrioProvider.CanGenerate(Arg.Any<ReqnrollDocument>()).Returns(true); // generic
            genericHighPrioProvider.Priority.Returns(1); // high-prio

            container.RegisterInstanceAs(genericHighPrioProvider, "custom");

            var featureGeneratorRegistry = CreateFeatureGeneratorRegistry();

            var anyFeature = ParserHelper.CreateAnyDocument();
            var generator = featureGeneratorRegistry.CreateGenerator(anyFeature);

            generator.Should().Be(dummyGenerator);
        }

        [Fact]
        public void Should_call_provider_wiht_the_given_feature()
        {
            var dummyGenerator = Substitute.For<IFeatureGenerator>();

            var genericHighPrioProvider = Substitute.For<IFeatureGeneratorProvider>();
            genericHighPrioProvider.CreateGenerator(Arg.Any<ReqnrollDocument>()).Returns(dummyGenerator);
            genericHighPrioProvider.CanGenerate(Arg.Any<ReqnrollDocument>()).Returns(true); // generic
            genericHighPrioProvider.Priority.Returns(1); // high-prio

            container.RegisterInstanceAs(genericHighPrioProvider, "custom");

            var featureGeneratorRegistry = CreateFeatureGeneratorRegistry();

            ReqnrollDocument theDocument = ParserHelper.CreateAnyDocument();
            featureGeneratorRegistry.CreateGenerator(theDocument);

            genericHighPrioProvider.Received(1).CreateGenerator(theDocument); //TODO NSub check
        }

        [Fact]
        public void Should_skip_high_priority_provider_when_not_applicable()
        {
            var dummyGenerator = Substitute.For<IFeatureGenerator>();

            ReqnrollDocument theDocument = ParserHelper.CreateAnyDocument();

            var genericHighPrioProvider = Substitute.For<IFeatureGeneratorProvider>();
            genericHighPrioProvider.CreateGenerator(Arg.Any<ReqnrollDocument>()).Returns(dummyGenerator);
            genericHighPrioProvider.CanGenerate(theDocument).Returns(false); // not applicable for aFeature
            genericHighPrioProvider.Priority.Returns(1); // high-prio

            container.RegisterInstanceAs(genericHighPrioProvider, "custom");

            var featureGeneratorRegistry = CreateFeatureGeneratorRegistry();

            var generator = featureGeneratorRegistry.CreateGenerator(theDocument);

            generator.Should().BeOfType<UnitTestFeatureGenerator>();
        }

        [Fact]
        public void Should_FeatureGeneratorRegistry_be_registered_as_IFeatureGeneratorRegistry_by_default()
        {
            var testContainer = new GeneratorContainerBuilder().CreateContainer(new ReqnrollConfigurationHolder(ConfigSource.Default, null), new ProjectSettings(), Enumerable.Empty<GeneratorPluginInfo>());

            var registry = testContainer.Resolve<IFeatureGeneratorRegistry>();

            registry.Should().BeOfType<FeatureGeneratorRegistry>();
        }

        private class TestTagFilteredFeatureGeneratorProvider : TagFilteredFeatureGeneratorProvider
        {
            static public IFeatureGenerator DummyGenerator = Substitute.For<IFeatureGenerator>();

            public TestTagFilteredFeatureGeneratorProvider(ITagFilterMatcher tagFilterMatcher, string registeredName) : base(tagFilterMatcher, registeredName)
            {
            }

            public override IFeatureGenerator CreateGenerator(ReqnrollDocument feature)
            {
                return DummyGenerator;
            }
        }

        [Fact]
        public void Should_TagFilteredFeatureGeneratorProvider_applied_for_registered_tag_name()
        {
            container.RegisterTypeAs<TestTagFilteredFeatureGeneratorProvider, IFeatureGeneratorProvider>("mytag");

            ReqnrollDocument theDocument = ParserHelper.CreateAnyDocument(tags: new[] {"mytag"});

            var featureGeneratorRegistry = CreateFeatureGeneratorRegistry();

            var generator = featureGeneratorRegistry.CreateGenerator(theDocument);

            generator.Should().Be(TestTagFilteredFeatureGeneratorProvider.DummyGenerator);
        }

        [Fact]
        public void Should_TagFilteredFeatureGeneratorProvider_applied_for_registered_tag_name_with_at()
        {
            container.RegisterTypeAs<TestTagFilteredFeatureGeneratorProvider, IFeatureGeneratorProvider>("@mytag");

            ReqnrollDocument theDocument = ParserHelper.CreateAnyDocument(tags: new[] { "mytag" });

            var featureGeneratorRegistry = CreateFeatureGeneratorRegistry();

            var generator = featureGeneratorRegistry.CreateGenerator(theDocument);

            generator.Should().Be(TestTagFilteredFeatureGeneratorProvider.DummyGenerator);
        }

        [Fact]
        public void Should_TagFilteredFeatureGeneratorProvider_not_be_applied_for_feature_with_other_tgas()
        {
            container.RegisterTypeAs<TestTagFilteredFeatureGeneratorProvider, IFeatureGeneratorProvider>("mytag");

            ReqnrollDocument theDocument = ParserHelper.CreateAnyDocument(tags: new[] { "othertag" });

            var featureGeneratorRegistry = CreateFeatureGeneratorRegistry();

            var generator = featureGeneratorRegistry.CreateGenerator(theDocument);

            generator.Should().NotBe(TestTagFilteredFeatureGeneratorProvider.DummyGenerator);
        }

        [Fact]
        public void Should_TagFilteredFeatureGeneratorProvider_not_be_applied_for_feature_with_no_tgas()
        {
            container.RegisterTypeAs<TestTagFilteredFeatureGeneratorProvider, IFeatureGeneratorProvider>("mytag");

            ReqnrollDocument theDocument = ParserHelper.CreateAnyDocument();

            var featureGeneratorRegistry = CreateFeatureGeneratorRegistry();

            var generator = featureGeneratorRegistry.CreateGenerator(theDocument);

            generator.Should().NotBe(TestTagFilteredFeatureGeneratorProvider.DummyGenerator);
        }
    }
}
