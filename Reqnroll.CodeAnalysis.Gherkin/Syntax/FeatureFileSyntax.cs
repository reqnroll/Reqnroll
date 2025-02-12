using Reqnroll.CodeAnalysis.Gherkin.Syntax.Internal;

namespace Reqnroll.CodeAnalysis.Gherkin.Syntax;

/// <summary>
/// Represents the structure of a Gherkin feature file.
/// </summary>
public class FeatureFileSyntax : SyntaxNode
{
    private FeatureDeclarationSyntax? _featureDeclaration;

    internal FeatureFileSyntax(Internal.FeatureFileSyntax node) : base(node)
    {
    }

    internal FeatureFileSyntax(Internal.FeatureFileSyntax node, SyntaxNode? parent, int position) : base(node, parent, position)
    {
    }

    /// <summary>
    /// Gets the feature declaration syntax contained by the file structure.
    /// </summary>
    public FeatureDeclarationSyntax? FeatureDeclaration => GetSyntaxNode(ref _featureDeclaration, 0);

    /// <summary>
    /// Gets the token which represents the end of the source file.
    /// </summary>
    public SyntaxToken EndOfFileToken => new(this, ((Internal.FeatureFileSyntax)RawNode).endOfFile, Position + RawNode.GetSlotOffset(1));

    public FeatureFileSyntax WithEndOfFileToken(SyntaxToken endOfFileToken)
    {
        if (endOfFileToken == EndOfFileToken)
        {
            return this;
        }

        return SyntaxFactory.FeatureFile(FeatureDeclaration, endOfFileToken);
    }

    internal override SyntaxNode? GetSlotAsSyntaxNode(int index)
    {
        return index switch
        {
            0 => FeatureDeclaration,
            _ => null
        };
    }
}
