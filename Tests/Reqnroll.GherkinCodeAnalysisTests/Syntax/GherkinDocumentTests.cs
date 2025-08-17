using Microsoft.CodeAnalysis.Text;

namespace Reqnroll.CodeAnalysis.Gherkin.Syntax;

public class GherkinDocumentTests
{
    [Fact]
    public void CanCreateEmptyFeatureFile()
    {
        var featureFile = SyntaxFactory.GherkinDocument();

        featureFile.ToFullString().Should().Be("");

        featureFile.Span.Should().Be(TextSpan.FromBounds(0, 0));
        featureFile.FullSpan.Should().Be(TextSpan.FromBounds(0, 0));
        featureFile.GetFirstToken().Should().Be(featureFile.EndOfFileToken);
        featureFile.GetLastToken().Should().Be(featureFile.EndOfFileToken);
        featureFile.HasLeadingTrivia.Should().BeTrue(because: "tokens are created with elastic trivia by default.");
        featureFile.HasTrailingTrivia.Should().BeTrue(because: "tokens are created with elastic trivia by default.");
        featureFile.HasDiagnostics.Should().BeFalse();
        featureFile.GetLeadingTrivia().Should().BeEquivalentTo([SyntaxFactory.ElasticMarker]);
        featureFile.GetTrailingTrivia().Should().BeEquivalentTo([SyntaxFactory.ElasticMarker]);
        featureFile.GetDiagnostics().Should().BeEmpty();
        featureFile.ChildNodesAndTokens().Should().BeEquivalentTo<SyntaxNodeOrToken<SyntaxNode>>([featureFile.EndOfFileToken]);

        featureFile.FeatureDeclaration.Should().BeNull();

        featureFile.EndOfFileToken.Span.Should().Be(TextSpan.FromBounds(0, 0));
        featureFile.EndOfFileToken.FullSpan.Should().Be(TextSpan.FromBounds(0, 0));
    }

    [Fact]
    public void CanCreateFeatureFileWithFeatureDeclaration()
    {
        var featureFile = SyntaxFactory.GherkinDocument(
            SyntaxFactory.Feature("Feature", "Guess the word"));

        featureFile.ToFullString().Should().Be("Feature:Guess the word");

        var fullLength = "Feature:Guess the word".Length;

        featureFile.Span.Should().Be(TextSpan.FromBounds(0, fullLength));
        featureFile.FullSpan.Should().Be(TextSpan.FromBounds(0, fullLength));
        featureFile.GetFirstToken().Should().Be(featureFile.FeatureDeclaration!.FeatureKeyword);
        featureFile.GetLastToken().Should().Be(featureFile.EndOfFileToken);
        featureFile.HasLeadingTrivia.Should().BeTrue(because: "tokens are created with elastic trivia by default.");
        featureFile.HasTrailingTrivia.Should().BeTrue(because: "tokens are created with elastic trivia by default.");
        featureFile.HasDiagnostics.Should().BeFalse();
        featureFile.GetLeadingTrivia().Should().BeEquivalentTo([SyntaxFactory.ElasticMarker]);
        featureFile.GetTrailingTrivia().Should().BeEquivalentTo([SyntaxFactory.ElasticMarker]);
        featureFile.GetDiagnostics().Should().BeEmpty();

        featureFile.ChildNodesAndTokens().Should().BeEquivalentTo<SyntaxNodeOrToken<SyntaxNode>>(
            [featureFile.FeatureDeclaration, featureFile.EndOfFileToken]);

        featureFile.FeatureDeclaration.Should().NotBeNull();
        featureFile.FeatureDeclaration!.Span.Should().Be(TextSpan.FromBounds(0, fullLength));
        featureFile.FeatureDeclaration!.FullSpan.Should().Be(TextSpan.FromBounds(0, fullLength));

        featureFile.EndOfFileToken.Span.Should().Be(TextSpan.FromBounds(fullLength, fullLength));
        featureFile.EndOfFileToken.FullSpan.Should().Be(TextSpan.FromBounds(fullLength, fullLength));
    }
}
