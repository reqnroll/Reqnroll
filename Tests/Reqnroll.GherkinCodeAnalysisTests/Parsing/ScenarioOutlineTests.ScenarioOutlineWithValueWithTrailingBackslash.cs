using Reqnroll.CodeAnalysis.Gherkin.Syntax;

namespace Reqnroll.CodeAnalysis.Gherkin.Parsing;

using static SyntaxFactory;

public partial class ScenarioOutlineTests
{
    [Fact]
    public void ScenarioOutlineWithValueWithTrailingBackslashIsRepresentedInTree()
    {
        // Taken from good/scenario_outline_with_value_with_trailing_backslash.feature
        const string source =
            """
            Feature: Scenario Outline with values with trailing backslash

            Scenario Outline: minimalistic
                Given <what>
                When <this>
                Then <that>

            Examples:
              | what | this  | that   |
              | x\\y | this\ | that\\ |

            """;

        var tree = GherkinSyntaxTree.ParseText(source);

        tree.GetRoot().Should().BeEquivalentTo(
            GherkinDocument(
                Feature(
                    Token(
                        TriviaList(),
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
                            "Example Outline with values with trailing backslash",
                            TriviaList([EnvironmentNewLine]))),
                    members: List([
                        Example(
                            Token(
                                TriviaList([EnvironmentNewLine]),
                                SyntaxKind.ExampleKeyword,
                                "Example Outline",
                                TriviaList()),
                            Token(
                                TriviaList(),
                                SyntaxKind.ColonToken,
                                TriviaList([Space])),
                            LiteralText(
                                Literal(
                                    TriviaList(),
                                    "minimalistic",
                                    TriviaList([EnvironmentNewLine]))),
                            steps: List([
                                Step(
                                    Token(
                                        TriviaList([Whitespace("    ")]),
                                        SyntaxKind.GivenKeyword,
                                        "Given",
                                        TriviaList([Space])),
                                    LiteralText(
                                        TokenList([
                                            Literal(
                                                TriviaList(),
                                                "<what>",
                                                TriviaList([EnvironmentNewLine]))
                                        ]))),
                                Step(
                                    Token(
                                        TriviaList([Whitespace("    ")]),
                                        SyntaxKind.WhenKeyword,
                                        "When",
                                        TriviaList([Space])),
                                    LiteralText(
                                        TokenList([
                                            Literal(
                                                TriviaList(),
                                                "<this>",
                                                TriviaList([EnvironmentNewLine]))
                                        ]))),
                                Step(
                                    Token(
                                        TriviaList([Whitespace("    ")]),
                                        SyntaxKind.ThenKeyword,
                                        "Then",
                                        TriviaList([Space])),
                                    LiteralText(
                                        TokenList([
                                            Literal(
                                                TriviaList(),
                                                "<that>",
                                                TriviaList([EnvironmentNewLine]))
                                        ])))
                            ]),
                            examples: List([
                                Examples(
                                    Token(
                                        TriviaList([EnvironmentNewLine]),
                                        SyntaxKind.ExamplesKeyword,
                                        "Examples",
                                        TriviaList()),
                                    Token(
                                        TriviaList(),
                                        SyntaxKind.ColonToken,
                                        TriviaList([EnvironmentNewLine])),
                                    table: Table(
                                        List([
                                            TableRow(
                                                Token(
                                                    TriviaList([Whitespace("  ")]),
                                                    SyntaxKind.VerticalBarToken,
                                                    TriviaList()),
                                                SeparatedList<PlainTextSyntax>([
                                                    LiteralText(
                                                        TableLiteral(
                                                            TriviaList([Space]),
                                                            "what",
                                                            TriviaList([Space]))),
                                                    Token(
                                                        TriviaList(),
                                                        SyntaxKind.VerticalBarToken,
                                                        TriviaList()),
                                                    LiteralText(
                                                        TableLiteral(
                                                            TriviaList([Space]),
                                                            "this",
                                                            TriviaList([Whitespace("  ")]))),
                                                    Token(
                                                        TriviaList(),
                                                        SyntaxKind.VerticalBarToken,
                                                        TriviaList()),
                                                    LiteralText(
                                                        TableLiteral(
                                                            TriviaList([Space]),
                                                            "that",
                                                            TriviaList([Whitespace("   ")])))
                                                ]),
                                                Token(
                                                    TriviaList(),
                                                    SyntaxKind.VerticalBarToken,
                                                    TriviaList([EnvironmentNewLine]))),
                                            TableRow(
                                                Token(
                                                    TriviaList([Whitespace("  ")]),
                                                    SyntaxKind.VerticalBarToken,
                                                    TriviaList([Space])),
                                                SeparatedList<PlainTextSyntax>([
                                                    LiteralText(
                                                        TableLiteral(
                                                            TriviaList(),
                                                            "x\\\\y",
                                                            "x\\y",
                                                            TriviaList([Space]))),
                                                    Token(
                                                        TriviaList(),
                                                        SyntaxKind.VerticalBarToken,
                                                        TriviaList([Space])),
                                                    LiteralText(
                                                        TableLiteral(
                                                            TriviaList(),
                                                            "this\\",
                                                            "this\\",
                                                            TriviaList([Whitespace("  ")]))),
                                                    Token(
                                                        TriviaList(),
                                                        SyntaxKind.VerticalBarToken,
                                                        TriviaList([Space])),
                                                    LiteralText(
                                                        TableLiteral(
                                                            TriviaList(),
                                                            "that\\\\",
                                                            "that\\",
                                                            TriviaList([Whitespace("   ")])))
                                                ]),
                                                Token(
                                                    TriviaList(),
                                                    SyntaxKind.VerticalBarToken,
                                                    TriviaList([EnvironmentNewLine])))
                                        ])))
                            ]))
                    ])),
                Token(
                    TriviaList(),
                    SyntaxKind.EndOfFileToken,
                    TriviaList())));

        tree.ToString().Should().Be(source);
    }
}
