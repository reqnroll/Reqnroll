using Gherkin.CucumberMessages.Types;
using Microsoft.CodeAnalysis.Text;
using Reqnroll.CodeAnalysis.Gherkin.Assertions;

namespace Reqnroll.CodeAnalysis.Gherkin.Syntax;

using static SyntaxFactory;

public class SyntaxNodeOrTokenListTests
{
    [Fact]
    public void CreateWithSpanOfNodesCreatesList()
    {
        var first = TableCell("Alpha");
        var second = Token(SyntaxKind.VerticalBarToken);
        var third = TableCell("Beta");

        var list = SyntaxNodeOrTokenList.Create<TableCellSyntax>([first, second, third]);

        list.Should().HaveCount(3);
        list.Count.Should().Be(3);

        list[0].Should().BeEquivalentTo(
            new SyntaxNodeOrToken<TableCellSyntax>(first),
            options => options.IgnoringSyntaxPositions());
        list[0].AsNode()!.Span.Should().Be(new TextSpan(0, 5));
        list[0].AsNode()!.FullSpan.Should().Be(new TextSpan(0, 5));
        list[0].AsNode()!.Span.Should().Be(new TextSpan(0, 5));
        list[0].AsNode()!.FullSpan.Should().Be(new TextSpan(0, 5));

        list[1].Should().BeEquivalentTo(
            new SyntaxNodeOrToken<TableCellSyntax>(second),
            options => options.IgnoringSyntaxPositions());
        list[1].AsToken().Span.Should().Be(new TextSpan(5, 1));
        list[1].AsToken().FullSpan.Should().Be(new TextSpan(5, 1));

        list[2].Should().BeEquivalentTo(
            new SyntaxNodeOrToken<TableCellSyntax>(third),
            options => options.IgnoringSyntaxPositions());
        list[2].AsNode()!.Span.Should().Be(new TextSpan(6, 4));
        list[2].AsNode()!.FullSpan.Should().Be(new TextSpan(6, 4));
        list[2].AsNode()!.Span.Should().Be(new TextSpan(6, 4));
        list[2].AsNode()!.FullSpan.Should().Be(new TextSpan(6, 4));
    }
}
