using Reqnroll.CodeAnalysis.Gherkin.Syntax;

namespace Reqnroll.CodeAnalysis.Gherkin.Parsing;

using static SyntaxFactory;

public partial class DataTablesTests
{
    [Fact]
    public void ComplexDataTablesAreRepresentedInTree()
    {
        // Taken from good/datatables.feature
        const string source =
            """
            Feature: DataTables

              Scenario: minimalistic
                Given a simple data table
                  | foo | bar |
                  | boz | boo |
                And a data table with a single cell
                  | foo |
                And a data table with different fromatting
                  |   foo|bar|    boz    |    
                And a data table with an empty cell
                  |foo||boz|
                And a data table with comments and newlines inside
                  | foo | bar |

                  | boz  | boo  |
                  # this is a comment
                  | boz2 | boo2 |

            """;

        var tree = GherkinSyntaxTree.ParseText(source);
        var expected = GherkinDocument(
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
                            "DataTables",
                            TriviaList([EnvironmentNewLine]))),
                    examples: List([
                        Example(
                            Token(
                                TriviaList([EnvironmentNewLine, Whitespace("  ")]),
                                SyntaxKind.ExampleKeyword,
                                "Scenario",
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
                                        SyntaxKind.ContextStepKeyword,
                                        "Given",
                                        TriviaList([Space])),
                                    LiteralText(
                                        TokenList([
                                            Literal(
                                                TriviaList(),
                                                "a simple data table",
                                                TriviaList([EnvironmentNewLine]))
                                        ])),
                                    StepTable(
                                        Table(
                                            List([
                                                TableRow(
                                                    Token(
                                                        TriviaList([Whitespace("      ")]),
                                                        SyntaxKind.VerticalBarToken,
                                                        TriviaList([Space])),
                                                    TableCellList([
                                                        TextTableCell(
                                                            TableLiteral(
                                                                TriviaList(),
                                                                "foo",
                                                                TriviaList())),
                                                        Token(
                                                            TriviaList([Space]),
                                                            SyntaxKind.VerticalBarToken,
                                                            TriviaList([Space])),
                                                        TextTableCell(
                                                            TableLiteral(
                                                                TriviaList(),
                                                                "bar",
                                                                TriviaList()))
                                                    ]),
                                                    Token(
                                                        TriviaList([Space]),
                                                        SyntaxKind.VerticalBarToken,
                                                        TriviaList([EnvironmentNewLine]))),
                                                TableRow(
                                                    Token(
                                                        TriviaList([Whitespace("      ")]),
                                                        SyntaxKind.VerticalBarToken,
                                                        TriviaList([Space])),
                                                    TableCellList([
                                                        TextTableCell(
                                                            TableLiteral(
                                                                TriviaList(),
                                                                "boz",
                                                                TriviaList())),
                                                        Token(
                                                            TriviaList([Space]),
                                                            SyntaxKind.VerticalBarToken,
                                                            TriviaList([Space])),
                                                        TextTableCell(
                                                            TableLiteral(
                                                                TriviaList(),
                                                                "boo",
                                                                TriviaList()))
                                                    ]),
                                                    Token(
                                                        TriviaList([Space]),
                                                        SyntaxKind.VerticalBarToken,
                                                        TriviaList([EnvironmentNewLine])))
                                            ])))),
                                Step(
                                    Token(
                                        TriviaList([Whitespace("    ")]),
                                        SyntaxKind.ConjunctionStepKeyword,
                                        "And",
                                        TriviaList([Space])),
                                    LiteralText(
                                        TokenList([
                                            Literal(
                                                TriviaList(),
                                                "a data table with a single cell",
                                                TriviaList([EnvironmentNewLine]))
                                        ])),
                                    StepTable(
                                        Table(
                                            List([
                                                TableRow(
                                                    Token(
                                                        TriviaList([Whitespace("      ")]),
                                                        SyntaxKind.VerticalBarToken,
                                                        TriviaList([Space])),
                                                    TableCellList([
                                                        TextTableCell(
                                                            TableLiteral(
                                                                TriviaList(),
                                                                "foo",
                                                                TriviaList()))
                                                    ]),
                                                    Token(
                                                        TriviaList([Space]),
                                                        SyntaxKind.VerticalBarToken,
                                                        TriviaList([EnvironmentNewLine]))),
                                            ])))),
                                Step(
                                    Token(
                                        TriviaList([Whitespace("    ")]),
                                        SyntaxKind.ConjunctionStepKeyword,
                                        "And",
                                        TriviaList([Space])),
                                    LiteralText(
                                        TokenList([
                                            Literal(
                                                TriviaList(),
                                                "a data table with different fromatting",
                                                TriviaList([EnvironmentNewLine]))
                                        ])),
                                    StepTable(
                                        Table(
                                            List([
                                                TableRow(
                                                    Token(
                                                        TriviaList([Whitespace("      ")]),
                                                        SyntaxKind.VerticalBarToken,
                                                        TriviaList([Whitespace("   ")])),
                                                    TableCellList([
                                                        TextTableCell(
                                                            TableLiteral(
                                                                TriviaList(),
                                                                "foo",
                                                                TriviaList())),
                                                        Token(
                                                            TriviaList(),
                                                            SyntaxKind.VerticalBarToken,
                                                            TriviaList()),
                                                        TextTableCell(
                                                            TableLiteral(
                                                                TriviaList(),
                                                                "bar",
                                                                TriviaList())),
                                                        Token(
                                                            TriviaList(),
                                                            SyntaxKind.VerticalBarToken,
                                                            TriviaList([Whitespace("    ")])),
                                                        TextTableCell(
                                                            TableLiteral(
                                                                TriviaList(),
                                                                "boz",
                                                                TriviaList()))
                                                    ]),
                                                    Token(
                                                        TriviaList([Whitespace("    ")]),
                                                        SyntaxKind.VerticalBarToken,
                                                        TriviaList([Whitespace("    "), EnvironmentNewLine]))),
                                            ])))),
                                Step(
                                    Token(
                                        TriviaList([Whitespace("    ")]),
                                        SyntaxKind.ConjunctionStepKeyword,
                                        "And",
                                        TriviaList([Space])),
                                    LiteralText(
                                        TokenList([
                                            Literal(
                                                TriviaList(),
                                                "a data table with an empty cell",
                                                TriviaList([EnvironmentNewLine]))
                                        ])),
                                    StepTable(
                                        Table(
                                            List([
                                                TableRow(
                                                    Token(
                                                        TriviaList([Whitespace("      ")]),
                                                        SyntaxKind.VerticalBarToken,
                                                        TriviaList()),
                                                    TableCellList([
                                                        TextTableCell(
                                                            TableLiteral(
                                                                TriviaList(),
                                                                "foo",
                                                                TriviaList())),
                                                        Token(
                                                            TriviaList(),
                                                            SyntaxKind.VerticalBarToken,
                                                            TriviaList()),
                                                        EmptyTableCell(),
                                                        Token(
                                                            TriviaList(),
                                                            SyntaxKind.VerticalBarToken,
                                                            TriviaList()),
                                                        TextTableCell(
                                                            TableLiteral(
                                                                TriviaList(),
                                                                "boz",
                                                                TriviaList()))
                                                    ]),
                                                    Token(
                                                        TriviaList(),
                                                        SyntaxKind.VerticalBarToken,
                                                        TriviaList([EnvironmentNewLine]))),
                                            ])))),
                                Step(
                                    Token(
                                        TriviaList([Whitespace("    ")]),
                                        SyntaxKind.ConjunctionStepKeyword,
                                        "And",
                                        TriviaList([Space])),
                                    LiteralText(
                                        TokenList([
                                            Literal(
                                                TriviaList(),
                                                "a data table with comments and newlines inside",
                                                TriviaList([EnvironmentNewLine]))
                                        ])),
                                    StepTable(
                                        Table(
                                            List([
                                                TableRow(
                                                    Token(
                                                        TriviaList([Whitespace("      ")]),
                                                        SyntaxKind.VerticalBarToken,
                                                        TriviaList([Space])),
                                                    TableCellList([
                                                        TextTableCell(
                                                            TableLiteral(
                                                                TriviaList(),
                                                                "foo",
                                                                TriviaList())),
                                                        Token(
                                                            TriviaList([Space]),
                                                            SyntaxKind.VerticalBarToken,
                                                            TriviaList([Space])),
                                                        TextTableCell(
                                                            TableLiteral(
                                                                TriviaList(),
                                                                "bar",
                                                                TriviaList()))
                                                    ]),
                                                    Token(
                                                        TriviaList([Space]),
                                                        SyntaxKind.VerticalBarToken,
                                                        TriviaList([EnvironmentNewLine]))),
                                                TableRow(
                                                    Token(
                                                        TriviaList([
                                                            EnvironmentNewLine,
                                                            Whitespace("      ")
                                                        ]),
                                                        SyntaxKind.VerticalBarToken,
                                                        TriviaList([Space])),
                                                    TableCellList([
                                                        TextTableCell(
                                                            TableLiteral(
                                                                TriviaList(),
                                                                "boz",
                                                                TriviaList())),
                                                        Token(
                                                            TriviaList([Whitespace("  ")]),
                                                            SyntaxKind.VerticalBarToken,
                                                            TriviaList([Space])),
                                                        TextTableCell(
                                                            TableLiteral(
                                                                TriviaList(),
                                                                "boo",
                                                                TriviaList()))
                                                    ]),
                                                    Token(
                                                        TriviaList([Whitespace("  ")]),
                                                        SyntaxKind.VerticalBarToken,
                                                        TriviaList([EnvironmentNewLine]))),
                                                TableRow(
                                                    Token(
                                                        TriviaList([
                                                            Whitespace("      "), Comment("# this is a comment"), EnvironmentNewLine,
                                                            Whitespace("      ")]),
                                                        SyntaxKind.VerticalBarToken,
                                                        TriviaList([Space])),
                                                    TableCellList([
                                                        TextTableCell(
                                                            TableLiteral(
                                                                TriviaList(),
                                                                "boz2",
                                                                TriviaList())),
                                                        Token(
                                                            TriviaList([Space]),
                                                            SyntaxKind.VerticalBarToken,
                                                            TriviaList([Space])),
                                                        TextTableCell(
                                                            TableLiteral(
                                                                TriviaList(),
                                                                "boo2",
                                                                TriviaList()))
                                                    ]),
                                                    Token(
                                                        TriviaList([Space]),
                                                        SyntaxKind.VerticalBarToken,
                                                        TriviaList([EnvironmentNewLine])))
                                            ]))))
                            ]))
                    ])),
                Token(
                    TriviaList(),
                    SyntaxKind.EndOfFileToken,
                    TriviaList()));

        tree.GetRoot().Should().BeEquivalentTo(expected);

        tree.ToString().Should().Be(source);
    }
}
