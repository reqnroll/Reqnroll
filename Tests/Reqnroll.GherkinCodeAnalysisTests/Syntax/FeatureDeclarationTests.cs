namespace Reqnroll.CodeAnalysis.Gherkin.Syntax;

using static SyntaxFactory;

public class FeatureDeclarationTests
{
    [Fact]
    public void ToStringReturnsTokensInDocumentOrder()
    {
        var feature = FeatureDeclaration(
            Token(SyntaxKind.FeatureKeyword, "Feature"),
            Token(TriviaList(), SyntaxKind.ColonToken, TriviaList([Whitespace(" ")])),
            Identifier(TriviaList(), "Guess the word", TriviaList([CarriageReturnLineFeed, CarriageReturnLineFeed])),
            Description(
                Literal(TriviaList(), "An example feature from the Gherkin reference.", TriviaList([CarriageReturnLineFeed]))));

        feature.ToString().Should().Be("Feature: Guess the word\r\n\r\nAn example feature from the Gherkin reference.");

        feature.ToFullString().Should().Be("Feature: Guess the word\r\n\r\nAn example feature from the Gherkin reference.\r\n");
    }
}
