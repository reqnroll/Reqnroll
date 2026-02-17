using System.Globalization;
using System.IO;
using System.Linq;
using FluentAssertions;
using Reqnroll.Generator.CodeDom;
using Reqnroll.Generator.UnitTestProvider;
using Reqnroll.Parser;
using Xunit;

namespace Reqnroll.GeneratorTests.UnitTestProvider
{
    public class MsTestGeneratorProviderTests
    {
        private const string TestDescriptionAttributeName = "Microsoft.VisualStudio.TestTools.UnitTesting.DescriptionAttribute";
        private const string TestMethodAttributeName = "Microsoft.VisualStudio.TestTools.UnitTesting.TestMethodAttribute";

        private const string SampleFeatureFile = @"
            Feature: Sample feature file

            Scenario: Simple scenario
                Given there is something
                When I do something
                Then something should happen

            @mytag
            Scenario Outline: Simple Scenario Outline
                Given there is something
                    """"""
                      long string
                    """"""
                When I do <what>
                    | foo | bar |
                    | 1   | 2   |
                Then something should happen
            Examples:
                | what           |
                | something      |
                | something else |
";

        private const string SampleFeatureFileWithMultipleExampleSets = @"
            Feature: Sample feature file

            Scenario: Simple scenario
                Given there is something
                When I do something
                Then something should happen

            @mytag
            Scenario Outline: Simple Scenario Outline
                Given there is something
                    """"""
                      long string
                    """"""
                When I do <what>
                    | foo | bar |
                    | 1   | 2   |
                Then something should happen
            Examples:
                | what           |
                | something      |
                | something else |
            Examples:
                | what           |
                | another        |
                | and another    |
";
        private const string SampleFeatureFileSameFirstColumn = @"
            Feature: Sample feature file

            Scenario: Simple scenario
                Given there is something
                When I do something
                Then something should happen

            @mytag
            Scenario Outline: Simple Scenario Outline
                Given there is something
                    """"""
                      long string
                    """"""
                When I do <what>
                    | foo | bar |
                    | 1   | 2   |
                Then something should happen
            Examples:
                | what           | what else       |
                | something      | thing           |
                | something      | different thing |
";
        private const string SampleFeatureFileMultipleColumns = @"
            Feature: Sample feature file

            Scenario: Simple scenario
                Given there is something
                When I do something
                Then something should happen

            @mytag
            Scenario Outline: Simple Scenario Outline
                Given there is something
                    """"""
                      long string
                    """"""
                When I do <what>
                    | foo | bar |
                    | 1   | 2   |
                Then something should happen
            Examples:
                | what           | what else       |
                | something      | thing           |
                | something else | different thing |
";

        [Fact]
        public void MsTestGeneratorProvider_ExampleSetSingleColumn_ShouldSetDescriptionWithVariantNameFromFirstColumn()
        {
            // ARRANGE
            var document = ParseDocumentFromString(SampleFeatureFile);
            var sampleTestGeneratorProvider = new MsTestGeneratorProvider(new CodeDomHelper(CodeDomProviderLanguage.CSharp));
            var converter = sampleTestGeneratorProvider.CreateUnitTestConverter();

            // ACT
            var generationResult = converter.GenerateUnitTestFixture(document, "TestClassName", "Target.Namespace");
            var code = generationResult.CodeNamespace;

            // ASSERT
            var descriptionAttributeForFirstScenarioOutline = code.Class().Members().Single(m => m.Name == "SimpleScenarioOutline_Something").CustomAttributes().Single(a => a.Name == TestDescriptionAttributeName);
            descriptionAttributeForFirstScenarioOutline.ArgumentValues().First().Should().Be("Simple Scenario Outline: something");
            var descriptionAttributeForSecondScenarioOutline = code.Class().Members().Single(m => m.Name == "SimpleScenarioOutline_SomethingElse").CustomAttributes().Single(a => a.Name == TestDescriptionAttributeName);
            descriptionAttributeForSecondScenarioOutline.ArgumentValues().First().Should().Be("Simple Scenario Outline: something else");

            generationResult.GenerationWarnings.Should().BeEmpty();
        }

        [Fact]
        public void MsTestGeneratorProvider_ExamplesWithIdenticalFirstColumn_ShouldSetDescriptionCorrectly()
        {
            // ARRANGE
            var document = ParseDocumentFromString(SampleFeatureFileSameFirstColumn);
            var sampleTestGeneratorProvider = new MsTestGeneratorProvider(new CodeDomHelper(CodeDomProviderLanguage.CSharp));
            var converter = sampleTestGeneratorProvider.CreateUnitTestConverter();

            // ACT
            var code = converter.GenerateUnitTestFixture(document, "TestClassName", "Target.Namespace").CodeNamespace;

            // ASSERT
            var descriptionAttributeForFirstScenarioOutline = code.Class().Members().Single(m => m.Name == "SimpleScenarioOutline_Variant0").CustomAttributes().Single(a => a.Name == TestDescriptionAttributeName);
            descriptionAttributeForFirstScenarioOutline.ArgumentValues().First().Should().Be("Simple Scenario Outline: Variant 0");
            var descriptionAttributeForSecondScenarioOutline = code.Class().Members().Single(m => m.Name == "SimpleScenarioOutline_Variant1").CustomAttributes().Single(a => a.Name == TestDescriptionAttributeName);
            descriptionAttributeForSecondScenarioOutline.ArgumentValues().First().Should().Be("Simple Scenario Outline: Variant 1");
        }

