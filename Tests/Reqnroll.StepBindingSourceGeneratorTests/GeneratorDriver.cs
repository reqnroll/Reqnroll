using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.Text;
using Reqnroll.Bindings;
using System.Collections.Immutable;

namespace Reqnroll.StepBindingSourceGenerator;

public class GeneratorDriver
{
    public static GeneratorResult RunGenerator(SourceText source, string path)
    {
        var syntaxTree = CSharpSyntaxTree.ParseText(source, path: path);

        syntaxTree.GetDiagnostics().Should().BeEmpty();

        // Build up the minimal set of references required to compile our simple sample sources.
        var assemblies = AppDomain.CurrentDomain
            .GetAssemblies()
            .Where(assembly => assembly.FullName!.StartsWith("netstandard, ")
                || assembly.FullName!.StartsWith("System.Runtime, "))
            .ToHashSet();

        assemblies.Add(typeof(object).Assembly);
        assemblies.Add(typeof(NotImplementedException).Assembly);
        assemblies.Add(typeof(ImmutableArray).Assembly);

        // Reqnroll.dll
        assemblies.Add(typeof(StepDefinitionRegistryAttribute).Assembly);

        var references = assemblies
            .Select(assembly => MetadataReference.CreateFromFile(assembly.Location))
            .ToImmutableArray();

        var compilation = CSharpCompilation.Create(
            "SourceGeneratorTests",
            [syntaxTree],
            references,
            new CSharpCompilationOptions(OutputKind.DynamicallyLinkedLibrary));

        compilation.GetDiagnostics().Should().BeEmpty();

        var generator = new CSharpStepBindingGenerator();

        var runResult = CSharpGeneratorDriver.Create(generator)
            .RunGeneratorsAndUpdateCompilation(compilation, out var outputCompilation, out var diagnostics)
            .GetRunResult();

        return new GeneratorResult((CSharpCompilation)outputCompilation, diagnostics, runResult.GeneratedTrees);
    }
}
