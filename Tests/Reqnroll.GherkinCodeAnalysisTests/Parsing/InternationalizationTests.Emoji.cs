using Reqnroll.CodeAnalysis.Gherkin.Syntax;

namespace Reqnroll.CodeAnalysis.Gherkin.Parsing;

using static SyntaxFactory;

public partial class InternationalizationTests
{
    [Fact]
    public void EmojiIsRepresentedInTree()
    {
        const string source =
            """
            #language:emoji
            🏷️: i18n support

              🎬: Support for emoji keywords
               
            """;

        var tree = GherkinSyntaxTree.ParseText(source);

        tree.GetRoot().Should().BeEquivalentTo(
            GherkinDocument(
                Feature(
                    Token(
                        TriviaList([
                            Trivia(
                                DirectiveCommentTrivia(
                                    Token(
                                        TriviaList(),
                                        SyntaxKind.HashToken,
                                        TriviaList()),
                                    DirectiveIdentifier(
                                        TriviaList(),
                                        "language",
                                        TriviaList()),
                                    Token(
                                        TriviaList(),
                                        SyntaxKind.ColonToken,
                                        TriviaList()),
                                    Literal(
                                        TriviaList(),
                                        "emoji",
                                        TriviaList([EnvironmentNewLine]))))]),
                        SyntaxKind.FeatureKeyword,
                        "🏷️",
                        TriviaList()),
                    Token(
                        TriviaList(),
                        SyntaxKind.ColonToken,
                        TriviaList([Space])),
                    LiteralText(
                        Literal(
                            TriviaList(),
                            "i18n support",
                            TriviaList([EnvironmentNewLine]))),
                    examples: List([
                        Example(
                            Token(
                                TriviaList([EnvironmentNewLine, Whitespace("  ")]),
                                SyntaxKind.ExampleKeyword,
                                "🎬",
                                TriviaList()),
                            Token(
                                TriviaList(),
                                SyntaxKind.ColonToken,
                                TriviaList([Space])),
                            LiteralText(
                                Literal(
                                    TriviaList(),
                                    "Support for emoji keywords",
                                    TriviaList([EnvironmentNewLine, Whitespace("  "), EnvironmentNewLine]))
                            ),
                            steps: List<StepSyntax>()
                        )
                    ])
                ),
                Token(
                    TriviaList(),
                    SyntaxKind.EndOfFileToken,
                    TriviaList()))
        );

        tree.ToString().Should().Be(source);
    }
}
