namespace Reqnroll.CodeAnalysis.Gherkin.Syntax;

using static SyntaxFactory;

public class TableCellSyntaxListTests
{
    [Fact]
    public static void CreateThrowsForSequenceWithNoSeparators()
    {
        var action = () => TableCellSyntaxList.Create([ TextTableCell("Party"), TextTableCell("Hard") ]);

        action.Should().Throw<ArgumentException>()
            .And.ParamName.Should().Be("nodes");
    }

    [Fact]
    public static void CreateThrowsForSequenceEndingWithASeparator()
    {
        var action = () => TableCellSyntaxList.Create([
            TextTableCell("Party"),
            Token(SyntaxKind.VerticalBarToken),
            TextTableCell("Hard"),
            Token(SyntaxKind.VerticalBarToken)
        ]);

        action.Should().Throw<ArgumentException>()
            .And.ParamName.Should().Be("nodes");
    }

    [Fact]
    public static void CreateWithEmptySequenceCreatesList()
    {
        var list = TableCellSyntaxList.Create([]);

        list.Should().BeEmpty();
    }

    [Fact]
    public static void CreateWithOneNodeCreatesList()
    {
        var singleNode = TextTableCell("Party");
        var list = TableCellSyntaxList.Create([ singleNode ]);
        
        list.Should().HaveCount(1);

        list[0].ToString().Should().Be("Party");
    }

    [Fact]
    public static void CreateWithTwoSeparatedNodesCreatesList()
    {
        var list = TableCellSyntaxList.Create([
            TextTableCell("Party"),
            Token(SyntaxKind.VerticalBarToken),
            TextTableCell("Hard")
        ]);

        list.Should().HaveCount(3);

        list[0].ToString().Should().Be("Party");
        list[1].ToString().Should().Be("|");
        list[2].ToString().Should().Be("Hard");
    }

    [Fact]
    public static void CreateWithNullNodeCreatesList()
    {
        var list = TableCellSyntaxList.Create([
            TextTableCell("Party"),
            Token(SyntaxKind.VerticalBarToken),
            null
        ]);

        list.Should().HaveCount(3);

        list[0].ToString().Should().Be("Party");
        list[1].ToString().Should().Be("|");
        list[2].ToString().Should().Be("");
    }

    [Fact]
    public static void EnumeratorReturnsAllNodes()
    {
        var list = TableCellList([TextTableCell("Party"), TextTableCell("Hard")]);

        list.Should().HaveCount(3);
    }

    [Fact]
    public static void EnumeratorReturnsNodesInOrder()
    {
        var list = TableCellList([TextTableCell("Party"), TextTableCell("Hard")]);

        var result = list.ToList();

        result[0].ToString().Should().Be("Party");
        result[1].ToString().Should().Be("|");
        result[2].ToString().Should().Be("Hard");
    }

    [Fact]
    public static void AddSingleCellAddsBarSeparatorBetweenAddedNodeAndList()
    {
        var firstCell = TextTableCell("Party");
        var secondCell = TextTableCell("Hard");

        var list = TableCellList([firstCell]);

        var extended = list.Cells.Add(secondCell);

        extended.Should().HaveCount(3);

        extended[0].ToString().Should().Be("Party");
        extended[1].ToString().Should().Be("|");
        extended[2].ToString().Should().Be("Hard");
    }

    [Fact]
    public static void InsertCellsAddsBarSeparatorsBetweenInsertedNodes()
    {
        var firstCell = TextTableCell("Uno");
        var secondCell = TextTableCell("Dos");
        var thirdCell = TextTableCell("Tres");

        var list = TableCellList([firstCell, thirdCell]);

        var extended = list.Cells.InsertRange(1, [secondCell]);

        extended[0].ToString().Should().Be("Uno");
        extended[1].ToString().Should().Be("|");
        extended[2].ToString().Should().Be("Dos");
        extended[3].ToString().Should().Be("|");
        extended[4].ToString().Should().Be("Tres");
    }

    [Fact]
    public static void InsertCellsBeyondExistingCellsThrowsArgumentOutOfRangeException()
    {
        var firstCell = TextTableCell("Uno");
        var secondCell = TextTableCell("Dos");
        var thirdCell = TextTableCell("Tres");

        var list = TableCellList([firstCell, secondCell]);

        list.Cells.Invoking(cells => cells.InsertRange(3, [secondCell])).Should().Throw<ArgumentOutOfRangeException>();
    }

    [Fact]
    public static void CellsReturnsCellValuesInOrder()
    {
        var list = TableCellSyntaxList.Create([
            TextTableCell("Party"),
            Token(SyntaxKind.VerticalBarToken),
            TextTableCell("Hard")
        ]);

        list.Cells.Should().HaveCount(2);
        list.Cells[0].ToString().Should().Be("Party");
        list.Cells[1].ToString().Should().Be("Hard");
    }

    [Fact]
    public static void CellsReturnsEmptyCellValues()
    {
        var list = TableCellSyntaxList.Create([
            TextTableCell("Party"),
            Token(SyntaxKind.VerticalBarToken),
            EmptyTableCell()
        ]);
        
        list.Cells.Should().HaveCount(2);
        list.Cells[0].ToString().Should().Be("Party");
        list.Cells[1].ToString().Should().Be("");
    }
}
