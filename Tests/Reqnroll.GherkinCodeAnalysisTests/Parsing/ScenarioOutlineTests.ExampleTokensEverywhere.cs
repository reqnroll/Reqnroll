using Reqnroll.CodeAnalysis.Gherkin.Syntax;

namespace Reqnroll.CodeAnalysis.Gherkin.Parsing;

using static SyntaxFactory;

public partial class ScenarioOutlineTests
{
    [Fact]
    public void ExampleTokensEverywhereIsRepresentedInTree()
    {
        // Taken from good/example_tokens_everywhere.feature
        const string source =
            """"
            Feature: Example tokens everywhere

              Scenario Outline: the <one>
                Given the <two>:
                  """
                  <three>
                  """
                Given the <four>:
                  | <five> |

                Examples:
                  | one | two  | three | four   | five  |
                  | un  | deux | trois | quatre | cinq  |
                  | uno | dos  | tres  | quatro | cinco |
            """";

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
                            "Example tokens everywhere",
                            TriviaList([EnvironmentNewLine]))),
                    examples: List([
                        Example(
                            Token(
                                TriviaList([EnvironmentNewLine, Whitespace("  ")]),
                                SyntaxKind.ExampleKeyword,
                                "Example Outline",
                                TriviaList()),
                            Token(
                                TriviaList(),
                                SyntaxKind.ColonToken,
                                TriviaList([Space])),
                            LiteralText(
                                TokenList([
                                    Literal(
                                        TriviaList(),
                                        "the <one>",
                                        TriviaList([EnvironmentNewLine]))
                                ])),
                            steps: List([
                                Step(
                                    Token(
                                        TriviaList([Whitespace("  ")]),
                                        SyntaxKind.ContextStepKeyword,
                                        "Given",
                                        TriviaList([Space])),
                                    LiteralText(
                                        TokenList([
                                            Literal(
                                                TriviaList(),
                                                "the <two>:",
                                                TriviaList([EnvironmentNewLine])),
                                    ])),
                                    StepDocString(
                                        DocString(
                                            Token(
                                                TriviaList([Whitespace("    ")]),
                                                SyntaxKind.DocStringDelimiterToken,
                                                "\"\"\"",
                                                TriviaList([EnvironmentNewLine])),
                                            LiteralText(
                                                TokenList([
                                                    Literal(
                                                        TriviaList([Whitespace("    ")]),
                                                        "<three>",
                                                        TriviaList([EnvironmentNewLine]))
                                                ])),
                                            Token(
                                                TriviaList([Whitespace("    ")]),
                                                SyntaxKind.DocStringDelimiterToken,
                                                "\"\"\"",
                                                TriviaList([EnvironmentNewLine]))))),
                                Step(
                                    Token(
                                        TriviaList([Whitespace("  ")]),
                                        SyntaxKind.ContextStepKeyword,
                                        "Given",
                                        TriviaList([Space])),
                                    LiteralText(
                                        TokenList([
                                            Literal(
                                                TriviaList(),
                                                "the <four>:",
                                                TriviaList([EnvironmentNewLine]))
                                        ])),
                                    StepTable(
                                        Table(
                                            List([
                                                TableRow(
                                                    Token(
                                                        TriviaList([Whitespace("    ")]),
                                                        SyntaxKind.VerticalBarToken,
                                                        TriviaList([Space])),
                                                    TableCellList([
                                                        TextTableCell(
                                                            TableLiteral(
                                                                TriviaList(),
                                                                "<five>",
                                                                TriviaList()))
                                                    ]),
                                                    Token(
                                                        TriviaList([Whitespace("  ")]),
                                                        SyntaxKind.VerticalBarToken,
                                                        TriviaList([EnvironmentNewLine])))
                                            ]))))
                            ]),
                            examples: List([
                                Examples(
                                    Token(
                                        TriviaList([EnvironmentNewLine, Whitespace("  ")]),
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
                                                TableCellList([
                                                    TextTableCell(
                                                        TableLiteral(
                                                            TriviaList(),
                                                            "one",
                                                            TriviaList())),
                                                    Token(
                                                        TriviaList([Space]),
                                                        SyntaxKind.VerticalBarToken,
                                                        TriviaList([Space])),
                                                    TextTableCell(
                                                        TableLiteral(
                                                            TriviaList(),
                                                            "two",
                                                            TriviaList())),
                                                    Token(
                                                        TriviaList([Whitespace("  ")]),
                                                        SyntaxKind.VerticalBarToken,
                                                        TriviaList([Space])),
                                                    TextTableCell(
                                                        TableLiteral(
                                                            TriviaList(),
                                                            "three",
                                                            TriviaList())),
                                                    Token(
                                                        TriviaList([Space]),
                                                        SyntaxKind.VerticalBarToken,
                                                        TriviaList([Space])),
                                                    TextTableCell(
                                                        TableLiteral(
                                                            TriviaList(),
                                                            "four",
                                                            TriviaList())),
                                                    Token(
                                                        TriviaList([Whitespace("  ")]),
                                                        SyntaxKind.VerticalBarToken,
                                                        TriviaList([Space])),
                                                    TextTableCell(
                                                        TableLiteral(
                                                            TriviaList(),
                                                            "five",
                                                            TriviaList()))
                                            ]),
                                            Token(
                                                TriviaList([Whitespace("  ")]),
                                                SyntaxKind.VerticalBarToken,
                                                TriviaList([EnvironmentNewLine]))),
                                            TableRow(
                                                Token(
                                                    TriviaList([Whitespace("    ")]),
                                                    SyntaxKind.VerticalBarToken,
                                                    TriviaList([Space])),
                                                TableCellList([
                                                    TextTableCell(
                                                        TableLiteral(
                                                            TriviaList(),
                                                            "un",
                                                            TriviaList())),
                                                    Token(
                                                        TriviaList([Space]),
                                                        SyntaxKind.VerticalBarToken,
                                                        TriviaList([Space])),
                                                    TextTableCell(
                                                        TableLiteral(
                                                            TriviaList(),
                                                            "deux",
                                                            TriviaList())),
                                                    Token(
                                                        TriviaList([Whitespace("  ")]),
                                                        SyntaxKind.VerticalBarToken,
                                                        TriviaList([Space])),
                                                    TextTableCell(
                                                        TableLiteral(
                                                            TriviaList(),
                                                            "trois",
                                                            TriviaList())),
                                                    Token(
                                                        TriviaList([Space]),
                                                        SyntaxKind.VerticalBarToken,
                                                        TriviaList([Space])),
                                                    TextTableCell(
                                                        TableLiteral(
                                                            TriviaList(),
                                                            "quatre",
                                                            TriviaList())),
                                                    Token(
                                                        TriviaList([Whitespace("  ")]),
                                                        SyntaxKind.VerticalBarToken,
                                                        TriviaList([Space])),
                                                    TextTableCell(
                                                        TableLiteral(
                                                            TriviaList(),
                                                            "cinq",
                                                            TriviaList()))
                                                ]),
                                                Token(
                                                    TriviaList([Whitespace("  ")]),
                                                    SyntaxKind.VerticalBarToken,
                                                    TriviaList([EnvironmentNewLine]))),
                                            TableRow(
                                                Token(
                                                    TriviaList([Whitespace("    ")]),
                                                    SyntaxKind.VerticalBarToken,
                                                    TriviaList([Space])),
                                                TableCellList([
                                                    TextTableCell(
                                                        TableLiteral(
                                                            TriviaList(),
                                                            "uno",
                                                            TriviaList())),
                                                    Token(
                                                        TriviaList([Space]),
                                                        SyntaxKind.VerticalBarToken,
                                                        TriviaList([Space])),
                                                    TextTableCell(
                                                        TableLiteral(
                                                            TriviaList(),
                                                            "dos",
                                                            TriviaList())),
                                                    Token(
                                                        TriviaList([Whitespace("  ")]),
                                                        SyntaxKind.VerticalBarToken,
                                                        TriviaList([Space])),
                                                    TextTableCell(
                                                        TableLiteral(
                                                            TriviaList(),
                                                            "tres",
                                                            TriviaList([Space]))),
                                                    Token(
                                                        TriviaList(),
                                                        SyntaxKind.VerticalBarToken,
                                                        TriviaList([Space])),
                                                    TextTableCell(
                                                        TableLiteral(
                                                            TriviaList(),
                                                            "quatro",
                                                            TriviaList([Space, Space]))),
                                                    Token(
                                                        TriviaList(),
                                                        SyntaxKind.VerticalBarToken,
                                                        TriviaList([Space])),
                                                    TextTableCell(
                                                        TableLiteral(
                                                            TriviaList(),
                                                            "cinco",
                                                            TriviaList([Space, Space])))
                                                ]),
                                                Token(
                                                    TriviaList(),
                                                    SyntaxKind.VerticalBarToken,
                                                    TriviaList([EnvironmentNewLine])))
                                        ]))
                                )
                            ])
                        )
                    ])
                ),
                Token(
                    TriviaList(),
                    SyntaxKind.EndOfFileToken,
                    TriviaList())));

        tree.ToString().Should().Be(source);
    }
}
