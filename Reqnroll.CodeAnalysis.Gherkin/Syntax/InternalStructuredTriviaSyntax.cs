using Microsoft.CodeAnalysis;
using System.Collections.Immutable;

namespace Reqnroll.CodeAnalysis.Gherkin.Syntax;

/// <summary>
/// Represents the internal state of structured trivia syntax.
/// </summary>
internal abstract class InternalStructuredTriviaSyntax : InternalNode
{
    protected InternalStructuredTriviaSyntax(SyntaxKind kind) : base(kind)
    {
        SetFlag(NodeFlags.ContainsStructuredTrivia);
    }

    protected InternalStructuredTriviaSyntax(
        SyntaxKind kind,
        ImmutableArray<InternalDiagnostic> diagnostics,
        ImmutableArray<SyntaxAnnotation> annotations) : base(kind, diagnostics, annotations)
    {
        SetFlag(NodeFlags.ContainsStructuredTrivia);
    }

    /// <inheritdoc />
    public override bool IsStructuredTrivia => true;
}
