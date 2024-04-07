using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;

namespace Reqnroll.FeatureSourceGenerator.MSTest;

public class MSTestFeatureSourceGeneratorTests
{
    [Fact]
    public void GeneratorProducesMSTestOutput()
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

        var driver = CSharpGeneratorDriver
            .Create(generator)
            .AddAdditionalTexts([new FeatureFile("Calculator.feature", featureText)])
            .RunGeneratorsAndUpdateCompilation(compilation, out var generatedCompilation, out var diagnostics);

        diagnostics.Should().BeEmpty();

        generatedCompilation.SyntaxTrees.Should().HaveCount(1);
        var generatedSyntaxTree = generatedCompilation.SyntaxTrees.Single().Should().BeAssignableTo<CSharpSyntaxTree>().Subject!;

        generatedSyntaxTree.GetDiagnostics().Should().BeEmpty();

        var generatedSyntaxRoot = (CompilationUnitSyntax)generatedSyntaxTree.GetRoot();
        generatedSyntaxRoot.ChildNodes().Should().HaveCount(1);

        var generatedNamespace = generatedSyntaxRoot.ChildNodes().Single().Should().BeAssignableTo<NamespaceDeclarationSyntax>().Subject;
        generatedNamespace.Name.ToString().Should().Be("test");

        generatedNamespace.Members.Should().HaveCount(1);
        var generatedClass = generatedNamespace.Members.Single().Should().BeAssignableTo<ClassDeclarationSyntax>().Subject;

        generatedClass.AttributeLists.Should().HaveCount(1);
        var generatedClassAttributes = generatedClass.AttributeLists.Single().Attributes;

        generatedClassAttributes.Should().HaveCount(1);
        var generatedClassAttribute = generatedClassAttributes.Single();

        generatedClassAttribute.Name.ToString().Should().Be("global::Microsoft.VisualStudio.TestTools.UnitTesting.TestClass");
    }
}
