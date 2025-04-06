using Microsoft.CodeAnalysis;
using System.Collections.Immutable;

namespace Reqnroll.CodeAnalysis.Gherkin.Syntax.Internal;

internal class FeatureDeclarationSyntax : DeclarationSyntax
{
    public FeatureDeclarationSyntax(
        InternalNode keyword,
        InternalNode colon,
        InternalNode name,
        DescriptionSyntax? description) : base(SyntaxKind.FeatureDeclaration)
    {
        this.keyword = keyword;
        IncludeChild(keyword);

        this.colon = colon;
        IncludeChild(colon);

        this.name = name;
        IncludeChild(name);

        this.description = description;
        if (description != null)
        {
            IncludeChild(description);
        }
    }

    private FeatureDeclarationSyntax(
        InternalNode keyword,
        InternalNode colon,
        InternalNode name,
        DescriptionSyntax? description,
        ImmutableArray<InternalDiagnostic> diagnostics,
        ImmutableArray<SyntaxAnnotation> annotations) : base(
            SyntaxKind.FeatureDeclaration,
            diagnostics,
            annotations)
    {
        this.keyword = keyword;
        IncludeChild(keyword);

        this.colon = colon;
        IncludeChild(colon);

        this.name = name;
        IncludeChild(name);

        this.description = description;
        if (description != null)
        {
            IncludeChild(description);
        }
    }

    public readonly InternalNode keyword;
    public readonly InternalNode colon;
    public readonly InternalNode name;
    public readonly DescriptionSyntax? description;

    public override SyntaxNode CreateSyntaxNode(SyntaxNode? parent, int position)
    {
        return new Syntax.FeatureDeclarationSyntax(this, parent, position);
    }

    public override InternalNode? GetSlot(int index)
    {
        return index switch
        {
            0 => keyword,
            1 => colon,
            2 => name,
            3 => description,
            _ => null
        };
    }

    public override int SlotCount => 4;

    public override InternalNode WithAnnotations(ImmutableArray<SyntaxAnnotation> annotations)
    {
        return new FeatureDeclarationSyntax(
            keyword,
            colon,
            name,
            description,
            GetAttachedDiagnostics(),
            annotations);
    }

    public override InternalNode WithDiagnostics(ImmutableArray<InternalDiagnostic> diagnostics)
    {
        return new FeatureDeclarationSyntax(
            keyword,
            colon,
            name,
            description,
            diagnostics,
            GetAnnotations());
    }
}
