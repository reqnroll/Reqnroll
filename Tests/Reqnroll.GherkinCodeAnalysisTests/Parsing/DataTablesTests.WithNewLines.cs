using Reqnroll.CodeAnalysis.Gherkin.Syntax;

namespace Reqnroll.CodeAnalysis.Gherkin.Parsing;

using static SyntaxFactory;

public partial class DataTablesTests
{
    [Fact]
    public void DataTablesWithNewLinesIsRepresentedInTree()
    {
        // Source: good/datatables_with_new_lines.feature
        const string source =
            """
            Feature: DataTables

              Scenario: some whitespace is important
                Given 3 lines of poetry on 5 lines
                  |  \nraindrops--\nher last kiss\ngoodbye.\n  |
                Given an example of negative space
                  |        lost        i n        space        |

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
                    Name(
                        TriviaList(),
                        "DataTables",
                        TriviaList([EnvironmentNewLine])),
                    examples: List([
                        Example(
                            Token(
                                TriviaList([EnvironmentNewLine, Whitespace("  ")]),
                                SyntaxKind.ExampleKeyword,
                                "Scenario",
                                TriviaList()),
                            ColonWithSpace,
                            Name(
                                TriviaList(),
                                "some whitespace is important",
                                TriviaList([EnvironmentNewLine])),
                            steps : List([
                                Step(
                                    Token(
                                        TriviaList([Whitespace("    ")]),
                                        SyntaxKind.ContextStepKeyword,
                                        "Given",
                                        TriviaList([Space])),
                                    StepText(
                                        TriviaList(),
                                        "3 lines of poetry on 5 lines",
                                        TriviaList([EnvironmentNewLine])),
                                    StepTable(
                                        Table(
                                            List([
                                                TableRow(
                                                    Token(
                                                        TriviaList([Whitespace("      ")]),
                                                        SyntaxKind.VerticalBarToken,
                                                        TriviaList([Whitespace("  ")])),
                                                    TableCellList([
                                                        TableCell(
                                                            TableLiteral(
                                                                TriviaList(),
                                                                @"\nraindrops--\nher last kiss\ngoodbye.\n",
                                                                "\nraindrops--\nher last kiss\ngoodbye.\n",
                                                                TriviaList()))
                                                    ]),
                                                    Token(
                                                        TriviaList([Whitespace("  ")]),
                                                        SyntaxKind.VerticalBarToken,
                                                        TriviaList([EnvironmentNewLine]))),
                                            ])))),
                                Step(
                                    Token(
                                        TriviaList([Whitespace("    ")]),
                                        SyntaxKind.ContextStepKeyword,
                                        "Given",
                                        TriviaList([Space])),
                                    StepText(
                                        TriviaList(),
                                        "an example of negative space",
                                        TriviaList([EnvironmentNewLine])),
                                    StepTable(
                                        Table(
                                            List([
                                                TableRow(
                                                    Token(
                                                        TriviaList([Whitespace("      ")]),
                                                        SyntaxKind.VerticalBarToken,
                                                        TriviaList([Whitespace("        ")])),
                                                    TableCellList([
                                                        TableCell(
                                                            TableLiteral(
                                                                TriviaList(),
                                                                "lost        i n        space",
                                                                TriviaList()))
                                                    ]),
                                                    Token(
                                                        TriviaList([Whitespace("        ")]),
                                                        SyntaxKind.VerticalBarToken,
                                                        TriviaList([EnvironmentNewLine]))),
                                            ]))))
                            ]))
                    ])),
                Token(
                    TriviaList(),
                    SyntaxKind.EndOfFileToken,
                    TriviaList())));

        tree.ToString().Should().Be(source);
    }
}
