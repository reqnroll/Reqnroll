using Reqnroll.CodeAnalysis.Gherkin.Syntax;

namespace Reqnroll.CodeAnalysis.Gherkin.Parsing;

using static SyntaxFactory;

public partial class ScenarioOutlineTests
{
    [Fact]
    public void ScenarioOutlineWithTableWithEmptyCellsIsRepresentedInTree()
    {
        const string source =
            """
            Feature: Scenario Outline with empty cells

              Scenario Outline: eating
                Given there are <start> cucumbers
                When I eat <eat> cucumbers
                Then I should have <left> cucumbers

                Examples:
                  | start | eat | left |
                  |       |  5  |      |
                  |  20   |     |  15  |

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
                            "Scenario Outline with empty cells",
                            TriviaList([EnvironmentNewLine]))),
                    scenarios: List([
                        Scenario(
                            Token(
                                TriviaList([EnvironmentNewLine, Whitespace("  ")]),
                                SyntaxKind.ScenarioKeyword,
                                "Scenario Outline",
                                TriviaList()),
                            Token(
                                TriviaList(),
                                SyntaxKind.ColonToken,
                                TriviaList([Space])),
                            LiteralText(
                                Literal(
                                    TriviaList(),
                                    "eating",
                                    TriviaList([EnvironmentNewLine]))),
                            steps: List([
                                Step(
                                    Token(
                                        TriviaList([Whitespace("  ")]),
                                        SyntaxKind.GivenKeyword,
                                        "Given",
                                        TriviaList([Space])),
                                    LiteralText(
                                        TokenList([
                                            Token(
                                                TriviaList(),
                                                SyntaxKind.LiteralToken,
                                                "there are <start> cucumbers",
                                                TriviaList([EnvironmentNewLine]))
                                        ]))),
                                Step(
                                    Token(
                                        TriviaList([Whitespace("  ")]),
                                        SyntaxKind.WhenKeyword,
                                        "When",
                                        TriviaList([Space])),
                                    LiteralText(
                                        TokenList([
                                            Token(
                                                TriviaList(),
                                                SyntaxKind.LiteralToken,
                                                "I eat <eat> cucumbers",
                                                TriviaList([EnvironmentNewLine]))
                                        ]))),
                                Step(
                                    Token(
                                        TriviaList([Whitespace("  ")]),
                                        SyntaxKind.ThenKeyword,
                                        "Then",
                                        TriviaList([Space])),
                                    LiteralText(
                                        TokenList([
                                            Token(
                                                TriviaList(),
                                                SyntaxKind.LiteralToken,
                                                "I should have <left> cucumbers",
                                                TriviaList([EnvironmentNewLine, EnvironmentNewLine]))
                                        ])))
                            ]),
                            examples: List([
                                Examples(
                                    Token(
                                        TriviaList([Whitespace("  ")]),
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
                                                    TriviaList([Whitespace("    ")]),
                                                    SyntaxKind.VerticalBarToken,
                                                    TriviaList([Space])),
                                                SeparatedList<PlainTextSyntax>([
                                                    LiteralText(
                                                        TableLiteral(
                                                            TriviaList(),
                                                            "start",
                                                            TriviaList([Space]))),
                                                    Token(
                                                        TriviaList(),
                                                        SyntaxKind.VerticalBarToken,
                                                        TriviaList([Space])),
                                                    LiteralText(
                                                        TableLiteral(
                                                            TriviaList(),
                                                            "eat",
                                                            TriviaList([Space]))),
                                                    Token(
                                                        TriviaList(),
                                                        SyntaxKind.VerticalBarToken,
                                                        TriviaList([Space])),
                                                    LiteralText(
                                                        TableLiteral(
                                                            TriviaList(),
                                                            "left",
                                                            TriviaList([Space])))
                                                ]),
                                                Token(
                                                    TriviaList(),
                                                    SyntaxKind.VerticalBarToken,
                                                    TriviaList([EnvironmentNewLine]))),
                                            TableRow(
                                                Token(
                                                    TriviaList([Whitespace("    ")]),
                                                    SyntaxKind.VerticalBarToken,
                                                    TriviaList([Whitespace("   ")])),
                                                SeparatedList<PlainTextSyntax>([
                                                    LiteralText(
                                                        MissingToken(
                                                            TriviaList(),
                                                            SyntaxKind.LiteralToken,
                                                            TriviaList([Whitespace("    ")]))),
                                                    Token(
                                                        TriviaList(),
                                                        SyntaxKind.VerticalBarToken,
                                                        TriviaList()),
                                                    LiteralText(
                                                        TableLiteral(
                                                            TriviaList([Whitespace("  ")]),
                                                            "5",
                                                            TriviaList([Whitespace("  ")]))),
                                                    Token(
                                                        TriviaList(),
                                                        SyntaxKind.VerticalBarToken,
                                                        TriviaList()),
                                                    LiteralText(
                                                        MissingToken(
                                                            TriviaList([Whitespace("   ")]),
                                                            SyntaxKind.LiteralToken,
                                                            TriviaList([Whitespace("   ")])))
                                                ]),
                                                Token(
                                                    TriviaList(),
                                                    SyntaxKind.VerticalBarToken,
                                                    TriviaList([EnvironmentNewLine]))),
                                            TableRow(
                                                Token(
                                                    TriviaList([Whitespace("    ")]),
                                                    SyntaxKind.VerticalBarToken,
                                                    TriviaList()),
                                                SeparatedList<PlainTextSyntax>([
                                                    LiteralText(
                                                        TableLiteral(
                                                            TriviaList([Whitespace("  ")]),
                                                            "20",
                                                            TriviaList([Whitespace("   ")]))),
                                                    Token(
                                                        TriviaList(),
                                                        SyntaxKind.VerticalBarToken,
                                                        TriviaList()),
                                                    LiteralText(
                                                        MissingToken(
                                                            TriviaList([Whitespace("  ")]),
                                                            SyntaxKind.TableLiteralToken,
                                                            TriviaList([Whitespace("   ")]))),
                                                    Token(
                                                        TriviaList(),
                                                        SyntaxKind.VerticalBarToken,
                                                        TriviaList()),
                                                    LiteralText(
                                                        TableLiteral(
                                                            TriviaList([Whitespace("  ")]),
                                                            "15",
                                                            TriviaList([Whitespace("  ")])))
                                                ]),
                                                Token(
                                                    TriviaList(),
                                                    SyntaxKind.VerticalBarToken,
                                                    TriviaList([EnvironmentNewLine])))
                                        ])
                                    )
                                )
                            ])
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
