using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.Text;

namespace Reqnroll.CodeAnalysis.Gherkin.SyntaxGenerator;

/// <summary>
/// Represents a diagnostic produced during source generation.
/// </summary>
/// <remarks>
/// <para>This type exists to allow diganostic information to be included in the output from a generator's pipeline stage.
/// Using the <see cref="Diagnostic"/> directly includes references to elements like the syntax tree
/// which will never be equal between generations and breaks the equality checks required to enable the incremental
/// source generation pipeline to skip generation stages.</para>
/// </remarks>
/// <param name="SourcePath">The path to the source file which has the problem.</param>
/// <param name="TextSpan">The text span within the file which </param>
/// <param name="Descriptor">The descriptor that describes the problem.</param>
/// <param name="MessageArgs">The arguments that will be passed to the descriptor to describe the problem.</param>
internal record GenerationDiagnostic(
    string SourcePath,
    TextSpan TextSpan,
    DiagnosticDescriptor Descriptor,
    ComparableArray<string> MessageArgs)
{
    internal Diagnostic ToDiagnostic(Compilation compilation)
    {
        var syntaxTree = compilation.SyntaxTrees.FirstOrDefault(st => st.FilePath == SourcePath);

        var location = syntaxTree == null ? Location.None : Location.Create(syntaxTree, TextSpan);

        return Diagnostic.Create(Descriptor, location, MessageArgs.ToArray<object>());
    }
}
