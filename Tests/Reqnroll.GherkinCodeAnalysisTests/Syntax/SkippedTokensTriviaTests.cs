using FluentAssertions;
using FluentAssertions.Equivalency;
using Microsoft.CodeAnalysis.Text;

namespace Reqnroll.CodeAnalysis.Gherkin.Syntax;

using static SyntaxFactory;

public class SkippedTokensTriviaTests
{
    [Fact]
    public void CanCreateSkippedTokensTriviaFromLiteralText()
    {
        var token = Literal(TriviaList(), "invalid", TriviaList([Space]));

        var skippedTokensTrivia = SkippedTokensTrivia(TokenList([token]));

        var length = "invalid".Length;
        var fullLength = "invalid ".Length;

        skippedTokensTrivia.ToString().Should().Be("invalid");
        skippedTokensTrivia.ToFullString().Should().Be("invalid ");

        skippedTokensTrivia.Span.Should().Be(TextSpan.FromBounds(0, length));
        skippedTokensTrivia.FullSpan.Should().Be(TextSpan.FromBounds(0, fullLength));

        skippedTokensTrivia.GetFirstToken().Should().BeEquivalentTo(token);
        skippedTokensTrivia.GetLastToken().Should().BeEquivalentTo(token);

        skippedTokensTrivia.HasLeadingTrivia.Should().BeFalse();
        skippedTokensTrivia.HasTrailingTrivia.Should().BeTrue();
        skippedTokensTrivia.GetLeadingTrivia().Should().BeEmpty();
        skippedTokensTrivia.GetTrailingTrivia().Should().BeEquivalentTo([Space]);

        skippedTokensTrivia.HasDiagnostics.Should().BeFalse();
        skippedTokensTrivia.GetDiagnostics().Should().BeEmpty();

        skippedTokensTrivia.ChildNodesAndTokens().Should().BeEquivalentTo([ (SyntaxNodeOrToken<SyntaxNode>)token ]);

        skippedTokensTrivia.Kind.Should().Be(SyntaxKind.SkippedTokensTrivia);
        skippedTokensTrivia.Tokens.Should().BeEquivalentTo([Literal(TriviaList(), "invalid", TriviaList([Space]))]);
    }

    [Fact]
    public void CanAssociatedSkippedTokensWithUnskippedTokens()
    {
        var token = Token(
            TriviaList([
                Trivia(
                    SkippedTokensTrivia(
                        TokenList([Literal(TriviaList(), "invalid", TriviaList([ Space ])) ])))
                ]),
            SyntaxKind.ColonToken,
            TriviaList());

        token.ToString().Should().Be(":");
        token.ToFullString().Should().Be("invalid :");

        token.Span.Should().Be(new TextSpan(8, 1));
        token.FullSpan.Should().Be(new TextSpan(0, 9));

        token.HasLeadingTrivia.Should().BeTrue();
        token.HasTrailingTrivia.Should().BeFalse();
        token.ContainsDiagnostics.Should().BeFalse();
        token.TrailingTrivia.Should().BeEmpty();
        token.GetDiagnostics().Should().BeEmpty();

        token.Kind.Should().Be(SyntaxKind.ColonToken);

        var skippedTrivia = token.LeadingTrivia[0];

        skippedTrivia.Token.Should().Be(token);
        skippedTrivia.Span.Should().Be(TextSpan.FromBounds(0, "invalid".Length));
        skippedTrivia.FullSpan.Should().Be(TextSpan.FromBounds(0, "invalid ".Length));

        skippedTrivia.Kind.Should().Be(SyntaxKind.SkippedTokensTrivia);
        var skippedTokens = skippedTrivia.Structure!;

        skippedTokens.Should().NotBeNull();
        skippedTokens.Kind.Should().Be(SyntaxKind.SkippedTokensTrivia);
        skippedTrivia.Span.Should().Be(TextSpan.FromBounds(0, "invalid".Length));
        skippedTrivia.FullSpan.Should().Be(TextSpan.FromBounds(0, "invalid ".Length));
    }
}
