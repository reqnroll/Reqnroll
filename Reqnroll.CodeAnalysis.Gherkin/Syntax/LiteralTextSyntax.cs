namespace Reqnroll.CodeAnalysis.Gherkin.Syntax;

/// <summary>
/// Represents plain, non-structural text.
/// </summary>
[SyntaxNode(SyntaxKind.LiteralText)]
public partial class LiteralTextSyntax : PlainTextSyntax
{
    [SyntaxSlot(SyntaxKind.LiteralToken, "The text of the step following the keyword.")]
    public partial SyntaxTokenList Text { get; }
}

[SyntaxNode]
public abstract partial class TableCellSyntax : SyntaxNode
{
}

[SyntaxNode(SyntaxKind.EmptyTableCell)]
public partial class TableEmptyCellSyntax : TableCellSyntax
{
}

[SyntaxNode(SyntaxKind.TextTableCell)]
public partial class TableTextCellSyntax : TableCellSyntax
{
    [SyntaxSlot(SyntaxKind.TableLiteralToken, "The text of the cell.")]
    public partial SyntaxToken Text { get; }
}
