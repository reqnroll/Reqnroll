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
                            TriviaList([EnvironmentNewline]))),
                    description: LiteralText(
                        TokenList([
                            Literal(
                                TriviaList([Whitespace("  ")]),
                                "We want to ensure PickleStep all have different IDs",
                                TriviaList([EnvironmentNewline]))
                        ])
                    ),
                    background: Background(
                        Token(
                            TriviaList([EnvironmentNewline, Whitespace("  ")]),
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
                                TriviaList([EnvironmentNewline]))),
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
                                            TriviaList([EnvironmentNewline]))
                                    ])))
                        ])
                    ),
                    scenarios: List([
                        Scenario(
                            Token(
                                TriviaList([EnvironmentNewline, EnvironmentNewline]),
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
                                    TriviaList([EnvironmentNewline]))),
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
                                                TriviaList([EnvironmentNewline]))
                                        ])))
                            ])),
                        Scenario(
                            Token(
                                TriviaList([EnvironmentNewline, EnvironmentNewline]),
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
                                    TriviaList([EnvironmentNewline]))),
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
                                                TriviaList([EnvironmentNewline]))
                                        ])))
                            ]))
                    ]),
                    rules: List([
                        Rule(
                            Token(
                                TriviaList([EnvironmentNewline, Whitespace("  ")]),
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
                                    TriviaList([EnvironmentNewline]))),
                            background: Background(
                                Token(
                                    TriviaList([EnvironmentNewline, Whitespace("    ")]),
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
                                                    TriviaList([EnvironmentNewline]))
                                            ])))
                                ])
                            ),
                            scenarios: List([
                                Scenario(
                                    Token(
                                        TriviaList([EnvironmentNewline, Whitespace("    ")]),
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
                                            TriviaList([EnvironmentNewline]))),
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
                                                        TriviaList([EnvironmentNewline]))
                                                ])))
                                    ]),
                                    examples: List([
                                        Examples(
                                            Token(
                                                TriviaList([EnvironmentNewline, Whitespace("    ")]),
                                                SyntaxKind.ExamplesKeyword,
                                                "Examples",
                                                TriviaList()),
                                            Token(
                                                TriviaList(),
                                                SyntaxKind.ColonToken,
                                                TriviaList([EnvironmentNewline])),
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
                                                            TriviaList([EnvironmentNewline]))),
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
                                                            TriviaList([EnvironmentNewline]))),
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
                                                            TriviaList([EnvironmentNewline])))
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
