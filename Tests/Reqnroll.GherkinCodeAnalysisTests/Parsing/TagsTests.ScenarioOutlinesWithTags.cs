using Reqnroll.CodeAnalysis.Gherkin.Syntax;

namespace Reqnroll.CodeAnalysis.Gherkin.Parsing;

using static SyntaxFactory;

public partial class TagsTests
{
    [Fact]
    public void ScenarioOutlinesWithTagsIsRepresentedInTree()
    {
        // Taken from good/scenario_outlines_with_tags.feature
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
                    List([
                        Tag(
                            Token(TriviaList(), SyntaxKind.AtToken, TriviaList()),
                            Literal(TriviaList(), "a", TriviaList([EnvironmentNewLine])))
                    ]),
                    Token(
                        TriviaList(),
                        SyntaxKind.FeatureKeyword,
                        "Feature",
                        TriviaList()),
                    Token(
                        TriviaList(),
                        SyntaxKind.ColonToken,
                        TriviaList([EnvironmentNewLine])),
                    scenarios: List([
                        Scenario(
                            List([
                                Tag(
                                    Token(TriviaList([Whitespace("  ")]), SyntaxKind.AtToken, TriviaList()),
                                    Literal(TriviaList(), "b", TriviaList([Space]))),
                                Tag(
                                    Token(TriviaList(), SyntaxKind.AtToken, TriviaList()),
                                    Literal(TriviaList(), "c", TriviaList([EnvironmentNewLine])))
                            ]),
                            Token(
                                TriviaList(),
                                SyntaxKind.ScenarioKeyword,
                                "Scenario Outline",
                                TriviaList()),
                            Token(
                                TriviaList(),
                                SyntaxKind.ColonToken,
                                TriviaList([EnvironmentNewLine])),
                            steps: List([
                                Step(
                                    Token(
                                        TriviaList([Whitespace("    ")]),
                                        SyntaxKind.GivenKeyword,
                                        "Given",
                                        TriviaList([Space])),
                                    LiteralText(
                                        TokenList([
                                            Literal(
                                                TriviaList(),
                                                "<x>",
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
                                                            "x",
                                                            TriviaList([Space]))
                                                    )
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
                                                            "y",
                                                            TriviaList([Space]))
                                                    )
                                                ]),
                                                Token(
                                                    TriviaList(),
                                                    SyntaxKind.VerticalBarToken,
                                                    TriviaList([EnvironmentNewLine])))
                                        ]))
                                )
                            ])
                        ),
                        Scenario(
                            List([
                                Tag(
                                    Token(TriviaList([EnvironmentNewLine, Whitespace("  ")]), SyntaxKind.AtToken, TriviaList()),
                                    Literal(TriviaList(), "d", TriviaList([Space]))),
                                Tag(
                                    Token(TriviaList(), SyntaxKind.AtToken, TriviaList()),
                                    Literal(TriviaList(), "e", TriviaList([EnvironmentNewLine])))
                            ]),
                            Token(
                                TriviaList(),
                                SyntaxKind.ScenarioKeyword,
                                "Scenario Outline",
                                TriviaList()),
                            Token(
                                TriviaList(),
                                SyntaxKind.ColonToken,
                                TriviaList([EnvironmentNewLine])),
                            steps: List([
                                Step(
                                    Token(
                                        TriviaList([Whitespace("    ")]),
                                        SyntaxKind.GivenKeyword,
                                        "Given",
                                        TriviaList([Space])),
                                    LiteralText(
                                        TokenList([
                                            Literal(
                                                TriviaList(),
                                                "<m>",
                                                TriviaList([EnvironmentNewLine]))
                                        ])))
                            ]),
                            examples: List([
                                Examples(
                                    List([
                                        Tag(
                                            Token(TriviaList([EnvironmentNewLine, Whitespace("    ")]), SyntaxKind.AtToken, TriviaList()),
                                            Literal(TriviaList(), "f", TriviaList([EnvironmentNewLine])))
                                    ]),
                                    Token(
                                        TriviaList([Whitespace("    ")]),
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
                                                            "m",
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
                                                    TriviaList([Space])),
                                                SeparatedList<PlainTextSyntax>([
                                                    LiteralText(
                                                        TableLiteral(
                                                            TriviaList(),
                                                            "n",
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
