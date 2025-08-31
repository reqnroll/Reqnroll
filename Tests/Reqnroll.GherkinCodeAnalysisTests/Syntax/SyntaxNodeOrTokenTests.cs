namespace Reqnroll.CodeAnalysis.Gherkin.Syntax;

public class SyntaxNodeOrTokenTests
{
    [Fact]
    public void EqualsReturnsTrueForSameNode()
    {
        var node = SyntaxFactory.Example("Scenario", "Test");
        var token = SyntaxFactory.Token(SyntaxKind.ExampleKeyword, "Example");

        var syntaxNodeOrToken1 = new SyntaxNodeOrToken<SyntaxNode>(node);
        var syntaxNodeOrToken2 = new SyntaxNodeOrToken<SyntaxNode>(node);
        var syntaxNodeOrToken3 = new SyntaxNodeOrToken<SyntaxNode>(token);

        syntaxNodeOrToken1.Equals(syntaxNodeOrToken2).Should().BeTrue();
        syntaxNodeOrToken1.Equals(syntaxNodeOrToken3).Should().BeFalse();
    }
}
