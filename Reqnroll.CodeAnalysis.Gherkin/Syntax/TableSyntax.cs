namespace Reqnroll.CodeAnalysis.Gherkin.Syntax;

/// <summary>
/// Represents a table in a Gherkin syntax tree.
/// </summary>
[SyntaxNode(SyntaxKind.Table)]
public partial class TableSyntax : SyntaxNode
{
    [SyntaxSlot(SyntaxKind.TableRow, "The rows of the table.")]
    public partial SyntaxList<TableRowSyntax> Rows { get; }
}
