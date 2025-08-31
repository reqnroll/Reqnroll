namespace Reqnroll.CodeAnalysis.Gherkin.Syntax;

/// <summary>
/// Background declaration syntax.
/// </summary>
[SyntaxNode(SyntaxKind.Background)]
[SyntaxConstructor(nameof(BackgroundKeyword), nameof(Name), nameof(Steps))]
[SyntaxConstructor(nameof(BackgroundKeyword), nameof(ColonToken), nameof(Name), nameof(Description), nameof(Steps))]
public sealed partial class BackgroundSyntax : BehaviorDeclarationSyntax
{
    [SyntaxSlot(
        SyntaxKind.BackgroundKeyword,
        "The token that represents the \"Background\" keyword.",
        LocatedAfter = nameof(Tags))]
    public partial SyntaxToken BackgroundKeyword { get; }

    /// <inheritdoc />
    public override SyntaxToken GetKeywordToken() => BackgroundKeyword;
}
