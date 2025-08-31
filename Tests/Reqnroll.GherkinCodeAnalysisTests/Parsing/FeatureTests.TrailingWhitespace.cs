namespace Reqnroll.CodeAnalysis.Gherkin.Parsing;

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
                    ColonWithSpace,
                    Name(
                        TriviaList(),
                        "Guess the word",
                        TriviaList([CarriageReturnLineFeed])),
                    default),
                Token(
                    TriviaList([Whitespace("    "), CarriageReturnLineFeed]),
                    SyntaxKind.EndOfFileToken,
                    TriviaList())));

        tree.GetRoot().ToFullString().Should().Be(source);
    }
}
