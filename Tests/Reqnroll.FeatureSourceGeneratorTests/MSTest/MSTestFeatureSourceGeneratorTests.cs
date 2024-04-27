using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Reqnroll.FeatureSourceGenerator.CSharp;

namespace Reqnroll.FeatureSourceGenerator.MSTest;

public class MSTestFeatureSourceGeneratorTests
{
    [Fact]
    public void GeneratorProducesMSTestOutputWhenWhenBuildPropertyConfiguredForMSTest()
    {
        var references = AppDomain.CurrentDomain.GetAssemblies()
            .Where(asm => !asm.IsDynamic)
            .Select(asm => MetadataReference.CreateFromFile(asm.Location));

        var compilation = CSharpCompilation.Create("test", references: references);

        var generator = new CSharpTestFixtureSourceGenerator([BuiltInTestFrameworkHandlers.MSTest]);

        const string featureText =
            """
            #language: en
            @featureTag1
            Feature: Calculator

            @mytag
            Scenario: Add two numbers
                Given the first number is 50
                And the second number is 70
                When the two numbers are added
                Then the result should be 120
            """;

        var optionsProvider = new FakeAnalyzerConfigOptionsProvider(
            new InMemoryAnalyzerConfigOptions(new Dictionary<string, string>
            {
                { "build_property.ReqnrollTargetTestFramework", "MSTest" }
            }));

        var driver = CSharpGeneratorDriver
            .Create(generator)
            .AddAdditionalTexts([new FeatureFile("Calculator.feature", featureText)])
            .WithUpdatedAnalyzerConfigOptions(optionsProvider)
            .RunGeneratorsAndUpdateCompilation(compilation, out var generatedCompilation, out var diagnostics);

        diagnostics.Should().BeEmpty();

        var generatedSyntaxTree = generatedCompilation.SyntaxTrees.Should().ContainSingle()
            .Which.Should().BeAssignableTo<CSharpSyntaxTree>().Subject!;

        generatedSyntaxTree.GetDiagnostics().Should().BeEmpty();

        generatedSyntaxTree.GetRoot().Should().ContainSingleNamespaceDeclaration("test")
            .Which.Should().ContainSingleClassDeclaration("CalculatorFeature")
            .Which.Should().HaveSingleAttribute("global::Microsoft.VisualStudio.TestTools.UnitTesting.TestClass");
    }

    [Fact]
    public void GeneratorProducesMSTestOutputWhenWhenEditorConfigConfiguredForMSTest()
    {
        var references = AppDomain.CurrentDomain.GetAssemblies()
            .Where(asm => !asm.IsDynamic)
            .Select(asm => MetadataReference.CreateFromFile(asm.Location));

        var compilation = CSharpCompilation.Create("test", references: references);

        var generator = new CSharpTestFixtureSourceGenerator([BuiltInTestFrameworkHandlers.MSTest]);

        const string featureText =
            """
            #language: en
            @featureTag1
            Feature: Calculator

            @mytag
            Scenario: Add two numbers
                Given the first number is 50
                And the second number is 70
                When the two numbers are added
                Then the result should be 120
            """;

        var optionsProvider = new FakeAnalyzerConfigOptionsProvider(
            new InMemoryAnalyzerConfigOptions(new Dictionary<string, string>
            {
                { "reqnroll.target_test_framework", "MSTest" }
            }));

        var driver = CSharpGeneratorDriver
            .Create(generator)
            .AddAdditionalTexts([new FeatureFile("Calculator.feature", featureText)])
            .WithUpdatedAnalyzerConfigOptions(optionsProvider)
            .RunGeneratorsAndUpdateCompilation(compilation, out var generatedCompilation, out var diagnostics);

        diagnostics.Should().BeEmpty();

        var generatedSyntaxTree = generatedCompilation.SyntaxTrees.Should().ContainSingle()
            .Which.Should().BeAssignableTo<CSharpSyntaxTree>().Subject!;

        generatedSyntaxTree.GetDiagnostics().Should().BeEmpty();

        generatedSyntaxTree.GetRoot().Should().ContainSingleNamespaceDeclaration("test")
            .Which.Should().ContainSingleClassDeclaration("CalculatorFeature")
            .Which.Should().HaveSingleAttribute("global::Microsoft.VisualStudio.TestTools.UnitTesting.TestClass");
    }
}
