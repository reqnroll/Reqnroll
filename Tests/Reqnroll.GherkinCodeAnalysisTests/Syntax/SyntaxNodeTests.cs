using FluentAssertions;

namespace Reqnroll.CodeAnalysis.Gherkin.Syntax;

using static SyntaxFactory;

/// <summary>
/// Although we can't directly test the behaviour of <see cref="SyntaxNode"/> because it's the abstract base for all
/// concrete syntax nodes, there are some things we'd like to validate in isolation using some simple implementations.
/// </summary>
public class SyntaxNodeTests
{
    [Fact]
    public void GetText_ReturnsSourceTextContainingNodeSyntax()
    {
        var node = Description(Literal("test"));

        var text = node.GetText();

        text.ToString().Should().Be("test");
    }
}
