namespace Reqnroll.CodeAnalysis.Gherkin.Syntax;

[SyntaxNode(SyntaxKind.TextTableCell)]
public partial class TableTextCellSyntax : TableCellSyntax
{
    [SyntaxSlot(SyntaxKind.TableLiteralToken, "The text of the cell.")]
    public partial SyntaxToken Text { get; }
}
