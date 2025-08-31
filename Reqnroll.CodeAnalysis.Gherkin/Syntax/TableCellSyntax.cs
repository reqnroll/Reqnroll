namespace Reqnroll.CodeAnalysis.Gherkin.Syntax;

[SyntaxNode(SyntaxKind.TableCell)]
public partial class TableCellSyntax : SyntaxNode
{
    [SyntaxSlot(SyntaxKind.TableLiteralToken, "The text of the cell.")]
    public partial SyntaxToken Text { get; }

    /// <summary>
    /// Gets whether the cell contains a value.
    /// </summary>
    public bool HasValue => !Text.IsMissing;

    /// <summary>
    /// Gets the value contained by the cell (if it has one).
    /// </summary>
    public object? Value => HasValue ? Text.Value : default;
}
