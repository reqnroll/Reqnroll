using Reqnroll.CodeAnalysis.Gherkin.Syntax;

namespace Reqnroll.CodeAnalysis.Gherkin.Parsing;

using static SyntaxFactory;

public partial class TagsTests
{
    [Fact]
    public void TaggedFeatureWithScenarioOutlineIsRepresentedInTree()
    {
        // Taken from good/tagged_feature_with_scenario_outline.feature
        const string source =
            """
            @sometag
            Feature: Foo

              Scenario Outline: Bar
                Then Baz

                Examples:
                | name |
                | X    |
                | Y    |

            """;

        var tree = GherkinSyntaxTree.ParseText(source);

        tree.GetRoot().Should().BeEquivalentTo(
            GherkinDocument(
                Feature(
                    List([
                        Tag(
                            Token(TriviaList(), SyntaxKind.AtToken, TriviaList()),
                            Name(TriviaList(), "sometag", TriviaList([EnvironmentNewLine])))
                    ]),
                    Token(
                        TriviaList(),
                        SyntaxKind.FeatureKeyword,
                        "Feature",
                        TriviaList()),
                    ColonWithSpace,
                    Name(
                        TriviaList(),
                        "Foo",
                        TriviaList([EnvironmentNewLine, EnvironmentNewLine])),
                    examples: List([
                        Example(
                            Token(
                                TriviaList([Whitespace("  ")]),
                                SyntaxKind.ExampleKeyword,
                                "Example Outline",
                                TriviaList()),
                            ColonWithSpace,
                            Name(
                                TriviaList(),
                                "Bar",
                                TriviaList([EnvironmentNewLine])),
                            steps: List([
                                Step(
                                    Token(
                                        TriviaList([Whitespace("    ")]),
                                        SyntaxKind.OutcomeStepKeyword,
                                        "Then",
                                        TriviaList([Space])),
                                    StepText(
                                        TriviaList(),
                                        "Baz",
                                        TriviaList([EnvironmentNewLine])))
                            ]),
                            examples: List([
                                Examples(
                                    Token(
                                        TriviaList([EnvironmentNewLine, Whitespace("    ")]),
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
                                                    TriviaList([Whitespace("      ")]),
                                                    SyntaxKind.VerticalBarToken,
                                                    TriviaList([Space])),
                                                TableCellList([
                                                    TableCell(
                                                        TableLiteral(
                                                            TriviaList(),
                                                            "name",
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
                                                    TableCell(
                                                        TableLiteral(
                                                            TriviaList(),
                                                            "X",
                                                            TriviaList()))
                                                ]),
                                                Token(
                                                    TriviaList([Whitespace("    ")]),
                                                    SyntaxKind.VerticalBarToken,
                                                    TriviaList([EnvironmentNewLine]))),
                                            TableRow(
                                                Token(
                                                    TriviaList([Whitespace("      ")]),
                                                    SyntaxKind.VerticalBarToken,
                                                    TriviaList([Space])),
                                                TableCellList([
                                                    TableCell(
                                                        TableLiteral(
                                                            TriviaList(),
                                                            "Y",
                                                            TriviaList()))
                                                ]),
                                                Token(
                                                    TriviaList([Whitespace("    ")]),
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
