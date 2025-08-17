using FluentAssertions;
using Microsoft.CodeAnalysis.Text;

namespace Reqnroll.CodeAnalysis.Gherkin.Syntax;

using static SyntaxFactory;

public class TriviaTests
{
    [Theory]
    [InlineData(" ")]
    [InlineData("    ")]
    public void CanCreateWhitespaceTrivia(string text)
    {
        var trivia = Whitespace(text);

        trivia.ToString().Should().Be(text);
        trivia.Kind.Should().Be(SyntaxKind.WhitespaceTrivia);

        trivia.SyntaxTree.Should().BeNull();
        trivia.Token.Should().Be(default(SyntaxToken));

        trivia.Span.Should().Be(new TextSpan(0, text.Length));
        trivia.FullSpan.Should().Be(trivia.Span);

        trivia.ContainsDiagnostics.Should().BeFalse();
        trivia.HasStructure.Should().BeFalse();

        trivia.ToString().Should().Be(text);
        trivia.ToFullString().Should().Be(text);
    }

    [Fact]
    public void CanCreateSkippedTokensTrivia()
    {
        var skippedTokens = SkippedTokensTrivia(
            TokenList([ 
                Literal("fibble"),
                Literal("wibble"),
                Literal("bibble"),
                Literal("bobble")
            ]));

        var trivia = Trivia(skippedTokens);

        trivia.HasStructure.Should().BeTrue();
        trivia.        Structure.Should().BeEquivalentTo(skippedTokens);
    }

    [Fact]
    public void CanCreateCommentTrivia()
    {
        var text = "# Commentary";

        var trivia = Comment(text);

        trivia.ToString().Should().Be(text);
        trivia.Kind.Should().Be(SyntaxKind.CommentTrivia);

        trivia.SyntaxTree.Should().BeNull();
        trivia.Token.Should().Be(default(SyntaxToken));

        trivia.Span.Should().Be(new TextSpan(0, text.Length));
        trivia.FullSpan.Should().Be(trivia.Span);

        trivia.ContainsDiagnostics.Should().BeFalse();
        trivia.HasStructure.Should().BeFalse();

        trivia.ToString().Should().Be(text);
        trivia.ToFullString().Should().Be(text);
    }

    [Fact]
    public void TriviaAreAttachedToParentTokens()
    {
        var trivia = Comment("# Commentary");

        var literal = Literal(TriviaList([trivia]), "Literally", TriviaList());

        literal.LeadingTrivia[0].Token.Should().Be(literal);
    }

    [Fact]
    public void TriviaAttachedToAParentAreEqualToThemselves()
    {
        var literal = Literal(TriviaList([Comment("# Commentary")]), "Literally", TriviaList(Comment("# Commentary")));

        var trivia1 = literal.LeadingTrivia[0];
        var trivia2 = literal.TrailingTrivia[0];

        trivia1.Equals(trivia1).Should().BeTrue();
        trivia1.Equals(trivia2).Should().BeFalse();
        trivia2.Equals(trivia1).Should().BeFalse();
        trivia2.Equals(trivia2).Should().BeTrue();
    }

    [Fact]
    public void TriviaNotAttachedToTokenAreEqualToThemselves()
    {
        var comment = Comment("# Commentary");

        comment.Equals(comment).Should().BeTrue();
    }

    [Fact]
    public void EquivalentTriviaAreNotEqual()
    {
        var comment1 = Comment("# Commentary");
        var comment2 = Comment("# Commentary");

        comment1.Equals(comment2).Should().BeFalse();
        comment2.Equals(comment1).Should().BeFalse();
    }

    [Fact]
    public void TriviaAttachedToTokensAreNotEqualToUnattachedTrivia()
    {
        var trivia = Comment("# Commentary");

        var literal = Literal(TriviaList([trivia]), "Literally", TriviaList());

        literal.LeadingTrivia[0].Equals(trivia).Should().BeFalse();
    }

    [Fact]
    public void EquivalentTriviaAttachedToDifferentTokensAreNotEqual()
    {
        var comment = Comment("# Commentary");

        var literal1 = Literal(TriviaList([comment]), "Literally", TriviaList());
        var literal2 = Literal(TriviaList([comment]), "Literally", TriviaList());

        var trivia1 = literal1.LeadingTrivia[0];
        var trivia2 = literal2.LeadingTrivia[0];

        trivia1.Equals(trivia2).Should().BeFalse();
        trivia2.Equals(trivia1).Should().BeFalse();
    }
}
