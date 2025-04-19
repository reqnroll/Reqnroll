namespace Reqnroll.CodeAnalysis.Gherkin.ParsingTests;

using Reqnroll.CodeAnalysis.Gherkin.Syntax;

using static SyntaxFactory;

public partial class FeatureTests
{
    [Fact]
    public void TrailingWhitespaceisRepresentedInTree()
    {
        var source =
            """
                Feature: Guess the word
                
            
            """;

        var tree = GherkinSyntaxTree.ParseText(source);

        tree.GetRoot().Should().BeEquivalentTo(
            GherkinDocument(
                Feature(
                    Token(
                        TriviaList([Whitespace("    ")]),
                        SyntaxKind.FeatureKeyword,
                        "Feature",
                        TriviaList()),
                    Token(
                        TriviaList(),
                        SyntaxKind.ColonToken,
                        TriviaList([Space])),
                    Identifier(
                        TriviaList(),
                        "Guess the word",
                        TriviaList([CarriageReturnLineFeed]))),
                Token(
                    TriviaList([Whitespace("    "), CarriageReturnLineFeed]),
                    SyntaxKind.EndOfFileToken,
                    TriviaList())));

        tree.GetRoot().ToFullString().Should().Be(source);
    }
}
