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
                    Token(
                        TriviaList(),
                        SyntaxKind.ColonToken,
                        TriviaList([Space])),
                    LiteralText(
                        Literal(
                            TriviaList(),
                            "Incomplete scenario outlines",
                            TriviaList([EnvironmentNewLine]))),
                    background: Background(
                        Token(
                            TriviaList([EnvironmentNewLine, Whitespace("  ")]),
                            SyntaxKind.BackgroundKeyword,
                            "Background",
                            TriviaList()),
                        Token(
                            TriviaList(),
                            SyntaxKind.ColonToken,
                            TriviaList([Space])),
                        LiteralText(
                            Literal(
                                TriviaList(),
                                "Adding a background won't make a pickle",
                                TriviaList([EnvironmentNewLine]))),
                        steps: List([
                            Step(
                                Token(
                                    TriviaList([Whitespace("    ")]),
                                    SyntaxKind.AsteriskToken,
                                    TriviaList([Space])),
                                LiteralText(
                                    TokenList([
                                        Token(
                                            TriviaList(),
                                            SyntaxKind.LiteralToken,
                                            "a step",
                                            TriviaList([EnvironmentNewLine]))
                                    ])))
                        ])),
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
                                    "steps, no examples",
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
                                                "a step",
                                                TriviaList([EnvironmentNewLine]))
                                        ])))
                            ])),
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
                                    "no steps, no examples",
                                    TriviaList([EnvironmentNewLine]))),
                            steps: List<StepSyntax>()),
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
                                    "no steps, no table",
                                    TriviaList([EnvironmentNewLine, EnvironmentNewLine, Whitespace("  ")]))),
                            steps: List<StepSyntax>(),
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
                                        TriviaList([EnvironmentNewLine])))
                                ])),
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
                                    "no steps, only table header",
                                    TriviaList([EnvironmentNewLine, EnvironmentNewLine, Whitespace("  ")]))),
                            steps: List<StepSyntax>(),
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
                                                SeparatedList<PlainTextSyntax>([
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
                                    "no steps, one example header",
                                    TriviaList([EnvironmentNewLine]))),
                            steps: List<StepSyntax>(),
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
                                                SeparatedList<PlainTextSyntax>([
                                                    DirectiveIdentifier(
                                                        TriviaList(),
                                                        "nope",
                                                        TriviaList([Space]))
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
                                                SeparatedList<PlainTextSyntax>([
                                                    LiteralText(
                                                        Literal(
                                                            TriviaList(),
                                                            "nada",
                                                            TriviaList([Space])))
                                                ]),
                                                Token(
                                                    TriviaList(),
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
