using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;

namespace Reqnroll.StepBindingSourceGenerator;

public class StepAnalysisTests
{
    [Fact]
    public void StepBindingWithEmptyText_GeneratesErrorStepTextCannotBeEmpty()
    {
        var source = MarkedSourceText.Parse(
            """
            using Reqnroll;

            namespace Sample.Tests;

            [Binding]
            public class GameSteps
            {
                [When(⇥""⇤)]
                public void WhenMakerStartsAGame()
                {
                }
            }
            """);

        var syntaxTree = CSharpSyntaxTree.ParseText(source, path: "/spec/Sample.feature");

        syntaxTree.GetDiagnostics().Should().BeEmpty();

        var references = AppDomain.CurrentDomain
            .GetAssemblies()
            .Where(assembly => !assembly.IsDynamic)
            .Select(assembly => MetadataReference.CreateFromFile(assembly.Location))
            .Cast<MetadataReference>()
            .ToList();

        references.Add(MetadataReference.CreateFromFile(typeof(WhenAttribute).Assembly.Location));

        var compilation = CSharpCompilation.Create(
            "SourceGeneratorTests",
            [ syntaxTree ],
            references,
            new CSharpCompilationOptions(OutputKind.DynamicallyLinkedLibrary));

        compilation.GetDiagnostics().Should().BeEmpty();

        var generator = new CSharpStepBindingGenerator();
        var result = CSharpGeneratorDriver.Create(generator).RunGenerators(compilation).GetRunResult();

        result.Diagnostics.Should().BeEquivalentTo(
            [
                Diagnostic.Create(
                    DiagnosticDescriptors.ErrorStepTextCannotBeEmpty,
                    source.GetMarkedLocation("/spec/Sample.feature"))
            ]);

        result.GeneratedTrees.Should().BeEmpty();
    }
}
