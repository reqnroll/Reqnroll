namespace Reqnroll.CodeAnalysis.Gherkin.Syntax;

using static SyntaxFactory;

public class SyntaxNodeOrTokenListTests
{
    [Fact]
    public void CreateWithSpanOfNodesCreatesList()
    {
        var first = LiteralText("Alpha");
        var second = Token(SyntaxKind.VerticalBarToken);
        var third = LiteralText("Beta");

        var list = SyntaxNodeOrTokenList.Create<PlainTextSyntax>([first, second, third]);

        list.Should().HaveCount(3);
        list.Count.Should().Be(3);

        list[0].Should().BeEquivalentTo(new SyntaxNodeOrToken<PlainTextSyntax>(first));
        list[1].Should().BeEquivalentTo(new SyntaxNodeOrToken<PlainTextSyntax>(second));
        list[2].Should().BeEquivalentTo(new SyntaxNodeOrToken<PlainTextSyntax>(third));
    }
}
