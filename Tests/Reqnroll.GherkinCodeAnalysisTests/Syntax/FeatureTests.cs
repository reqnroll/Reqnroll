namespace Reqnroll.CodeAnalysis.Gherkin.Syntax;

using static SyntaxFactory;

public class FeatureTests
{
    [Fact]
    public void ToStringReturnsTokensInDocumentOrder()
    {
        var feature = Feature(
            default,
            Token(SyntaxKind.FeatureKeyword, "Feature"),
            Token(TriviaList(), SyntaxKind.ColonToken, TriviaList([Whitespace(" ")])),
            Name(TriviaList(), "Guess the word", TriviaList([CarriageReturnLineFeed, CarriageReturnLineFeed])),
            Description(
                TokenList([
                    DescriptionText(
                        TriviaList(),
                        "An example feature from the Gherkin reference.",
                        TriviaList([CarriageReturnLineFeed]))
                ])));

        feature.ToString().Should().Be(
            "Feature: Guess the word" + 
            "\r\n" +
            "\r\n" + 
            "An example feature from the Gherkin reference.");

        feature.ToFullString().Should().Be(
            "Feature: Guess the word" +
            "\r\n" +
            "\r\n" +
            "An example feature from the Gherkin reference." +
            "\r\n");
    }
}
