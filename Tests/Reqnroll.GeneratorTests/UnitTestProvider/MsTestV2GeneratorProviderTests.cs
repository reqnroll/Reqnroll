using FluentAssertions;
using Reqnroll.Generator.CodeDom;
using Reqnroll.Generator.UnitTestProvider;
using Reqnroll.Parser;
using System.Globalization;
using System.IO;
using System.Linq;
using Xunit;

namespace Reqnroll.GeneratorTests.UnitTestProvider
{
    public class MsTestV2GeneratorProviderTests
    {
        private const string TestDeploymentItemAttributeName = "Microsoft.VisualStudio.TestTools.UnitTesting.DeploymentItemAttribute";

        [Fact]
        public void MsTestV2GeneratorProvider_WithFeatureWithoutDeploymentItem_GeneratedClassShouldNotIncludeDeploymentItemForPlugin()
        {
            // ARRANGE
            var document = ParseDocumentFromString(@"
            Feature: Sample feature file

            Scenario: Simple scenario
                Given there is something
                When I do something
                Then something should happen");
            var sampleTestGeneratorProvider = new MsTestV2GeneratorProvider(new CodeDomHelper(CodeDomProviderLanguage.CSharp));
            var converter = sampleTestGeneratorProvider.CreateUnitTestConverter();

            // ACT
            var code = converter.GenerateUnitTestFixture(document, "TestClassName", "Target.Namespace");

            // ASSERT
            code.Class().CustomAttributes().Any(a => a.Name == TestDeploymentItemAttributeName).Should().BeFalse();
        }

        [Fact]
        public void MsTestV2GeneratorProvider_WithFeatureContainingDeploymentItem_GeneratedClassShouldIncludeDeploymentItemForPlugin()
        {
            // ARRANGE
            var document = ParseDocumentFromString(@"
            @MsTest:DeploymentItem:DeploymentItemTestFile.txt
            Feature: Sample feature file

            Scenario: Simple scenario
                Given there is something
                When I do something
                Then something should happen");

            var sampleTestGeneratorProvider = new MsTestV2GeneratorProvider(new CodeDomHelper(CodeDomProviderLanguage.CSharp));
            var converter = sampleTestGeneratorProvider.CreateUnitTestConverter();

            // ACT
            var code = converter.GenerateUnitTestFixture(document, "TestClassName", "Target.Namespace");

            // ASSERT
            var deploymentItemAttributeForClass = code.Class().CustomAttributes().Single(a => a.Name == TestDeploymentItemAttributeName);
            deploymentItemAttributeForClass.ArgumentValues().First().Should().Be("Reqnroll.MSTest.ReqnrollPlugin.dll");
        }

        [Fact]
        public void MsTestV2GeneratorProvider_ShouldHaveParallelExecutionTrait()
        {
            var sut = new MsTestV2GeneratorProvider(new CodeDomHelper(CodeDomProviderLanguage.CSharp));

            sut.GetTraits()
                .HasFlag(UnitTestGeneratorTraits.ParallelExecution)
                .Should()
                .BeTrue("trait ParallelExecution was not found");
        }

        [Fact]
        public void MsTestV2GeneratorProvider_WithFeatureWithMatchingTag_ShouldAddNonParallelizableAttribute()
        {
            // ARRANGE
            var document = ParseDocumentFromString(@"
            @nonparallelizable
            Feature: Sample feature file

            Scenario: Simple scenario
                Given there is something");

            var provider = new MsTestV2GeneratorProvider(new CodeDomHelper(CodeDomProviderLanguage.CSharp));
            var featureGenerator = provider.CreateFeatureGenerator(addNonParallelizableMarkerForTags: new string[] { "nonparallelizable" });

            // ACT
            var code = featureGenerator.GenerateUnitTestFixture(document, "TestClassName", "Target.Namespace");

            // ASSERT
            var attributes = code.Class().CustomAttributes().ToArray();
            attributes.Should()
                      .ContainSingle(a => a.Name == "Microsoft.VisualStudio.TestTools.UnitTesting.DoNotParallelizeAttribute")
                      .Which.Arguments.Count.Should()
                      .Be(0);
        }

        [Fact]
        public void MsTestV2GeneratorProvider_WithFeatureWithNoMatchingTag_ShouldNotAddNonParallelizableAttribute()
        {
            // ARRANGE
            var document = ParseDocumentFromString(@"
            Feature: Sample feature file

            Scenario: Simple scenario
                Given there is something");

            var provider = new MsTestV2GeneratorProvider(new CodeDomHelper(CodeDomProviderLanguage.CSharp));
            var featureGenerator = provider.CreateFeatureGenerator(addNonParallelizableMarkerForTags: new string[] { "nonparallelizable" });

            // ACT
            var code = featureGenerator.GenerateUnitTestFixture(document, "TestClassName", "Target.Namespace");

            // ASSERT
            var attributes = code.Class().CustomAttributes().ToArray();
            attributes.Should().NotContain(a => a.Name == "Microsoft.VisualStudio.TestTools.UnitTesting.DoNotParallelizeAttribute");
        }

        [Theory]
        [InlineData(true)]
        [InlineData(false)]
        public void MsTestV2GeneratorProvider_WithPriorityTag_ShouldAddPriorityAttribute(bool testFeatureTag)
        {
            // ARRANGE
            var featureTag = testFeatureTag ? "@priority:1" : string.Empty;
            var scenarioTag = testFeatureTag ? string.Empty : "@priority:1";

            var document = ParseDocumentFromString($@"
            {featureTag}
            Feature: Sample feature file

            {scenarioTag}
            Scenario: Simple scenario
                Given there is something");

            var provider = new MsTestV2GeneratorProvider(new CodeDomHelper(CodeDomProviderLanguage.CSharp));
            var featureGenerator = provider.CreateFeatureGenerator();

            // ACT
            var code = featureGenerator.GenerateUnitTestFixture(document, "TestClassName", "Target.Namespace");

            // ASSERT
            var testMethod = code.Class().Members().Single(m => m.Name == "SimpleScenario");
            testMethod.CustomAttributes().Should().ContainSingle(a => a.Name == MsTestV2GeneratorProvider.PRIORITY_ATTR && (int)a.ArgumentValues().First() == 1);
        }

        [Fact]
        public void MsTestV2GeneratorProvider_WithoutPriorityTag_ShouldntAddPriorityAttribute()
        {
            // ARRANGE
            var document = ParseDocumentFromString(@"
            Feature: Sample feature file

            Scenario: Simple scenario
                Given there is something");

            var provider = new MsTestV2GeneratorProvider(new CodeDomHelper(CodeDomProviderLanguage.CSharp));
            var featureGenerator = provider.CreateFeatureGenerator();

            // ACT
            var code = featureGenerator.GenerateUnitTestFixture(document, "TestClassName", "Target.Namespace");

            // ASSERT
            var testMethod = code.Class().Members().Single(m => m.Name == "SimpleScenario");
            testMethod.CustomAttributes().Should().NotContain(a => a.Name == MsTestV2GeneratorProvider.PRIORITY_ATTR);
        }

        public ReqnrollDocument ParseDocumentFromString(string documentSource, CultureInfo parserCultureInfo = null)
        {
            var parser = new ReqnrollGherkinParser(parserCultureInfo ?? new CultureInfo("en-US"));
            using (var reader = new StringReader(documentSource))
            {
                var document = parser.Parse(reader, null);
                document.Should().NotBeNull();
                return document;
            }
        }
    }
}
