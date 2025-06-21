namespace Reqnroll.CodeAnalysis.Gherkin.Syntax;

/// <summary>
/// Represents a row in a table syntax structure.
/// </summary>
[SyntaxNode(SyntaxKind.TableRow)]
public partial class TableRowSyntax : SyntaxNode
{
    [SyntaxSlot(SyntaxKind.VerticalBarToken, "The token that marks the start of the row.")]
    public partial SyntaxToken StartVerticalBarToken { get; }

    [SyntaxSlot(SyntaxKind.LiteralText, "The values in the row.")]
    public partial SeparatedSyntaxList<PlainTextSyntax> Values { get; }

    [SyntaxSlot(SyntaxKind.VerticalBarToken, "The token that marks the end of the header row.")]
    public partial SyntaxToken EndVerticalBarToken { get; }
}