        [Fact]
        public void MsTestGeneratorProvider_ExamplesFirstColumnIsDifferentAndMultipleColumns_ShouldSetDescriptionCorrectly()
        {
            // ARRANGE
            var document = ParseDocumentFromString(SampleFeatureFileMultipleColumns);
            var sampleTestGeneratorProvider = new MsTestGeneratorProvider(new CodeDomHelper(CodeDomProviderLanguage.CSharp));
            var converter = sampleTestGeneratorProvider.CreateUnitTestConverter();

            // ACT
            var code = converter.GenerateUnitTestFixture(document, "TestClassName", "Target.Namespace").CodeNamespace;

            // ASSERT
            var descriptionAttributeForFirstScenarioOutline = code.Class().Members().Single(m => m.Name == "SimpleScenarioOutline_Something").CustomAttributes().Single(a => a.Name == TestDescriptionAttributeName);
            descriptionAttributeForFirstScenarioOutline.ArgumentValues().First().Should().Be("Simple Scenario Outline: something");
            var descriptionAttributeForSecondScenarioOutline = code.Class().Members().Single(m => m.Name == "SimpleScenarioOutline_SomethingElse").CustomAttributes().Single(a => a.Name == TestDescriptionAttributeName);
            descriptionAttributeForSecondScenarioOutline.ArgumentValues().First().Should().Be("Simple Scenario Outline: something else");
        }

        [Fact]
        public void MsTestGeneratorProvider_ExampleSetIdentifiers_ShouldSetDescriptionCorrectly()
        {
            // ARRANGE
            var document = ParseDocumentFromString(SampleFeatureFileWithMultipleExampleSets);
            var sampleTestGeneratorProvider = new MsTestGeneratorProvider(new CodeDomHelper(CodeDomProviderLanguage.CSharp));
            var converter = sampleTestGeneratorProvider.CreateUnitTestConverter();

            // ACT
            var code = converter.GenerateUnitTestFixture(document, "TestClassName", "Target.Namespace").CodeNamespace;

            // ASSERT
            var descriptionAttributeForFirstScenarioOutline = code.Class().Members().Single(m => m.Name == "SimpleScenarioOutline_ExampleSet0_Something").CustomAttributes().Single(a => a.Name == TestDescriptionAttributeName);
            descriptionAttributeForFirstScenarioOutline.ArgumentValues().First().Should().Be("Simple Scenario Outline: something");
            var descriptionAttributeForSecondScenarioOutline = code.Class().Members().Single(m => m.Name == "SimpleScenarioOutline_ExampleSet0_SomethingElse").CustomAttributes().Single(a => a.Name == TestDescriptionAttributeName);
            descriptionAttributeForSecondScenarioOutline.ArgumentValues().First().Should().Be("Simple Scenario Outline: something else");
            var descriptionAttributeForThirdScenarioOutline = code.Class().Members().Single(m => m.Name == "SimpleScenarioOutline_ExampleSet1_Another").CustomAttributes().Single(a => a.Name == TestDescriptionAttributeName);
            descriptionAttributeForThirdScenarioOutline.ArgumentValues().First().Should().Be("Simple Scenario Outline: another");
            var descriptionAttributeForFourthScenarioOutline = code.Class().Members().Single(m => m.Name == "SimpleScenarioOutline_ExampleSet1_AndAnother").CustomAttributes().Single(a => a.Name == TestDescriptionAttributeName);
            descriptionAttributeForFourthScenarioOutline.ArgumentValues().First().Should().Be("Simple Scenario Outline: and another");
        }

        [Fact]
        public void MsTestGeneratorProvider_ShouldNotHaveParallelExecutionTrait()
        {
            var sut = new MsTestGeneratorProvider(new CodeDomHelper(CodeDomProviderLanguage.CSharp));

            sut.GetTraits()
                .HasFlag(UnitTestGeneratorTraits.ParallelExecution)
                .Should()
                .BeFalse("trait ParallelExecution was found");
        }

