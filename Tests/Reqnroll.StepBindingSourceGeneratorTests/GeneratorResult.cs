using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using System.Collections.Immutable;
using System.Runtime.Loader;

namespace Reqnroll.StepBindingSourceGenerator;

public class GeneratorResult(
    CSharpCompilation compilation,
    ImmutableArray<Diagnostic> diagnostics,
    ImmutableArray<SyntaxTree> generatedTrees)
{
    public ImmutableArray<Diagnostic> Diagnostics { get; } = diagnostics;

    public ImmutableArray<SyntaxTree> GeneratedTrees { get; } = generatedTrees;

    public AssemblyTestContext CompileAssembly()
    {
        using var buffer = new MemoryStream();

        var result = compilation.Emit(buffer);

        if (!result.Success)
        {
            throw new InvalidOperationException(
                "Failed to compile generated code.\nDiagnostics:\n  " + 
                string.Join("\n  ", result.Diagnostics.Select(diag => diag.ToString())));
        }

        buffer.Seek(0, SeekOrigin.Begin);

        var assemblyLoadContext = new AssemblyLoadContext(nameof(AssemblyTestContext), true);
        var assembly = assemblyLoadContext.LoadFromStream(buffer);

        return new AssemblyTestContext(assemblyLoadContext, assembly);
    }
}
