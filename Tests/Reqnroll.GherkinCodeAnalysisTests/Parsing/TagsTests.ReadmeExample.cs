using Reqnroll.CodeAnalysis.Gherkin.Syntax;

namespace Reqnroll.CodeAnalysis.Gherkin.Parsing;

using static SyntaxFactory;

public partial class TagsTests
{
    [Fact]
    public void ReadmeExampleTagsAreRepresentedInTree()
    {
        // Taken from good/readme_example.feature
        const string source =
            """
            @a
            Feature:
              @b @c
              Scenario Outline:
                Given <x>

                Examples:
                  | x |
                  | y |

              @d @e
              Scenario Outline:
                Given <m>

                @f
                Examples:
                  | m |
                  | n |

            """;

        var tree = GherkinSyntaxTree.ParseText(source);

        tree.GetRoot().Should().BeEquivalentTo(
            GherkinDocument(
                Feature(
                    tags: List([
                        Tag(
                            Token(TriviaList(), SyntaxKind.AtToken, TriviaList()),
                            Literal(TriviaList(), "a", TriviaList([EnvironmentNewline])))
                        ]),
                    featureKeyword: Token(
                        TriviaList(),
                        SyntaxKind.FeatureKeyword,
                        "Feature",
                        TriviaList()),
                    colonToken: Token(
                        TriviaList(),
                        SyntaxKind.ColonToken,
                        TriviaList([EnvironmentNewline])),
                    scenarios: List([
                        Scenario(
                            List([
                                Tag(
                                    Token(TriviaList([Whitespace("  ")]), SyntaxKind.AtToken, TriviaList()),
                                    Literal(TriviaList(), "b", TriviaList([Space]))),
                                Tag(
                                    Token(TriviaList(), SyntaxKind.AtToken, TriviaList()),
                                    Literal(TriviaList(), "c", TriviaList([EnvironmentNewline])))
                            ]),
                            Token(
                                TriviaList([Whitespace("  ")]),
                                SyntaxKind.ScenarioKeyword,
                                "Scenario Outline",
                                TriviaList()),
                            Token(
                                TriviaList(),
                                SyntaxKind.ColonToken,
                                TriviaList([EnvironmentNewline])),
                            steps: List([
                                Step(
                                    Token(
                                        TriviaList([Whitespace("    ")]),
                                        SyntaxKind.GivenKeyword,
                                        "Given",
                                        TriviaList([Space])),
                                    InterpolatedText(
                                        List<InterpolatedTextContentSyntax>([
                                            Interpolation(
                                                Token(
                                                    TriviaList(),
                                                    SyntaxKind.LessThanToken,
                                                    TriviaList()),
                                                Identifier(
                                                    TriviaList(),
                                                    "x",
                                                    TriviaList()),
                                                Token(
                                                    TriviaList(),
                                                    SyntaxKind.GreaterThanToken,
                                                    TriviaList([EnvironmentNewline])))
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
                                                            "x",
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
                                                            "y",
                                                            TriviaList([Space]))
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
                            Token(
                                TriviaList([Whitespace("  ")]),
                                SyntaxKind.ScenarioKeyword,
                                "Scenario Outline",
                                TriviaList()),
                            Token(
                                TriviaList(),
                                SyntaxKind.ColonToken,
                                TriviaList([EnvironmentNewline])),
                            steps: List([
                                Step(
                                    Token(
                                        TriviaList([Whitespace("    ")]),
                                        SyntaxKind.GivenKeyword,
                                        "Given",
                                        TriviaList([Space])),
                                    InterpolatedText(
                                        List<InterpolatedTextContentSyntax>([
                                            Interpolation(
                                                Token(
                                                    TriviaList(),
                                                    SyntaxKind.LessThanToken,
                                                    TriviaList()),
                                                Identifier(
                                                    TriviaList(),
                                                    "m",
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
                                            Token(TriviaList([EnvironmentNewline, Whitespace("  ")]), SyntaxKind.AtToken, TriviaList()),
                                            Literal(TriviaList(), "f", TriviaList([EnvironmentNewline])))
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
                                                            "m",
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
                                                            "n",
                                                            TriviaList([Space]))
                                                    )
                                                ]),
                                                Token(
                                                    TriviaList(),
                                                    SyntaxKind.VerticalBarToken,
                                                    TriviaList([EnvironmentNewline])))
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
