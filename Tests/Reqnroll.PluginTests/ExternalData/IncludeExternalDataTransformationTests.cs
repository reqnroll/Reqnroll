using System;
using System.Collections.Generic;
using System.Linq;
using Gherkin;
using Gherkin.Ast;
using NSubstitute;
using Reqnroll.ExternalData.ReqnrollPlugin.DataSources;
using Reqnroll.ExternalData.ReqnrollPlugin.Transformation;
using Reqnroll.Parser;
using Xunit;

//TODO NSub format document

namespace Reqnroll.PluginTests.ExternalData
{
    public class IncludeExternalDataTransformationTests
    {
        private const string DOCUMENT_PATH = @"C:\Temp\Sample.feature";
        private ExternalDataSpecification _specification;
        private readonly ISpecificationProvider _specificationProviderMock;

        public IncludeExternalDataTransformationTests()
        {
            _specificationProviderMock = Substitute.For<ISpecificationProvider>();
            _specificationProviderMock.GetSpecification(Arg.Any<IEnumerable<Tag>>(), Arg.Any<string>())
                                      .Returns(_ => _specification);
        }

        private IncludeExternalDataTransformation CreateSut() => new(_specificationProviderMock);

        private Reqnroll.ExternalData.ReqnrollPlugin.DataSources.DataTable CreateProductDataTable()
        {
            return new(new []{"product", "price"})
            {
                Items =
                {
                    new DataRecord(new Dictionary<string, string> { {"product", "Chocolate" }, {"price", "2.5"} }),
                    new DataRecord(new Dictionary<string, string> { {"product", "Apple" }, {"price", "1.0"} }),
                    new DataRecord(new Dictionary<string, string> { {"product", "Orange" }, {"price", "1.2"} }),
                }
            };
        }

        private ReqnrollDocument CreateReqnrollDocument(params IHasLocation[] children)
        {
            return new(new ReqnrollFeature(new Tag[0], null, null, "Feature", "Sample feature", "", children), new Comment[0], 
                       new ReqnrollDocumentLocation(DOCUMENT_PATH));
        }

        private ReqnrollDocument CreateReqnrollDocumentWithFeatureTags(string[] featureTags, params IHasLocation[] children)
        {
            return new(new ReqnrollFeature(featureTags.Select(t => new Tag(null, t)).ToArray(), null, null, "Feature", "Sample feature", "", children), new Comment[0], 
                       new ReqnrollDocumentLocation(DOCUMENT_PATH));
        }

        private ScenarioOutline CreateScenarioOutline() =>
            CreateScenarioOutline(
                new[]
                {
                    new Examples(new Tag[] { new(null, "@extag1") }, null, "Examples", "1", "", new TableRow(null, new[] { new TableCell(null, "product") }), Array.Empty<TableRow>()),
                    new Examples(new Tag[] { new(null, "@extag2") }, null, "Examples", "2", "", new TableRow(null, new[] { new TableCell(null, "product") }), Array.Empty<TableRow>())
                });


        private ScenarioOutline CreateScenarioOutline(Examples[] examples)
        {
            return new(
                new Tag[] { new(null, "@sotag") },
                null,
                "Scenario Outline",
                "SO 1",
                null,
                new[] { new Step(null, "Given ", StepKeywordType.Context, "the customer has <product>", null) },
                examples);
        }

        private Scenario CreateScenario()
        {
            return new(
                new Tag[] { new(null, "@stag") },
                null,
                "Scenario",
                "S 1",
                null,
                new[] { new Step(null, "Given ", StepKeywordType.Context, "the customer has stuff", null) },
                null);
        }

        [Fact]
        public void Keeps_document_as_is_when_no_transformation_needed()
        {
            var scenario = CreateScenario();
            var scenarioOutline = CreateScenarioOutline();
            var document = CreateReqnrollDocument(scenario, scenarioOutline);
            _specification = null; // no transformation

            var sut = CreateSut();

            var result = sut.TransformDocument(document);

            Assert.Same(document, result);
        }

        [Fact]
        public void Should_include_external_data_to_scenario()
        {
            var scenario = CreateScenario();
            var document = CreateReqnrollDocument(scenario);
            _specification = new ExternalDataSpecification(new DataSource(CreateProductDataTable()));

            var sut = CreateSut();

            var result = sut.TransformDocument(document);
            
            var transformedOutline = result.Feature.Children.OfType<ScenarioOutline>().FirstOrDefault();
            Assert.NotNull(transformedOutline);
            var examples = transformedOutline.Examples.Last();
            Assert.Equal(3, examples.TableBody.Count());
        }

