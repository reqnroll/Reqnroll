using Reqnroll.CodeAnalysis.Gherkin.Syntax;

namespace Reqnroll.CodeAnalysis.Gherkin.Parsing;

using static SyntaxFactory;

public partial class FeatureTests
{
    [Fact]
    public void FeatureDeclarationWithDescriptionIsRepresentedInTree()
    {
        const string source =
            """
                Feature: Guess the word

                  The word guess game is a turn-based game for two players.
                  The Maker makes a word for the Breaker to guess. The game
                  is over when the Breaker guesses the Maker's word.

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
                    LiteralText(
                        Literal(
                            TriviaList(),
                            "Guess the word",
                            TriviaList([EnvironmentNewLine]))),
                    LiteralText(
                        TokenList([
                            Literal(
                                TriviaList([EnvironmentNewLine, Whitespace("      ")]),
                                "The word guess game is a turn-based game for two players.",
                                TriviaList([EnvironmentNewLine])),
                            Literal(
                                TriviaList([Whitespace("      ")]),
                                "The Maker makes a word for the Breaker to guess. The game",
                                TriviaList([EnvironmentNewLine])),
                            Literal(
                                TriviaList([Whitespace("      ")]),
                                "is over when the Breaker guesses the Maker's word.",
                                TriviaList([EnvironmentNewLine]))
                        ]))),
                Token(
                    TriviaList(),
                    SyntaxKind.EndOfFileToken,
                    TriviaList())));

        tree.ToString().Should().Be(source);
    }
}
