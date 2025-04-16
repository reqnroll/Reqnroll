using FluentAssertions;
using Microsoft.CodeAnalysis;
using Reqnroll.CodeAnalysis.Gherkin.Syntax;

namespace Reqnroll.CodeAnalysis.Gherkin;

using static SyntaxFactory;

public partial class ParsingTests
{
    [Fact]
    public void MinimalFeatureDeclarationIsRepresentedInTree()
    {
        const string source =
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
                    TriviaList(),
                    SyntaxKind.EndOfFileToken,
                    TriviaList())));

        tree.ToString().Should().Be(source);
    }

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
                    Identifier(
                        TriviaList(),
                        "Guess the word",
                        TriviaList([CarriageReturnLineFeed])),
                    Description(
                        TokenList([
                            Literal(
                                TriviaList([ CarriageReturnLineFeed, Whitespace("      ") ]),
                                "The word guess game is a turn-based game for two players.",
                                TriviaList([ CarriageReturnLineFeed ])),
                            Literal(
                                TriviaList([ Whitespace("      ") ]),
                                "The Maker makes a word for the Breaker to guess. The game",
                                TriviaList([ CarriageReturnLineFeed ])),
                            Literal(
                                TriviaList([ Whitespace("      ") ]),
                                "is over when the Breaker guesses the Maker's word.",
                                TriviaList([ CarriageReturnLineFeed ]))
                        ]))),
                Token(
                    TriviaList(),
                    SyntaxKind.EndOfFileToken,
                    TriviaList())));

        tree.ToString().Should().Be(source);
    }

    [Fact]
	public void DoubleFeatureDeclarationIsRepresentedInTreeAsFeatureDescription()
	{
        const string source =
            """
                Feature: Guess the word

                Feature: Guess the word again 
            
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
                        TriviaList([CarriageReturnLineFeed])),
                    Description(
                        TokenList([
                            Literal(
                                TriviaList([ CarriageReturnLineFeed, Whitespace("    ") ]),
                                "Feature: Guess the word again",
                                TriviaList([ Space, CarriageReturnLineFeed ]))
                        ]))),
                Token(
                    TriviaList(),
                    SyntaxKind.EndOfFileToken,
                    TriviaList())));

        tree.ToString().Should().Be(source);
	}

    [Fact]
    public void InvalidFeatureSourceTextCreatesTreeWithSkippedTokensAndDiagnostic()
    {
        const string source = "invalid ";

        var tree = GherkinSyntaxTree.ParseText(source);

        tree.GetRoot().Should().BeEquivalentTo(
            GherkinDocument(
                null,
                Token(
                    TriviaList([
                        Trivia(
                            SkippedTokensTrivia(
                                TokenList([ Literal(TriviaList(), "invalid", TriviaList([ Space ])) ])))
                        ]),
                    SyntaxKind.EndOfFileToken,
                    TriviaList())));

        tree.ToString().Should().Be(source);
    }
}
