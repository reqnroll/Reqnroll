using Reqnroll.CodeAnalysis.Gherkin.Syntax;

namespace Reqnroll.CodeAnalysis.Gherkin.Parsing;

using static SyntaxFactory;

public partial class ScenarioOutlineTests
{
    [Fact]
    public void IncompleteScenarioOutlinesAreRepresentedInTree()
    {
        // Taken from good/incomplete_scenario_outline.feature
        const string source =
            """
            Feature: Incomplete scenario outlines

              Background: Adding a background won't make a pickle
                * a step

              Scenario Outline: steps, no examples
                Given a step

              Scenario Outline: no steps, no examples

              Scenario Outline: no steps, no table

                Examples:

              Scenario Outline: no steps, only table header

                Examples:
                | what |

              Scenario Outline: no steps, one example header

                Examples:
                | nope |
                | nada |

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
                        "Incomplete scenario outlines",
                        TriviaList([EnvironmentNewLine])),
                    background: Background(
                        Token(
                            TriviaList([EnvironmentNewLine, Whitespace("  ")]),
                            SyntaxKind.BackgroundKeyword,
                            "Background",
                            TriviaList()),
                        ColonWithSpace,
                        Name(
                            TriviaList(),
                            "Adding a background won't make a pickle",
                            TriviaList([EnvironmentNewLine])),
                        steps: List([
                            Step(
                                Token(
                                    TriviaList([Whitespace("    ")]),
                                    SyntaxKind.WildcardStepKeyword,
                                    TriviaList([Space])),
                                StepText(
                                    TriviaList(),
                                    "a step",
                                    TriviaList([EnvironmentNewLine])))
                        ])),
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
                                "steps, no examples",
                                TriviaList([EnvironmentNewLine])),
                            steps: List([
                                Step(
                                    Token(
                                        TriviaList([Whitespace("  ")]),
                                        SyntaxKind.ContextStepKeyword,
                                        "Given",
                                        TriviaList([Space])),
                                    StepText(
                                        TriviaList(),
                                        "a step",
                                        TriviaList([EnvironmentNewLine])))
                            ])),
                        Example(
                            Token(
                                TriviaList([EnvironmentNewLine, Whitespace("  ")]),
                                SyntaxKind.ExampleKeyword,
                                "Example Outline",
                                TriviaList()),
                            ColonWithSpace,
                            Name(
                                TriviaList(),
                                "no steps, no examples",
                                TriviaList([EnvironmentNewLine])),
                            steps: default),
                        Example(
                            Token(
                                TriviaList([EnvironmentNewLine, Whitespace("  ")]),
                                SyntaxKind.ExampleKeyword,
                                "Example Outline",
                                TriviaList()),
                            ColonWithSpace,
                            Name(
                                TriviaList(),
                                "no steps, no table",
                                TriviaList([EnvironmentNewLine, EnvironmentNewLine, Whitespace("  ")])),
                            steps: default,
                            examples: List([
                                Examples(
                                    Token(
                                        TriviaList(),
                                        SyntaxKind.ExamplesKeyword,
                                        "Examples",
                                        TriviaList()),
                                    Token(
                                        TriviaList(),
                                        SyntaxKind.ColonToken,
                                        TriviaList([EnvironmentNewLine])),
                                    table: default)
                                ])),
                        Example(
                            Token(
                                TriviaList([EnvironmentNewLine, Whitespace("  ")]),
                                SyntaxKind.ExampleKeyword,
                                "Example Outline",
                                TriviaList()),
                            ColonWithSpace,
                            Name(
                                TriviaList(),
                                "no steps, only table header",
                                TriviaList([EnvironmentNewLine, EnvironmentNewLine, Whitespace("  ")])),
                            steps: default,
                            examples: List([
                                Examples(
                                    Token(
                                        TriviaList(),
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
                                                    TriviaList()),
                                                TableCellList([
                                                    TableLiteral(
                                                        TriviaList([Space]),
                                                        "what",
                                                        TriviaList([Space]))
                                                ]),
                                                Token(
                                                    TriviaList(),
                                                    SyntaxKind.VerticalBarToken,
                                                    TriviaList([EnvironmentNewLine])))
                                        ])))
                            ])),
                        Example(
                            Token(
                                TriviaList([EnvironmentNewLine, Whitespace("  ")]),
                                SyntaxKind.ExampleKeyword,
                                "Example Outline",
                                TriviaList()),
                            ColonWithSpace,
                            Name(
                                TriviaList(),
                                "no steps, one example header",
                                TriviaList([EnvironmentNewLine])),
                            steps: default,
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
                                                    TableCell(
                                                        TableLiteral(
                                                            TriviaList(),
                                                            "nope",
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
                                                    TriviaList([Space])),
                                                TableCellList([
                                                    TableCell(
                                                        Name(
                                                            TriviaList(),
                                                            "nada",
                                                            TriviaList()))
                                                ]),
                                                Token(
                                                    TriviaList([Space]),
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
