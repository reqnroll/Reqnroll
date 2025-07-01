using Reqnroll.CodeAnalysis.Gherkin.Syntax;

namespace Reqnroll.CodeAnalysis.Gherkin.Parsing;

using static SyntaxFactory;

public partial class TagsTests
{
    [Fact]
    public void SeveralExamplesIsRepresentedInTree()
    {
        // Taken from good/several_examples.feature
        const string source =
            """
            Feature: Tagged Examples

              Scenario Outline: minimalistic
                Given the <what>

                @foo
                Examples:
                  | what |
                  | foo  |

                @bar
                Examples:
                  | what |
                  | bar  |

              @zap
              Scenario: ha ok

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
                            "Tagged Examples",
                            TriviaList([EnvironmentNewline, EnvironmentNewline]))),
                    scenarios: List([
                        Scenario(
                            Token(
                                TriviaList([Whitespace("  ")]),
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
                                    "minimalistic",
                                    TriviaList([EnvironmentNewline]))),
                            steps: List([
                                Step(
                                    Token(
                                        TriviaList([Whitespace("    ")]),
                                        SyntaxKind.GivenKeyword,
                                        "Given",
                                        TriviaList([Space])),
                                    InterpolatedText(
                                        List<InterpolatedTextContentSyntax>([
                                            InterpolatedTextLiteral(
                                                Literal(
                                                    TriviaList(),
                                                    "the ",
                                                    TriviaList())),
                                            Interpolation(
                                                Token(
                                                    TriviaList(),
                                                    SyntaxKind.LessThanToken,
                                                    TriviaList()),
                                                Identifier(
                                                    TriviaList(),
                                                    "what",
                                                    TriviaList()),
                                                Token(
                                                    TriviaList(),
                                                    SyntaxKind.GreaterThanToken,
                                                    TriviaList([EnvironmentNewline])))
                                        ])))
                            ]),
                            examples: List([
                                Examples(
                                    List([
                                        Tag(
                                            Token(TriviaList([EnvironmentNewline, Whitespace("    ")]), SyntaxKind.AtToken, TriviaList()),
                                            Literal(TriviaList(), "foo", TriviaList([EnvironmentNewline])))
                                    ]),
                                    Token(
                                        TriviaList(),
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
                                                            "what",
                                                            TriviaList([Space]))
                                                    )
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
                                                            "foo",
                                                            TriviaList([Whitespace("  ")]))
                                                    )
                                                ]),
                                                Token(
                                                    TriviaList(),
                                                    SyntaxKind.VerticalBarToken,
                                                    TriviaList([EnvironmentNewline])))
                                        ]))
                                ),
                                Examples(
                                    List([
                                        Tag(
                                            Token(TriviaList([EnvironmentNewline, Whitespace("    ")]), SyntaxKind.AtToken, TriviaList()),
                                            Literal(TriviaList(), "bar", TriviaList([EnvironmentNewline])))
                                    ]),
                                    Token(
                                        TriviaList(),
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
                                                            "what",
                                                            TriviaList([Space]))
                                                    )
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
                                                            "bar",
                                                            TriviaList([Whitespace("  ")]))
                                                    )
                                                ]),
                                                Token(
                                                    TriviaList(),
                                                    SyntaxKind.VerticalBarToken,
                                                    TriviaList([EnvironmentNewline])))
                                        ]))
                                )
                            ])
                        ),
                        Scenario(
                            List([
                                Tag(
                                    Token(TriviaList([EnvironmentNewline, Whitespace("  ")]), SyntaxKind.AtToken, TriviaList()),
                                    Literal(TriviaList(), "zap", TriviaList([EnvironmentNewline])))
                            ]),
                            Token(
                                TriviaList(),
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
                                    "ha ok",
                                    TriviaList([EnvironmentNewline]))),
                            steps: List<StepSyntax>())
                    ])),
                Token(
                    TriviaList(),
                    SyntaxKind.EndOfFileToken,
                    TriviaList())));

        tree.ToString().Should().Be(source);
    }
}
