using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.Text;

namespace Reqnroll.StepBindingSourceGenerator;

public class GeneratorDriver
{
    public static GeneratorDriverRunResult RunGenerator(SourceText source, string path)
    {
        var syntaxTree = CSharpSyntaxTree.ParseText(source, path: path);

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
            [syntaxTree],
            references,
            new CSharpCompilationOptions(OutputKind.DynamicallyLinkedLibrary));

        compilation.GetDiagnostics().Should().BeEmpty();

        var generator = new CSharpStepBindingGenerator();
        
        return CSharpGeneratorDriver.Create(generator).RunGenerators(compilation).GetRunResult();
    }
}
