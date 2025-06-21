using Reqnroll.CodeAnalysis.Gherkin.Syntax;

namespace Reqnroll.CodeAnalysis.Gherkin.Parsing;

using static SyntaxFactory;

public partial class ScenarioOutlineTests
{
    [Fact]
    public void ScenarioOutlineWithSplitExamplesTableIsRepresentedInTree()
    {
        const string source =
            """
            Feature: Scenario Outline

              Scenario Outline: eating
                Given there are <start> cucumbers
                When I eat <eat> cucumbers
                Then I should have <left> cucumbers

                Examples: first
                  | start | eat | left |
                  |  12   |  5  |  7   |

                Examples: second
                  | start | eat | left |
                  |  20   |  5  |  15  |

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
                            "Scenario Outline",
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
                                        TriviaList([Space])),
                                    LiteralText(
                                        Literal(
                                            TriviaList(),
                                            "first",
                                            TriviaList([EnvironmentNewLine]))),
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
                                                    TriviaList(Whitespace("  "))),
                                                SeparatedList<PlainTextSyntax>([
                                                    LiteralText(
                                                        TableLiteral(
                                                            TriviaList(),
                                                            "12",
                                                            TriviaList([Whitespace("   ")]))),
                                                    Token(
                                                        TriviaList(),
                                                        SyntaxKind.VerticalBarToken,
                                                        TriviaList([Whitespace("  ")])),
                                                    LiteralText(
                                                        TableLiteral(
                                                            TriviaList(),
                                                            "5",
                                                            TriviaList([Whitespace("  ")]))),
                                                    Token(
                                                        TriviaList(),
                                                        SyntaxKind.VerticalBarToken,
                                                        TriviaList([Whitespace("  ")])),
                                                    LiteralText(
                                                        TableLiteral(
                                                            TriviaList(),
                                                            "7",
                                                            TriviaList([Whitespace("   ")])))
                                                ]),
                                                Token(
                                                    TriviaList(),
                                                    SyntaxKind.VerticalBarToken,
                                                    TriviaList([EnvironmentNewLine])))
                                        ])
                                    )
                                ),
                                    Examples(
                                        Token(
                                            TriviaList([EnvironmentNewLine, Whitespace("  ")]),
                                            SyntaxKind.ExamplesKeyword,
                                            "Examples",
                                            TriviaList()),
                                        Token(
                                            TriviaList(),
                                            SyntaxKind.ColonToken,
                                            TriviaList([Space])),
                                        LiteralText(
                                            TableLiteral(
                                                TriviaList(),
                                                "second",
                                                TriviaList([EnvironmentNewLine]))),
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
                                                        TriviaList(Whitespace("  "))),
                                                    SeparatedList<PlainTextSyntax>([
                                                        LiteralText(
                                                            TableLiteral(
                                                                TriviaList(),
                                                                "20",
                                                                TriviaList([Whitespace("   ")]))),
                                                        Token(
                                                            TriviaList(),
                                                            SyntaxKind.VerticalBarToken,
                                                            TriviaList([Whitespace("  ")])),
                                                        LiteralText(
                                                            TableLiteral(
                                                                TriviaList(),
                                                                "5",
                                                                TriviaList([Whitespace("  ")]))),
                                                        Token(
                                                            TriviaList(),
                                                            SyntaxKind.VerticalBarToken,
                                                            TriviaList([Whitespace("  ")])),
                                                        LiteralText(
                                                            TableLiteral(
                                                                TriviaList(),
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
