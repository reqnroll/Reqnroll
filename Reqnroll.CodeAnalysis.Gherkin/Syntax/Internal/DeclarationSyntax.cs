using Microsoft.CodeAnalysis;
using System.Collections.Immutable;

namespace Reqnroll.CodeAnalysis.Gherkin.Syntax.Internal;

internal abstract class DeclarationSyntax : RawNode
{
    protected DeclarationSyntax(SyntaxKind kind) : base(kind)
    {
    }

    protected DeclarationSyntax(
        SyntaxKind kind,
        ImmutableArray<InternalDiagnostic> diagnostics,
        ImmutableArray<SyntaxAnnotation> annotations) : base(kind, diagnostics, annotations)
    {
    }

    public override SyntaxNode CreateSyntaxNode(SyntaxNode? parent, int position)
    {
        throw new NotImplementedException();
    }
}