        [Fact]
        public void Should_include_external_data_to_scenario_outline()
        {
            var scenarioOutline = CreateScenarioOutline();
            var document = CreateReqnrollDocument(scenarioOutline);
            _specification = new ExternalDataSpecification(new DataSource(CreateProductDataTable()));

            var sut = CreateSut();

            var result = sut.TransformDocument(document);
            
            var transformedOutline = result.Feature.Children.OfType<ScenarioOutline>().FirstOrDefault();
            Assert.NotNull(transformedOutline);
            var examples = transformedOutline.Examples.Last();
            Assert.Equal(3, examples.TableBody.Count());
        }

        [Fact]
        public void Should_provide_scenario_outline_and_examples_tags_and_path_for_SpecificationProvider()
        {
            var scenarioOutline = CreateScenarioOutline();
            var document = CreateReqnrollDocument(scenarioOutline);
            _specification = new ExternalDataSpecification(new DataSource(CreateProductDataTable()));

            var sut = CreateSut();

            sut.TransformDocument(document);
            
            _specificationProviderMock.Received().GetSpecification(Arg.Is<IEnumerable<Tag>>(tags => tags.SequenceEqual(scenarioOutline.Tags.Concat(scenarioOutline.Examples.SelectMany(e => e.Tags)))), DOCUMENT_PATH);
        }

        [Fact]
        public void Should_provide_scenario_tags_and_path_for_SpecificationProvider()
        {
            var scenario = CreateScenario();
            var document = CreateReqnrollDocument(scenario);
            _specification = new ExternalDataSpecification(new DataSource(CreateProductDataTable()));

            var sut = CreateSut();

            sut.TransformDocument(document);
            
            _specificationProviderMock.Received().GetSpecification(Arg.Is<IEnumerable<Tag>>(tags => tags.SequenceEqual(scenario.Tags)), DOCUMENT_PATH);
        }

        [Fact]
        public void Should_provide_feature_tags_for_scenarios_to_SpecificationProvider()
        {
            var scenario = CreateScenario();
            var document = CreateReqnrollDocumentWithFeatureTags(new []{ "@featureTag1", "@featureTag2" }, scenario);
            _specification = new ExternalDataSpecification(new DataSource(CreateProductDataTable()));

            var sut = CreateSut();

            sut.TransformDocument(document);
            
            _specificationProviderMock.Received().GetSpecification( Arg.Is<IEnumerable<Tag>>(tags => tags.Take(document.ReqnrollFeature.Tags.Count()).SequenceEqual(document.ReqnrollFeature.Tags)), DOCUMENT_PATH);
        }

        [Fact]
        public void Should_provide_feature_tags_for_scenario_outlines_to_SpecificationProvider()
        {
            var scenarioOutline = CreateScenarioOutline();
            var document = CreateReqnrollDocumentWithFeatureTags(new []{ "@featureTag1", "@featureTag2" }, scenarioOutline);
            _specification = new ExternalDataSpecification(new DataSource(CreateProductDataTable()));

            var sut = CreateSut();

            sut.TransformDocument(document);
            
            _specificationProviderMock.Received().GetSpecification( Arg.Is<IEnumerable<Tag>>(tags => tags.Take(document.ReqnrollFeature.Tags.Count()).SequenceEqual(document.ReqnrollFeature.Tags)), DOCUMENT_PATH);
        }

        [Fact]
        public void Should_handle_scenario_outline_with_empty_examples()
        {
            var scenario = CreateScenarioOutline(new Examples[0]);
            var document = CreateReqnrollDocument(scenario);
            _specification = new ExternalDataSpecification(new DataSource(CreateProductDataTable()));

            var sut = CreateSut();

            sut.TransformDocument(document);

            var result = sut.TransformDocument(document);

            var transformedOutline = result.Feature.Children.OfType<ScenarioOutline>().FirstOrDefault();
            Assert.NotNull(transformedOutline);
            Assert.NotSame(scenario, transformedOutline);
        }

        [Fact]
        public void Should_handle_scenario_outline_with_null_examples()
        {
            var scenario = CreateScenarioOutline(null);
            var document = CreateReqnrollDocument(scenario);
            _specification = new ExternalDataSpecification(new DataSource(CreateProductDataTable()));

            var sut = CreateSut();

            sut.TransformDocument(document);

            var result = sut.TransformDocument(document);

            var transformedOutline = result.Feature.Children.OfType<ScenarioOutline>().FirstOrDefault();
            Assert.NotNull(transformedOutline);
            Assert.NotSame(scenario, transformedOutline);
        }
    }
}