        [Fact]
        public void MsTestGeneratorProvider_WithFeatureWithMatchingTag_ShouldNotAddDoNotParallelizeAttribute()
        {
            // ARRANGE
            var document = ParseDocumentFromString(@"
            @nonparallelizable
            Feature: Sample feature file

            Scenario: Simple scenario
                Given there is something");

            var provider = new MsTestGeneratorProvider(new CodeDomHelper(CodeDomProviderLanguage.CSharp));
            var featureGenerator = provider.CreateFeatureGenerator(addNonParallelizableMarkerForTags: new[] { "nonparallelizable" });

            // ACT
            var code = featureGenerator.GenerateUnitTestFixture(document, "TestClassName", "Target.Namespace").CodeNamespace;

            // ASSERT
            var attributes = code.Class().CustomAttributes().ToArray();
            attributes.Should().NotContain(a => a.Name == "Microsoft.VisualStudio.TestTools.UnitTesting.DoNotParallelizeAttribute");
        }

        [Fact]
        public void MsTestGeneratorProvider_WithFeatureWithNoMatchingTag_ShouldNotAddDoNotParallelizeAttribute()
        {
            // ARRANGE
            var document = ParseDocumentFromString(@"
            Feature: Sample feature file

            Scenario: Simple scenario
                Given there is something");

            var provider = new MsTestGeneratorProvider(new CodeDomHelper(CodeDomProviderLanguage.CSharp));
            var featureGenerator = provider.CreateFeatureGenerator(addNonParallelizableMarkerForTags: ["nonparallelizable"]);

            // ACT
            var code = featureGenerator.GenerateUnitTestFixture(document, "TestClassName", "Target.Namespace").CodeNamespace;

            // ASSERT
            var attributes = code.Class().CustomAttributes().ToArray();
            attributes.Should().NotContain(a => a.Name == "Microsoft.VisualStudio.TestTools.UnitTesting.DoNotParallelizeAttribute");
        }

        [Fact]
        public void MsTestGeneratorProvider_WithScenarioTagOnly_ShouldNotAddDoNotParallelizeAttribute()
        {
            // ARRANGE
            var document = ParseDocumentFromString(@"
            Feature: Sample feature file

            @nonparallelizable
            Scenario: Isolated scenario
                Given there is something");

            var provider = new MsTestGeneratorProvider(new CodeDomHelper(CodeDomProviderLanguage.CSharp));
            var featureGenerator = provider.CreateFeatureGenerator(addNonParallelizableMarkerForTags: new[] { "nonparallelizable" });

            // ACT
            var code = featureGenerator.GenerateUnitTestFixture(document, "TestClassName", "Target.Namespace").CodeNamespace;

            // ASSERT
            code.Class().CustomAttributes().Should().NotContain(a => a.Name == "Microsoft.VisualStudio.TestTools.UnitTesting.DoNotParallelizeAttribute");
            var method = code.Class().Members().Single(m => m.Name == "IsolatedScenario");
            method.CustomAttributes().Should().NotContain(a => a.Name == "Microsoft.VisualStudio.TestTools.UnitTesting.DoNotParallelizeAttribute");
        }

        // Test for GH#588 - Generator Provider adds Friendly Name as an argument to the TestMethodAttribute on a non-paramterized test
        [Theory]
        [InlineData(true)]
        [InlineData(false)]
        public void MsTestGeneratorProvider_SimpleScenario_ShouldProvideFriendlyNameForTestMethodAttribute(bool disableFriendlyTestNames)
        {
            // ARRANGE
            var document = ParseDocumentFromString(SampleFeatureFile);
            var sampleTestGeneratorProvider = new MsTestGeneratorProvider(new CodeDomHelper(CodeDomProviderLanguage.CSharp));
            var converter = sampleTestGeneratorProvider.CreateUnitTestConverter(disableFriendlyTestNames: disableFriendlyTestNames);

            // ACT
            var code = converter.GenerateUnitTestFixture(document, "TestClassName", "Target.Namespace").CodeNamespace;

            // ASSERT
            var testMethodAttributeForFirstScenario = code.Class().Members().Single(m => m.Name == "SimpleScenario").CustomAttributes().Single(a => a.Name == TestMethodAttributeName);
            if (disableFriendlyTestNames)
            {
                testMethodAttributeForFirstScenario.ArgumentValues().Should().BeEmpty();
            }
            else
            {
                testMethodAttributeForFirstScenario.ArgumentValues().Should().HaveCount(1);
                testMethodAttributeForFirstScenario.ArgumentValues().First().Should().Be("Simple scenario");
            }
        }

        public ReqnrollDocument ParseDocumentFromString(string documentSource, CultureInfo parserCultureInfo = null)
        {
            var parser = new ReqnrollGherkinParser(parserCultureInfo ?? new CultureInfo("en-US"));
            using (var reader = new StringReader(documentSource))
            {
                var document = parser.Parse(reader, new ReqnrollDocumentLocation($"dummy_Reqnroll_Location_for{nameof(MsTestGeneratorProviderTests)}"));
                document.Should().NotBeNull();
                return document;
            }
        }
    }
}