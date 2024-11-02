using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Reqnroll.FeatureSourceGenerator;
using Reqnroll.FeatureSourceGenerator.CSharp;
using Xunit.Abstractions;

namespace Reqnroll.FeatureSourceGenerator.NUnit;

public class NUnitFeatureSourceGeneratorTests(ITestOutputHelper output)
{
    [Fact]
    public void GeneratorProducesNUnitOutputWhenWhenBuildPropertyConfiguredForNUnit()
    {
        var references = AppDomain.CurrentDomain.GetAssemblies()
            .Where(asm => !asm.IsDynamic)
            .Select(asm => MetadataReference.CreateFromFile(asm.Location));

        var compilation = CSharpCompilation.Create("test", references: references);

        var generator = new CSharpTestFixtureSourceGenerator([BuiltInTestFrameworkHandlers.NUnit]);

        const string featureText =
            """
            #language: en
            @featureTag1
            Feature: Calculator

            @mytag
            Scenario: Add two "simple" numbers
                Given the first number is 50
                And the second number is 70
                When the two numbers are added
                Then the result should be 120
            """;

        var optionsProvider = new FakeAnalyzerConfigOptionsProvider(
            new InMemoryAnalyzerConfigOptions(new Dictionary<string, string>
            {
                { "build_property.ReqnrollTargetTestFramework", "NUnit" }
            }));

        var driver = CSharpGeneratorDriver
            .Create(generator)
            .AddAdditionalTexts([new FeatureFile("Calculator.feature", featureText)])
            .WithUpdatedAnalyzerConfigOptions(optionsProvider)
            .RunGeneratorsAndUpdateCompilation(compilation, out var generatedCompilation, out var diagnostics);

        var generatedSyntaxTree = generatedCompilation.SyntaxTrees.Should().ContainSingle()
            .Which.Should().BeAssignableTo<CSharpSyntaxTree>().Subject!;

        output.WriteLine($"Generated source:\n{generatedSyntaxTree}");

        diagnostics.Should().BeEmpty();

        generatedSyntaxTree.GetDiagnostics().Should().BeEmpty();

        generatedSyntaxTree.GetRoot().Should().ContainSingleNamespaceDeclaration("test")
            .Which.Should().ContainSingleClassDeclaration("CalculatorFeature")
            .Which.Should().HaveSingleAttribute("global::NUnit.Framework.Description");
    }

    [Fact]
    public void GeneratorProducesNUnitOutputWhenWhenEditorConfigConfiguredForNUnit()
    {
        var references = AppDomain.CurrentDomain.GetAssemblies()
            .Where(asm => !asm.IsDynamic)
            .Select(asm => MetadataReference.CreateFromFile(asm.Location));

        var compilation = CSharpCompilation.Create("test", references: references);

        var generator = new CSharpTestFixtureSourceGenerator([BuiltInTestFrameworkHandlers.NUnit]);

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
                { "reqnroll.target_test_framework", "NUnit" }
            }));

        var driver = CSharpGeneratorDriver
            .Create(generator)
            .AddAdditionalTexts([new FeatureFile("Calculator.feature", featureText)])
            .WithUpdatedAnalyzerConfigOptions(optionsProvider)
            .RunGeneratorsAndUpdateCompilation(compilation, out var generatedCompilation, out var diagnostics);

        var generatedSyntaxTree = generatedCompilation.SyntaxTrees.Should().ContainSingle()
            .Which.Should().BeAssignableTo<CSharpSyntaxTree>().Subject!;

        output.WriteLine($"Generated source:\n{generatedSyntaxTree}");

        diagnostics.Should().BeEmpty();

        generatedSyntaxTree.GetDiagnostics().Should().BeEmpty();

        generatedSyntaxTree.GetRoot().Should().ContainSingleNamespaceDeclaration("test")
            .Which.Should().ContainSingleClassDeclaration("CalculatorFeature")
            .Which.Should().HaveSingleAttribute("global::NUnit.Framework.Description");
    }
}
