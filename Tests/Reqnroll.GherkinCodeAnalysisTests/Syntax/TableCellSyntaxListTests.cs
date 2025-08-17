namespace Reqnroll.CodeAnalysis.Gherkin.Syntax;

using static SyntaxFactory;

public class TableCellSyntaxListTests
{
    [Fact]
    public static void CreateThrowsForSequenceWithNoSeparators()
    {
        var action = () => TableCellSyntaxList.Create([ LiteralText("Party"), LiteralText("Hard") ]);

        action.Should().Throw<ArgumentException>()
            .And.ParamName.Should().Be("nodes");
    }

    [Fact]
    public static void CreateThrowsForSequenceEndingWithASeparator()
    {
        var action = () => TableCellSyntaxList.Create([
            LiteralText("Party"),
            Token(SyntaxKind.VerticalBarToken),
            LiteralText("Hard"),
            Token(SyntaxKind.VerticalBarToken)
        ]);

        action.Should().Throw<ArgumentException>()
            .And.ParamName.Should().Be("nodes");
    }

    [Fact]
    public static void CreateWithEmptySequenceCreatesList()
    {
        var list = TableCellSyntaxList.Create([]);

        list.Count.Should().Be(0);
    }

    [Fact]
    public static void CreateWithOneNodeCreatesList()
    {
        var singleNode = LiteralText("Party");
        var list = TableCellSyntaxList.Create([ singleNode ]);

        list.Count.Should().Be(1);

        list[0].ToString().Should().Be("Party");
    }

    [Fact]
    public static void CreateWithTwoSeparatedNodesCreatesList()
    {
        var list = TableCellSyntaxList.Create([
            LiteralText("Party"),
            Token(SyntaxKind.VerticalBarToken),
            LiteralText("Hard")
        ]);

        list.Count.Should().Be(3);

        list[0].ToString().Should().Be("Party");
        list[1].ToString().Should().Be("|");
        list[2].ToString().Should().Be("Hard");
    }

    [Fact]
    public static void CreateWithNullNodeCreatesList()
    {
        var list = TableCellSyntaxList.Create([
            LiteralText("Party"),
            Token(SyntaxKind.VerticalBarToken),
            null
        ]);

        list.Count.Should().Be(3);

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
    public static void AddWithSingleNodeAddsBarSeparatorBetweenAddedNodes()
    {
        var firstNode = TextTableCell("Party");
        var secondNode = TextTableCell("Hard");

        var list = TableCellList([firstNode]);

        var extended = list.Cells.Add(secondNode);

        extended.Count.Should().Be(3);

        extended[0].ToString().Should().Be("Party");
        extended[1].ToString().Should().Be("|");
        extended[2].ToString().Should().Be("Hard");
    }

    [Fact]
    public static void ValuesReturnsCellValuesInOrder()
    {
        var list = TableCellSyntaxList.Create([
            LiteralText("Party"),
            Token(SyntaxKind.VerticalBarToken),
            LiteralText("Hard")
        ]);

        list.Cells.Count.Should().Be(2);
        list.Cells[0]!.ToString().Should().Be("Party");
        list.Cells[1]!.ToString().Should().Be("Hard");
    }

    [Fact]
    public static void ValuesReturnsEmptyCellValues()
    {
        var list = TableCellSyntaxList.Create([
            LiteralText("Party"),
            Token(SyntaxKind.VerticalBarToken),
            null
        ]);

        list.Cells.Count.Should().Be(2);
        list.Cells[0]!.ToString().Should().Be("Party");
        list.Cells[1].Should().BeNull();
    }
}
