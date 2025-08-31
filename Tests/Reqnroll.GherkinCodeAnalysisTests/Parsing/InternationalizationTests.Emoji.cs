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
                                    Token(
                                        TriviaList(),
                                        SyntaxKind.DirectiveIdentifierToken,
                                        "language",
                                        TriviaList()),
                                    Token(
                                        TriviaList(),
                                        SyntaxKind.ColonToken,
                                        TriviaList()),
                                    Token(
                                        TriviaList(),
                                        SyntaxKind.DirectiveValueToken,
                                        "emoji",
                                        TriviaList([EnvironmentNewLine]))))]),
                        SyntaxKind.FeatureKeyword,
                        "🏷️",
                        TriviaList()),
                    ColonWithSpace,
                    Name(
                        TriviaList(),
                        "i18n support",
                        TriviaList([EnvironmentNewLine])),
                    examples: List([
                        Example(
                            Token(
                                TriviaList([EnvironmentNewLine, Whitespace("  ")]),
                                SyntaxKind.ExampleKeyword,
                                "🎬",
                                TriviaList()),
                            ColonWithSpace,
                            Name(
                                TriviaList(),
                                "Support for emoji keywords",
                                TriviaList([EnvironmentNewLine, Whitespace("  "), EnvironmentNewLine])),
                            steps: default)
                    ])),
                Token(
                    TriviaList(),
                    SyntaxKind.EndOfFileToken,
                    TriviaList()))
        );

        tree.ToString().Should().Be(source);
    }
}
