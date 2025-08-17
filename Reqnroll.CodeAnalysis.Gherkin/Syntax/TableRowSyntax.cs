namespace Reqnroll.CodeAnalysis.Gherkin.Syntax;

/// <summary>
/// Represents a row in a table syntax structure.
/// </summary>
[SyntaxNode(SyntaxKind.TableRow)]
public sealed partial class TableRowSyntax : SyntaxNode
{
    [SyntaxSlot(SyntaxKind.VerticalBarToken, "The token that marks the start of the row.")]
    public partial SyntaxToken StartVerticalBarToken { get; }

    [SyntaxSlot(SyntaxKind.LiteralText, "The values in the row.")]
    public partial TableCellSyntaxList Values { get; }

    [SyntaxSlot(SyntaxKind.VerticalBarToken, "The token that marks the end of the header row.")]
    public partial SyntaxToken EndVerticalBarToken { get; }
}
