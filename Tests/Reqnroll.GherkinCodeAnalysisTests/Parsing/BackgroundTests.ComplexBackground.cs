using Reqnroll.CodeAnalysis.Gherkin.Syntax;

namespace Reqnroll.CodeAnalysis.Gherkin.Parsing;

using static SyntaxFactory;

public partial class BackgroundTests
{
    [Fact]
    public void ComplexBackgroundsAreRepresentedInTree()
    {
        // Taken from good/complex_background.feature
        const string source =
            """
            Feature: Complex background
              We want to ensure PickleStep all have different IDs

              Background: a simple background
                Given the minimalism inside a background

              Scenario: minimalistic
                Given the minimalism

              Scenario: also minimalistic
                Given the minimalism

              Rule: My Rule

                Background:
                  Given a rule background step

                Scenario: with examples
                  Given the <value> minimalism

                  Examples:
                  | value |
                  | 1     |
                  | 2     |

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
                            "Complex background",
                            TriviaList([EnvironmentNewLine]))),
                    description: LiteralText(
                        TokenList([
                            Literal(
                                TriviaList([Whitespace("  ")]),
                                "We want to ensure PickleStep all have different IDs",
                                TriviaList([EnvironmentNewLine]))
                        ])
                    ),
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
                                "a simple background",
                                TriviaList([EnvironmentNewLine]))),
                        steps: List([
                            Step(
                                Token(
                                    TriviaList([Whitespace("    ")]),
                                    SyntaxKind.GivenKeyword,
                                    "Given",
                                    TriviaList([Space])),
                                LiteralText(
                                    TokenList([
                                        Token(
                                            TriviaList(),
                                            SyntaxKind.LiteralToken,
                                            "the minimalism inside a background",
                                            TriviaList([EnvironmentNewLine]))
                                    ])))
                        ])
                    ),
                    scenarios: List([
                        Scenario(
                            Token(
                                TriviaList([EnvironmentNewLine, EnvironmentNewLine]),
                                SyntaxKind.ScenarioKeyword,
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
                                        TriviaList([Whitespace("  ")]),
                                        SyntaxKind.GivenKeyword,
                                        "Given",
                                        TriviaList([Space])),
                                    LiteralText(
                                        TokenList([
                                            Token(
                                                TriviaList(),
                                                SyntaxKind.LiteralToken,
                                                "the minimalism",
                                                TriviaList([EnvironmentNewLine]))
                                        ])))
                            ])),
                        Scenario(
                            Token(
                                TriviaList([EnvironmentNewLine, EnvironmentNewLine]),
                                SyntaxKind.ScenarioKeyword,
                                "Scenario",
                                TriviaList()),
                            Token(
                                TriviaList(),
                                SyntaxKind.ColonToken,
                                TriviaList([Space])),
                            LiteralText(
                                Literal(
                                    TriviaList(),
                                    "also minimalistic",
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
                                                "the minimalism",
                                                TriviaList([EnvironmentNewLine]))
                                        ])))
                            ]))
                    ]),
                    rules: List([
                        Rule(
                            Token(
                                TriviaList([EnvironmentNewLine, Whitespace("  ")]),
                                SyntaxKind.RuleKeyword,
                                "Rule",
                                TriviaList()),
                            Token(
                                TriviaList(),
                                SyntaxKind.ColonToken,
                                TriviaList([Space])),
                            LiteralText(
                                Literal(
                                    TriviaList(),
                                    "My Rule",
                                    TriviaList([EnvironmentNewLine]))),
                            background: Background(
                                Token(
                                    TriviaList([EnvironmentNewLine, Whitespace("    ")]),
                                    SyntaxKind.BackgroundKeyword,
                                    "Background",
                                    TriviaList()),
                                Token(
                                    TriviaList(),
                                    SyntaxKind.ColonToken,
                                    TriviaList([Space])),
                                steps: List([
                                    Step(
                                        Token(
                                            TriviaList([Whitespace("      ")]),
                                            SyntaxKind.GivenKeyword,
                                            "Given",
                                            TriviaList([Space])),
                                        LiteralText(
                                            TokenList([
                                                Token(
                                                    TriviaList(),
                                                    SyntaxKind.LiteralToken,
                                                    "a rule background step",
                                                    TriviaList([EnvironmentNewLine]))
                                            ])))
                                ])
                            ),
                            scenarios: List([
                                Scenario(
                                    Token(
                                        TriviaList([EnvironmentNewLine, Whitespace("    ")]),
                                        SyntaxKind.ScenarioKeyword,
                                        "Scenario",
                                        TriviaList()),
                                    Token(
                                        TriviaList(),
                                        SyntaxKind.ColonToken,
                                        TriviaList([Space])),
                                    LiteralText(
                                        Literal(
                                            TriviaList(),
                                            "with examples",
                                            TriviaList([EnvironmentNewLine]))),
                                    steps: List([
                                        Step(
                                            Token(
                                                TriviaList([Whitespace("      ")]),
                                                SyntaxKind.GivenKeyword,
                                                "Given",
                                                TriviaList([Space])),
                                            LiteralText(
                                                TokenList([
                                                    Token(
                                                        TriviaList(),
                                                        SyntaxKind.LiteralToken,
                                                        "the <value> minimalism",
                                                        TriviaList([EnvironmentNewLine]))
                                                ])))
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
                                                            TriviaList()),
                                                        SeparatedList<PlainTextSyntax>([
                                                            LiteralText(
                                                                TableLiteral(
                                                                    TriviaList([Space]),
                                                                    "value",
                                                                    TriviaList([Space])))
                                                        ]),
                                                        Token(
                                                            TriviaList(),
                                                            SyntaxKind.VerticalBarToken,
                                                            TriviaList([EnvironmentNewLine]))),
                                                    TableRow(
                                                        Token(
                                                            TriviaList([Whitespace("      ")]),
                                                            SyntaxKind.VerticalBarToken,
                                                            TriviaList()),
                                                        SeparatedList<PlainTextSyntax>([
                                                            LiteralText(
                                                                TableLiteral(
                                                                    TriviaList([Space]),
                                                                    "1",
                                                                    TriviaList([Space])))
                                                            ]),
                                                        Token(
                                                            TriviaList(),
                                                            SyntaxKind.VerticalBarToken,
                                                            TriviaList([EnvironmentNewLine]))),
                                                    TableRow(
                                                        Token(
                                                            TriviaList([Whitespace("      ")]),
                                                            SyntaxKind.VerticalBarToken,
                                                            TriviaList()),
                                                        SeparatedList<PlainTextSyntax>([
                                                            LiteralText(
                                                                TableLiteral(
                                                                    TriviaList([Space]),
                                                                    "2",
                                                                    TriviaList([Space])))
                                                        ]),
                                                        Token(
                                                            TriviaList(),
                                                            SyntaxKind.VerticalBarToken,
                                                            TriviaList([EnvironmentNewLine])))
                                                ])))
                                    ]))
                            ]))
                    ])),
                Token(
                    TriviaList(),
                    SyntaxKind.EndOfFileToken,
                    TriviaList())));

        tree.ToString().Should().Be(source);
    }
}
