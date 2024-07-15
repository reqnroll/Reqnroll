using FluentAssertions;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.Diagnostics;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Reflection;
using Xunit.Abstractions;

namespace Reqnroll.Analyzers;

internal class AnalyzerTestHarness<TAnalyzer>(ITestOutputHelper output) where TAnalyzer : DiagnosticAnalyzer, new()
{
    private static readonly ImmutableArray<string> CoreAssemblyNames = 
        [ 
            "System.Runtime.dll",
            "netstandard.dll",
            "mscorlib.dll",
            "System.Private.CoreLib.dll"
        ];

    private static readonly ImmutableArray<MetadataReference> CoreReferences =
        ((string)AppContext.GetData("TRUSTED_PLATFORM_ASSEMBLIES")!)
        .Split(';')
        .Where(assemblyPath => CoreAssemblyNames.Any(assemblyName => assemblyPath.EndsWith(assemblyName)))
        .Select(assemblyPath => MetadataReference.CreateFromFile(assemblyPath))
        .ToImmutableArray<MetadataReference>();

    public Task<AnalysisResult> RunAnalyzerAsync(SyntaxTree source)
    {
        var trustedAssemblies = ((string)AppContext.GetData("TRUSTED_PLATFORM_ASSEMBLIES")!).Split(';');

        var testLocation = Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location)!;

        var dependencies = new List<string>
        {
            "Reqnroll.dll"
        };

        var references = new List<MetadataReference>();
        references.AddRange(CoreReferences);
        references.AddRange(dependencies.Select(dep => MetadataReference.CreateFromFile(Path.Combine(testLocation, dep))));

        var options = new CSharpCompilationOptions(OutputKind.DynamicallyLinkedLibrary);
        var compilation = CSharpCompilation.Create("test", [source], references, options);

        var diagnostics = compilation.GetDiagnostics();

        if (diagnostics.Length > 0)
        {
            output.WriteLine("Compilation diagnostics:");

            foreach (var diagnostic in diagnostics)
            {
                output.WriteLine(diagnostic.ToString());
            }

            Assert.Fail("Source compilation failed.");
        }

        return compilation
            .WithAnalyzers([new TAnalyzer()])
            .GetAnalysisResultAsync(default);
    }
}
