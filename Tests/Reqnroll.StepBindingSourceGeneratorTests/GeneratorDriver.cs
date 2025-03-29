using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.Text;
using System.Collections.Immutable;
using System.Reflection;
using System.Runtime.Loader;

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
        assemblies.Add(typeof(WhenAttribute).Assembly);

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

public class GeneratorResult
{
    private readonly CSharpCompilation _compilation;

    public GeneratorResult(
        CSharpCompilation compilation,
        ImmutableArray<Diagnostic> diagnostics,
        ImmutableArray<SyntaxTree> generatedTrees)
    {
        _compilation = compilation;
        Diagnostics = diagnostics;
        GeneratedTrees = generatedTrees;
    }

    public ImmutableArray<Diagnostic> Diagnostics { get; }

    public ImmutableArray<SyntaxTree> GeneratedTrees { get; }

    public AssemblyTestContext CompileAssembly()
    {
        using var buffer = new MemoryStream();

        var result = _compilation.Emit(buffer);

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

public sealed class AssemblyTestContext : IDisposable
{
    private readonly AssemblyLoadContext _context;

    public Assembly Assembly { get; }

    internal AssemblyTestContext(AssemblyLoadContext context, Assembly assembly)
    {
        _context = context;
        Assembly = assembly;
    }

    public void Dispose()
    {
        _context.Unload();
    }
}
