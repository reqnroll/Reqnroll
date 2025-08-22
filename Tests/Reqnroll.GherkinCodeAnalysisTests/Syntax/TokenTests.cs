using FluentAssertions;
using Microsoft.CodeAnalysis.Text;

namespace Reqnroll.CodeAnalysis.Gherkin.Syntax;

public class TokenTests
{
    [Fact]
    public void CanCreateColonToken()
    {
        var token = SyntaxFactory.Token(SyntaxKind.ColonToken);

        token.Text.Should().Be(":");
        token.Kind.Should().Be(SyntaxKind.ColonToken);
        token.IsMissing.Should().BeFalse();

        token.Parent.Should().BeNull();
        token.SyntaxTree.Should().BeNull();

        token.Span.Should().Be(new TextSpan(0, 1));
        token.FullSpan.Should().Be(token.Span);

        token.HasLeadingTrivia.Should().BeTrue();
        token.HasTrailingTrivia.Should().BeTrue();
        token.LeadingTrivia.Should().BeEquivalentTo([SyntaxFactory.ElasticMarker]);
        token.TrailingTrivia.Should().BeEquivalentTo([SyntaxFactory.ElasticMarker]);

        token.ContainsDiagnostics.Should().BeFalse();

        token.ToString().Should().Be(":");
        token.ToFullString().Should().Be(":");
    }

    [Fact]
    public void CanCreateFeatureKeywordToken()
    {
        var token = SyntaxFactory.Token(SyntaxKind.FeatureKeyword, "Feature");

        token.Text.Should().Be("Feature");
        token.Kind.Should().Be(SyntaxKind.FeatureKeyword);
        token.IsMissing.Should().BeFalse();

        token.Parent.Should().BeNull();
        token.SyntaxTree.Should().BeNull();

        token.Span.Should().Be(new TextSpan(0, 7));
        token.FullSpan.Should().Be(token.Span);

        token.HasLeadingTrivia.Should().BeTrue();
        token.HasTrailingTrivia.Should().BeTrue();
        token.LeadingTrivia.Should().BeEquivalentTo([SyntaxFactory.ElasticMarker]);
        token.TrailingTrivia.Should().BeEquivalentTo([SyntaxFactory.ElasticMarker]);

        token.ContainsDiagnostics.Should().BeFalse();

        token.ToString().Should().Be("Feature");
        token.ToFullString().Should().Be("Feature");
    }

    [Fact]
    public void CanCreateTokenWithTrivia()
    {
        var whitespace = SyntaxFactory.Whitespace("    ");

        var token = SyntaxFactory.Token(SyntaxKind.ColonToken)
            .WithLeadingTrivia(whitespace)
            .WithTrailingTrivia(whitespace);

        token.ToString().Should().Be(":");
        token.ToFullString().Should().Be("    :    ");

        token.Span.Should().Be(new TextSpan(4, 1));
        token.FullSpan.Should().Be(new TextSpan(0, 9));

        token.HasLeadingTrivia.Should().BeTrue();
        token.HasTrailingTrivia.Should().BeTrue();

        token.LeadingTrivia.Should().BeEquivalentTo(
            [whitespace],
            options => options
                .Excluding(trivia => trivia.Span)
                .Excluding(trivia => trivia.FullSpan));
        token.LeadingTrivia[0].Span.Should().Be(new TextSpan(0, 4));

        token.TrailingTrivia.Should().BeEquivalentTo(
            [whitespace],
            options => options
                .Excluding(trivia => trivia.Span)
                .Excluding(trivia => trivia.FullSpan));
        token.TrailingTrivia[0].Span.Should().Be(new TextSpan(5, 4));

        token.Parent.Should().BeNull();
        token.SyntaxTree.Should().BeNull();
    }

    [Fact]
    public void CanCreateMissingToken()
    {
        var token = SyntaxFactory.MissingToken(SyntaxKind.ColonToken);

        token.Text.Should().Be(":");
        token.Kind.Should().Be(SyntaxKind.ColonToken);
        token.IsMissing.Should().BeTrue();

        token.Parent.Should().BeNull();
        token.SyntaxTree.Should().BeNull();

        token.Span.Should().Be(new TextSpan(0, 1));
        token.FullSpan.Should().Be(token.Span);

        token.HasLeadingTrivia.Should().BeTrue();
        token.HasTrailingTrivia.Should().BeTrue();
        token.LeadingTrivia.Should().BeEquivalentTo([SyntaxFactory.ElasticMarker]);
        token.TrailingTrivia.Should().BeEquivalentTo([SyntaxFactory.ElasticMarker]);

        token.ContainsDiagnostics.Should().BeFalse();

        token.ToString().Should().Be(":");
        token.ToFullString().Should().Be(":");
    }
}
