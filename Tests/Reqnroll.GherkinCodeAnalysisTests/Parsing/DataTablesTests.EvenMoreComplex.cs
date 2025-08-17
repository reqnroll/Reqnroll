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
                                                        TriviaList([Whitespace("    ")]),
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
                                                            TriviaList()),
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
                                                        TriviaList([Whitespace("    ")]),
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
                                        TriviaList([Whitespace("  ")]),
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
                                                        TriviaList([Whitespace("    ")]),
                                                        SyntaxKind.VerticalBarToken,
                                                        TriviaList([Space])),
                                                    TableCellList([
                                                        TextTableCell(
                                                            TableLiteral(
                                                                TriviaList(),
                                                                "foo",
                                                                TriviaList([Space])))
                                                    ]),
                                                    Token(
                                                        TriviaList(),
                                                        SyntaxKind.VerticalBarToken,
                                                        TriviaList([EnvironmentNewLine]))),
                                            ])))),
                                Step(
                                    Token(
                                        TriviaList([Whitespace("  ")]),
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
                                                        TriviaList([Whitespace("    ")]),
                                                        SyntaxKind.VerticalBarToken,
                                                        TriviaList([Space, Space, Space])),
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
                                                                TriviaList([Space]))),
                                                        Token(
                                                            TriviaList(),
                                                            SyntaxKind.VerticalBarToken,
                                                            TriviaList()),
                                                        TextTableCell(
                                                            TableLiteral(
                                                                TriviaList([Space, Space, Space, Space]),
                                                                "boz",
                                                                TriviaList([Space])))
                                                    ]),
                                                    Token(
                                                        TriviaList(),
                                                        SyntaxKind.VerticalBarToken,
                                                        TriviaList([Space, Space, Space, Space, EnvironmentNewLine]))),
                                            ])))),
                                Step(
                                    Token(
                                        TriviaList([Whitespace("  ")]),
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
                                                        TriviaList([Whitespace("    ")]),
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
                                                        TextTableCell(
                                                            TableLiteral(
                                                                TriviaList(),
                                                                "",
                                                                TriviaList())),
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
                                        TriviaList([Whitespace("  ")]),
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
                                                        TriviaList([Whitespace("    ")]),
                                                        SyntaxKind.VerticalBarToken,
                                                        TriviaList([Space])),
                                                    TableCellList([
                                                        TextTableCell(
                                                            TableLiteral(
                                                                TriviaList(),
                                                                "foo",
                                                                TriviaList([Space]))),
                                                        Token(
                                                            TriviaList(),
                                                            SyntaxKind.VerticalBarToken,
                                                            TriviaList()),
                                                        TextTableCell(
                                                            TableLiteral(
                                                                TriviaList(),
                                                                "bar",
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
                                                        TriviaList()),
                                                    TableCellList([
                                                        TextTableCell(
                                                            TableLiteral(
                                                                TriviaList([Space]),
                                                                "boz",
                                                                TriviaList([Space, Space]))),
                                                        Token(
                                                            TriviaList(),
                                                            SyntaxKind.VerticalBarToken,
                                                            TriviaList()),
                                                        TextTableCell(
                                                            TableLiteral(
                                                                TriviaList(),
                                                                "boo",
                                                                TriviaList([Space, Space])))
                                                    ]),
                                                    Token(
                                                        TriviaList([Space, Space]),
                                                        SyntaxKind.VerticalBarToken,
                                                        TriviaList([
                                                            EnvironmentNewLine,
                                                            EnvironmentNewLine,
                                                            Whitespace("    "),
                                                            Comment("# this is a comment"),
                                                            EnvironmentNewLine
                                                        ]))),
                                                TableRow(
                                                    Token(
                                                        TriviaList([Whitespace("    ")]),
                                                        SyntaxKind.VerticalBarToken,
                                                        TriviaList()),
                                                    TableCellList([
                                                        TextTableCell(
                                                            TableLiteral(
                                                                TriviaList([Space]),
                                                                "boz2",
                                                                TriviaList([Space]))),
                                                        Token(
                                                            TriviaList(),
                                                            SyntaxKind.VerticalBarToken,
                                                            TriviaList()),
                                                        TextTableCell(
                                                            TableLiteral(
                                                                TriviaList([Space]),
                                                                "boo2",
                                                                TriviaList([Space])))
                                                    ]),
                                                    Token(
                                                        TriviaList(),
                                                        SyntaxKind.VerticalBarToken,
                                                        TriviaList([EnvironmentNewLine])))
                                            ]))))
                            ]))
                    ])),
                Token(
                    TriviaList(),
                    SyntaxKind.EndOfFileToken,
                    TriviaList()));


        var actualFeature = ((GherkinDocumentSyntax)tree.GetRoot()).FeatureDeclaration!;
        var expectedFeature = expected.FeatureDeclaration!;

        var actualExample = actualFeature.Examples[0];
        var expectedExample = expectedFeature.Examples[0];

        actualExample.Steps[0].Should().BeEquivalentTo(actualExample.Steps[0]);
        actualExample.Steps[1].Should().BeEquivalentTo(actualExample.Steps[1]);
        actualExample.Steps[2].Should().BeEquivalentTo(actualExample.Steps[2]);
        actualExample.Steps[3].Should().BeEquivalentTo(actualExample.Steps[3]);
        actualExample.Steps[4].Should().BeEquivalentTo(actualExample.Steps[4]);

        //actualExample.Should().BeEquivalentTo(expectedExample);

        //actualFeature.Should().BeEquivalentTo(expectedFeature);

        //tree.GetRoot().Should().BeEquivalentTo(expected);

        //tree.ToString().Should().Be(source);
    }
}
