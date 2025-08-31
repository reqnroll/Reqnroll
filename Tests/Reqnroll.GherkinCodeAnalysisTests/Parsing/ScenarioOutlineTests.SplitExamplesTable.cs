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
                    ColonWithSpace,
                    Name(
                        TriviaList(),
                        "Example Outline",
                        TriviaList([EnvironmentNewLine])),
                    examples: List([
                        Example(
                            Token(
                                TriviaList([EnvironmentNewLine, Whitespace("  ")]),
                                SyntaxKind.ExampleKeyword,
                                "Example Outline",
                                TriviaList()),
                            ColonWithSpace,
                            Name(
                                TriviaList(),
                                "eating",
                                TriviaList([EnvironmentNewLine])),
                            steps: List([
                                Step(
                                    Token(
                                        TriviaList([Whitespace("  ")]),
                                        SyntaxKind.ContextStepKeyword,
                                        "Given",
                                        TriviaList([Space])),
                                    Name(
                                        TriviaList(),
                                        "there are <start> cucumbers",
                                        TriviaList([EnvironmentNewLine]))),
                                Step(
                                    Token(
                                        TriviaList([Whitespace("  ")]),
                                        SyntaxKind.ActionStepKeyword,
                                        "When",
                                        TriviaList([Space])),
                                    StepText(
                                        TriviaList(),
                                        "I eat <eat> cucumbers",
                                        TriviaList([EnvironmentNewLine]))),
                                Step(
                                    Token(
                                        TriviaList([Whitespace("  ")]),
                                        SyntaxKind.OutcomeStepKeyword,
                                        "Then",
                                        TriviaList([Space])),
                                    StepText(
                                        TriviaList(),
                                        "I should have <left> cucumbers",
                                        TriviaList([EnvironmentNewLine, EnvironmentNewLine])))
                            ]),
                            examples: List([
                                Examples(
                                    Token(
                                        TriviaList([Whitespace("  ")]),
                                        SyntaxKind.ExamplesKeyword,
                                        "Examples",
                                        TriviaList()),
                                    ColonWithSpace,
                                    Name(
                                        TriviaList(),
                                        "first",
                                        TriviaList([EnvironmentNewLine])),
                                    table: Table(
                                        List([
                                            TableRow(
                                                Token(
                                                    TriviaList([Whitespace("    ")]),
                                                    SyntaxKind.VerticalBarToken,
                                                    TriviaList([Space])),
                                                TableCellList([
                                                    TableCell(
                                                        TableLiteral(
                                                            TriviaList(),
                                                            "start",
                                                            TriviaList())),
                                                    Token(
                                                        TriviaList([Space]),
                                                        SyntaxKind.VerticalBarToken,
                                                        TriviaList([Space])),
                                                    TableCell(
                                                        TableLiteral(
                                                            TriviaList(),
                                                            "eat",
                                                            TriviaList())),
                                                    Token(
                                                        TriviaList([Space]),
                                                        SyntaxKind.VerticalBarToken,
                                                        TriviaList([Space])),
                                                    TableCell(
                                                        TableLiteral(
                                                            TriviaList(),
                                                            "left",
                                                            TriviaList()))
                                                ]),
                                                Token(
                                                    TriviaList([Space]),
                                                    SyntaxKind.VerticalBarToken,
                                                    TriviaList([EnvironmentNewLine]))),
                                            TableRow(
                                                Token(
                                                    TriviaList([Whitespace("    ")]),
                                                    SyntaxKind.VerticalBarToken,
                                                    TriviaList(Whitespace("  "))),
                                                TableCellList([
                                                    TableCell(
                                                        TableLiteral(
                                                            TriviaList(),
                                                            "12",
                                                            TriviaList())),
                                                    Token(
                                                        TriviaList([Whitespace("   ")]),
                                                        SyntaxKind.VerticalBarToken,
                                                        TriviaList([Whitespace("  ")])),
                                                    TableCell(
                                                        TableLiteral(
                                                            TriviaList(),
                                                            "5",
                                                            TriviaList())),
                                                    Token(
                                                        TriviaList([Whitespace("  ")]),
                                                        SyntaxKind.VerticalBarToken,
                                                        TriviaList([Whitespace("  ")])),
                                                    TableCell(
                                                        TableLiteral(
                                                            TriviaList(),
                                                            "7",
                                                            TriviaList()))
                                                ]),
                                                Token(
                                                    TriviaList([Whitespace("   ")]),
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
                                        ColonWithSpace,
                                        Name(
                                            TriviaList(),
                                            "second",
                                            TriviaList([EnvironmentNewLine])),
                                        table: Table(
                                            List([
                                                TableRow(
                                                    Token(
                                                        TriviaList([Whitespace("    ")]),
                                                        SyntaxKind.VerticalBarToken,
                                                        TriviaList([Space])),
                                                    TableCellList([
                                                        TableCell(
                                                            TableLiteral(
                                                                TriviaList(),
                                                                "start",
                                                                TriviaList())),
                                                        Token(
                                                            TriviaList([Space]),
                                                            SyntaxKind.VerticalBarToken,
                                                            TriviaList([Space])),
                                                        TableCell(
                                                            TableLiteral(
                                                                TriviaList(),
                                                                "eat",
                                                                TriviaList())),
                                                        Token(
                                                            TriviaList([Space]),
                                                            SyntaxKind.VerticalBarToken,
                                                            TriviaList([Space])),
                                                        TableCell(
                                                            TableLiteral(
                                                                TriviaList(),
                                                                "left",
                                                                TriviaList()))
                                                    ]),
                                                    Token(
                                                        TriviaList([Space]),
                                                        SyntaxKind.VerticalBarToken,
                                                        TriviaList([EnvironmentNewLine]))),
                                                TableRow(
                                                    Token(
                                                        TriviaList([Whitespace("    ")]),
                                                        SyntaxKind.VerticalBarToken,
                                                        TriviaList(Whitespace("  "))),
                                                    TableCellList([
                                                        TableCell(
                                                            TableLiteral(
                                                                TriviaList(),
                                                                "20",
                                                                TriviaList())),
                                                        Token(
                                                            TriviaList([Whitespace("   ")]),
                                                            SyntaxKind.VerticalBarToken,
                                                            TriviaList([Whitespace("  ")])),
                                                        TableCell(
                                                            TableLiteral(
                                                                TriviaList(),
                                                                "5",
                                                                TriviaList())),
                                                        Token(
                                                            TriviaList([Whitespace("  ")]),
                                                            SyntaxKind.VerticalBarToken,
                                                            TriviaList([Whitespace("  ")])),
                                                        TableCell(
                                                            TableLiteral(
                                                                TriviaList(),
                                                                "15",
                                                                TriviaList()))
                                                    ]),
                                                    Token(
                                                        TriviaList([Whitespace("  ")]),
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
