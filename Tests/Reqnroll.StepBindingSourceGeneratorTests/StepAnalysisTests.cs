using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.Text;

namespace Reqnroll.StepBindingSourceGenerator;

public class StepAnalysisTests
{
    [Fact]
    public void StepBindingWithEmptyText_GeneratesErrorStepTextCannotBeEmpty()
    {
        const string source =
            """
            using Reqnroll;

            namespace Sample.Tests;

            [Binding]
            public class GameSteps
            {
                [Reqnroll.WhenAttribute("test")]
                public void WhenMakerStartsAGame()
                {
                }
            }
            """;

        var syntaxTree = CSharpSyntaxTree.ParseText(source);

        syntaxTree.GetDiagnostics().Should().BeEmpty();

        var references = AppDomain.CurrentDomain
            .GetAssemblies()
            .Where(assembly => !assembly.IsDynamic)
            .Select(assembly => MetadataReference.CreateFromFile(assembly.Location))
            .Cast<MetadataReference>();

        var compilation = CSharpCompilation.Create(
            "SourceGeneratorTests",
            [ syntaxTree ],
            references,
            new CSharpCompilationOptions(OutputKind.DynamicallyLinkedLibrary));

        var generator = new CSharpStepBindingGenerator();
        var result = CSharpGeneratorDriver.Create(generator).RunGenerators(compilation).GetRunResult();

        result.Diagnostics.Should().BeEquivalentTo([
            Diagnostic.Create(DiagnosticDescriptors.ErrorStepTextCannotBeEmpty,
            syntaxTree.GetLocation(new TextSpan(100, 2)))]);

        result.GeneratedTrees.Should().BeEmpty();
    }
}
