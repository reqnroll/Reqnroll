using Reqnroll.CodeAnalysis.Gherkin.Syntax;

namespace Reqnroll.CodeAnalysis.Gherkin.Parsing;

using static SyntaxFactory;

public partial class FeatureTests
{
    [Fact]
    public void LeadingWhitespaceIsRepresentedInTree()
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
                        TriviaList([Whitespace("    "), CarriageReturnLineFeed, Whitespace("    ")]),
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
                    TriviaList(),
                    SyntaxKind.EndOfFileToken,
                    TriviaList())));

        tree.GetRoot().ToFullString().Should().Be(source);
    }
}
