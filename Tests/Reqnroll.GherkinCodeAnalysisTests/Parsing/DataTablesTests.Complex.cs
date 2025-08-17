using Reqnroll.CodeAnalysis.Gherkin.Syntax;
using Xunit.Abstractions;

namespace Reqnroll.CodeAnalysis.Gherkin.Parsing;

using static SyntaxFactory;

public partial class DataTablesTests(ITestOutputHelper output)
{
    [Fact]
    public void SparselyPopulatedTablesAreRepresentedInTree()
    {
        const string source =
            """
            Feature: Complex DataTables

              Scenario: multi-line and complex tables
                Given a table with
                  | foo | bar | baz |
                  |  1  |  2  |  3  |
                  |  4  |  5  |  6  |
                And a table with empty cells
                  | foo | bar | baz |
                  |     |  2  |     |
                  |  4  |     |  6  |
                And a table with whitespace
                  | foo | bar | baz |
                  |     |     |     |
                  |  4  |  5  |     |
                  |     |  8  |  9  |

            """;

        output.WriteLine("Source text");
        output.WriteLine("-----------");
        output.WriteLine(source);

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
                            "Complex DataTables",
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
                                    "multi-line and complex tables",
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
                                                "a table with",
                                                TriviaList([EnvironmentNewLine]))
                                        ])),
                                    StepTable(
                                        Table(
                                            List([
                                                TableRow(Token(
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
                                                            TriviaList())),
                                                    Token(
                                                        TriviaList([Space]),
                                                        SyntaxKind.VerticalBarToken,
                                                        TriviaList([Space])),
                                                    TextTableCell(
                                                        TableLiteral(
                                                            TriviaList(),
                                                            "baz",
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
                                                        TriviaList([Whitespace("  ")])),
                                                    TableCellList([
                                                        TextTableCell(
                                                            TableLiteral(
                                                                TriviaList(),
                                                                "1",
                                                                TriviaList())),
                                                        Token(
                                                            TriviaList([Whitespace("  ")]),
                                                            SyntaxKind.VerticalBarToken,
                                                            TriviaList([Whitespace("  ")])),
                                                        TextTableCell(
                                                            TableLiteral(
                                                                TriviaList(),
                                                                "2",
                                                                TriviaList())),
                                                        Token(
                                                            TriviaList([Whitespace("  ")]),
                                                            SyntaxKind.VerticalBarToken,
                                                            TriviaList([Whitespace("  ")])),
                                                        TextTableCell(
                                                            TableLiteral(
                                                                TriviaList(),
                                                                "3",
                                                                TriviaList()))
                                                    ]),
                                                    Token(
                                                        TriviaList([Whitespace("  ")]),
                                                        SyntaxKind.VerticalBarToken,
                                                        TriviaList([EnvironmentNewLine]))),
                                                TableRow(
                                                    Token(
                                                        TriviaList([Whitespace("      ")]),
                                                        SyntaxKind.VerticalBarToken,
                                                        TriviaList([Whitespace("  ")])),
                                                    TableCellList([
                                                        TextTableCell(
                                                            TableLiteral(
                                                                TriviaList(),
                                                                "4",
                                                                TriviaList())),
                                                        Token(
                                                            TriviaList([Whitespace("  ")]),
                                                            SyntaxKind.VerticalBarToken,
                                                            TriviaList([Whitespace("  ")])),
                                                        TextTableCell(
                                                            TableLiteral(
                                                                TriviaList(),
                                                                "5",
                                                                TriviaList())),
                                                        Token(
                                                            TriviaList([Whitespace("  ")]),
                                                            SyntaxKind.VerticalBarToken,
                                                            TriviaList([Whitespace("  ")])),
                                                        TextTableCell(
                                                            TableLiteral(
                                                                TriviaList(),
                                                                "6",
                                                                TriviaList()))
                                                    ]),
                                                    Token(
                                                        TriviaList([Whitespace("  ")]),
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
                                                "a table with empty cells",
                                                TriviaList([EnvironmentNewLine]))
                                        ])),
                                    StepTable(
                                        Table(
                                            List([
                                                TableRow(Token(
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
                                                            TriviaList())),
                                                    Token(
                                                        TriviaList([Space]),
                                                        SyntaxKind.VerticalBarToken,
                                                        TriviaList([Space])),
                                                    TextTableCell(
                                                        TableLiteral(
                                                            TriviaList(),
                                                            "baz",
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
                                                        TriviaList([Whitespace("     ")])),
                                                    TableCellList([
                                                        EmptyTableCell(),
                                                        Token(
                                                            TriviaList(),
                                                            SyntaxKind.VerticalBarToken,
                                                            TriviaList([Whitespace("  ")])),
                                                        TextTableCell(
                                                            TableLiteral(
                                                                TriviaList(),
                                                                "2",
                                                                TriviaList())),
                                                        Token(
                                                            TriviaList([Whitespace("  ")]),
                                                            SyntaxKind.VerticalBarToken,
                                                            TriviaList([Whitespace("     ")])),
                                                        EmptyTableCell()
                                                    ]),
                                                    Token(
                                                        TriviaList(),
                                                        SyntaxKind.VerticalBarToken,
                                                        TriviaList([EnvironmentNewLine]))),
                                                TableRow(
                                                    Token(
                                                        TriviaList([Whitespace("      ")]),
                                                        SyntaxKind.VerticalBarToken,
                                                        TriviaList([Whitespace("  ")])),
                                                    TableCellList([
                                                        TextTableCell(
                                                            TableLiteral(
                                                                TriviaList(),
                                                                "4",
                                                                TriviaList())),
                                                        Token(
                                                            TriviaList([Whitespace("  ")]),
                                                            SyntaxKind.VerticalBarToken,
                                                            TriviaList([Whitespace("     ")])),
                                                        EmptyTableCell(),
                                                        Token(
                                                            TriviaList(),
                                                            SyntaxKind.VerticalBarToken,
                                                            TriviaList([Whitespace("  ")])),
                                                        TextTableCell(
                                                            TableLiteral(
                                                                TriviaList(),
                                                                "6",
                                                                TriviaList()))
                                                    ]),
                                                    Token(
                                                        TriviaList([Whitespace("  ")]),
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
                                                "a table with whitespace",
                                                TriviaList([EnvironmentNewLine]))
                                        ])),
                                    StepTable(
                                        Table(
                                            List([
                                                TableRow(Token(
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
                                                            TriviaList())),
                                                    Token(
                                                        TriviaList([Space]),
                                                        SyntaxKind.VerticalBarToken,
                                                        TriviaList([Space])),
                                                    TextTableCell(
                                                        TableLiteral(
                                                            TriviaList(),
                                                            "baz",
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
                                                        TriviaList([Whitespace("     ")])),
                                                    TableCellList([
                                                        EmptyTableCell(),
                                                        Token(
                                                            TriviaList(),
                                                            SyntaxKind.VerticalBarToken,
                                                            TriviaList([Whitespace("     ")])),
                                                        EmptyTableCell(),
                                                        Token(
                                                            TriviaList(),
                                                            SyntaxKind.VerticalBarToken,
                                                            TriviaList([Whitespace("     ")])),
                                                        EmptyTableCell()
                                                    ]),
                                                    Token(
                                                        TriviaList(),
                                                        SyntaxKind.VerticalBarToken,
                                                        TriviaList([EnvironmentNewLine]))),
                                                TableRow(
                                                    Token(
                                                        TriviaList([Whitespace("      ")]),
                                                        SyntaxKind.VerticalBarToken,
                                                        TriviaList([Whitespace("  ")])),
                                                    TableCellList([
                                                        TextTableCell(
                                                            TableLiteral(
                                                                TriviaList(),
                                                                "4",
                                                                TriviaList())),
                                                        Token(
                                                            TriviaList([Whitespace("  ")]),
                                                            SyntaxKind.VerticalBarToken,
                                                            TriviaList([Whitespace("  ")])),
                                                        TextTableCell(
                                                            TableLiteral(
                                                                TriviaList(),
                                                                "5",
                                                                TriviaList())),
                                                        Token(
                                                            TriviaList([Whitespace("  ")]),
                                                            SyntaxKind.VerticalBarToken,
                                                            TriviaList([Whitespace("     ")])),
                                                        EmptyTableCell()
                                                    ]),
                                                    Token(
                                                        TriviaList(),
                                                        SyntaxKind.VerticalBarToken,
                                                        TriviaList([EnvironmentNewLine]))),
                                                TableRow(
                                                    Token(
                                                        TriviaList([Whitespace("      ")]),
                                                        SyntaxKind.VerticalBarToken,
                                                        TriviaList([Whitespace("     ")])),
                                                    TableCellList([
                                                        EmptyTableCell(),
                                                        Token(
                                                            TriviaList(),
                                                            SyntaxKind.VerticalBarToken,
                                                            TriviaList([Whitespace("  ")])),
                                                        TextTableCell(
                                                            TableLiteral(
                                                                TriviaList(),
                                                                "8",
                                                                TriviaList())),
                                                        Token(
                                                            TriviaList([Whitespace("  ")]),
                                                            SyntaxKind.VerticalBarToken,
                                                            TriviaList([Whitespace("  ")])),
                                                        TextTableCell(
                                                            TableLiteral(
                                                                TriviaList(),
                                                                "9",
                                                                TriviaList()))
                                                    ]),
                                                    Token(
                                                        TriviaList([Whitespace("  ")]),
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
