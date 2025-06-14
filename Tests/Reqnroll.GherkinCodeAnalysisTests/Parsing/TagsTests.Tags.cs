using Reqnroll.CodeAnalysis.Gherkin.Syntax;

namespace Reqnroll.CodeAnalysis.Gherkin.Parsing;

using static SyntaxFactory;

public partial class TagsTests
{
    [Fact]
    public void TagsFeatureIsRepresentedInTree()
    {
        // Taken from good/tags.feature
        const string source =
            """
            @feature_tag1 @feature_tag2
              @feature_tag3
            Feature: Minimal Scenario Outline

            @scenario_tag1 @scenario_tag2
              @scenario_tag3
            Scenario: minimalistic
                Given the minimalism

            @so_tag1  @so_tag2  
              @so_tag3
            Scenario Outline: minimalistic outline
                Given the <what>

            @ex_tag1 @ex_tag2
              @ex_tag3
            Examples: 
              | what       |
              | minimalism |

            @ex_tag4 @ex_tag5
              @ex_tag6
            Examples: 
              | what            |
              | more minimalism |

            @comment_tag1 #a comment
            Scenario: comments
              Given a comment

            @comment_tag#2 #a comment
            Scenario: hash in tags
              Given a comment is preceded by a space

            @rule_tag
            Rule:

            @joined_tag3@joined_tag4
            Scenario: joined tags
              Given the @delimits tags

            """;

        var tree = GherkinSyntaxTree.ParseText(source);

        tree.GetRoot().Should().BeEquivalentTo(
            GherkinDocument(
                Feature(
                    List([
                        Tag(
                            Token(TriviaList(), SyntaxKind.AtToken, TriviaList()),
                            Literal(TriviaList(), "feature_tag1", TriviaList([Space]))),
                        Tag(
                            Token(TriviaList(), SyntaxKind.AtToken, TriviaList()),
                            Literal(TriviaList(), "feature_tag2", TriviaList([EnvironmentNewline]))),
                        Tag(
                            Token(TriviaList([Whitespace("  ")]), SyntaxKind.AtToken, TriviaList()),
                            Literal(TriviaList(), "feature_tag3", TriviaList([EnvironmentNewline])))
                    ]),
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
                            "Minimal Scenario Outline",
                            TriviaList([EnvironmentNewline]))),
                    scenarios: List([
                        Scenario(
                            List([
                                Tag(
                                    Token(TriviaList([EnvironmentNewline]), SyntaxKind.AtToken, TriviaList()),
                                    Literal(TriviaList(), "scenario_tag1", TriviaList([Space]))),
                                Tag(
                                    Token(TriviaList(), SyntaxKind.AtToken, TriviaList()),
                                    Literal(TriviaList(), "scenario_tag2", TriviaList([EnvironmentNewline]))),
                                Tag(
                                    Token(TriviaList([Whitespace("  ")]), SyntaxKind.AtToken, TriviaList()),
                                    Literal(TriviaList(), "scenario_tag3", TriviaList([EnvironmentNewline])))
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
                                    "minimalistic",
                                    TriviaList([EnvironmentNewline]))),
                            steps: List([
                                Step(
                                    Token(
                                        TriviaList([Whitespace("    ")]),
                                        SyntaxKind.GivenKeyword,
                                        "Given",
                                        TriviaList([Space])),
                                    LiteralText(
                                        Literal(
                                            TriviaList(),
                                            "the minimalism",
                                            TriviaList([EnvironmentNewline]))))
                            ])),
                        Scenario(
                            List([
                                Tag(
                                    Token(TriviaList(), SyntaxKind.AtToken, TriviaList()),
                                    Literal(TriviaList(), "so_tag1", TriviaList([Space, Space]))),
                                Tag(
                                    Token(TriviaList(), SyntaxKind.AtToken, TriviaList()),
                                    Literal(TriviaList(), "so_tag2", TriviaList([Space, Space, EnvironmentNewline]))),
                                Tag(
                                    Token(TriviaList([Whitespace("  ")]), SyntaxKind.AtToken, TriviaList()),
                                    Literal(TriviaList(), "so_tag3", TriviaList([EnvironmentNewline])))
                            ]),
                            Token(
                                TriviaList(),
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
                                    "minimalistic outline",
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
                                            Token(TriviaList(), SyntaxKind.AtToken, TriviaList()),
                                            Literal(TriviaList(), "ex_tag1", TriviaList([Space]))),
                                        Tag(
                                            Token(TriviaList(), SyntaxKind.AtToken, TriviaList()),
                                            Literal(TriviaList(), "ex_tag2", TriviaList([EnvironmentNewline]))),
                                        Tag(
                                            Token(TriviaList([Whitespace("  ")]), SyntaxKind.AtToken, TriviaList()),
                                            Literal(TriviaList(), "ex_tag3", TriviaList([EnvironmentNewline])))
                                    ]),
                                    Token(
                                        TriviaList(),
                                        SyntaxKind.ExamplesKeyword,
                                        "Examples",
                                        TriviaList([Space])),
                                    Token(
                                        TriviaList(),
                                        SyntaxKind.ColonToken,
                                        TriviaList([EnvironmentNewline])),
                                    table: Table(
                                        List([
                                            TableRow(
                                                Token(
                                                    TriviaList([Whitespace("    ")]),
                                                    SyntaxKind.VerticalBarToken,
                                                    TriviaList([Space])),
                                                SeparatedList<PlainTextSyntax>([
                                                    LiteralText(
                                                        TableLiteral(
                                                            TriviaList(),
                                                            "what",
                                                            TriviaList([Space, Space, Space, Space, Space, Space, Space]))
                                                    )
                                                ]),
                                                Token(
                                                    TriviaList(),
                                                    SyntaxKind.VerticalBarToken,
                                                    TriviaList([EnvironmentNewline]))),
                                            TableRow(
                                                Token(
                                                    TriviaList([Whitespace("    ")]),
                                                    SyntaxKind.VerticalBarToken,
                                                    TriviaList([Space])),
                                                SeparatedList<PlainTextSyntax>([
                                                    LiteralText(
                                                        TableLiteral(
                                                            TriviaList(),
                                                            "minimalism",
                                                            TriviaList([Space]))
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
                                            Token(TriviaList(), SyntaxKind.AtToken, TriviaList()),
                                            Literal(TriviaList(), "ex_tag4", TriviaList([Space]))),
                                        Tag(
                                            Token(TriviaList(), SyntaxKind.AtToken, TriviaList()),
                                            Literal(TriviaList(), "ex_tag5", TriviaList([EnvironmentNewline]))),
                                        Tag(
                                            Token(TriviaList([Whitespace("  ")]), SyntaxKind.AtToken, TriviaList()),
                                            Literal(TriviaList(), "ex_tag6", TriviaList([EnvironmentNewline])))
                                    ]),
                                    Token(
                                        TriviaList(),
                                        SyntaxKind.ExamplesKeyword,
                                        "Examples",
                                        TriviaList([Space])),
                                    Token(
                                        TriviaList(),
                                        SyntaxKind.ColonToken,
                                        TriviaList([EnvironmentNewline])),
                                    table: Table(
                                        List([
                                            TableRow(
                                                Token(
                                                    TriviaList([Whitespace("    ")]),
                                                    SyntaxKind.VerticalBarToken,
                                                    TriviaList([Space])),
                                                SeparatedList<PlainTextSyntax>([
                                                    LiteralText(
                                                        TableLiteral(
                                                            TriviaList(),
                                                            "what",
                                                            TriviaList([Space, Space, Space, Space, Space, Space, Space, Space, Space, Space, Space]))
                                                    )
                                                ]),
                                                Token(
                                                    TriviaList(),
                                                    SyntaxKind.VerticalBarToken,
                                                    TriviaList([EnvironmentNewline]))),
                                            TableRow(
                                                Token(
                                                    TriviaList([Whitespace("    ")]),
                                                    SyntaxKind.VerticalBarToken,
                                                    TriviaList([Space])),
                                                SeparatedList<PlainTextSyntax>([
                                                    LiteralText(
                                                        TableLiteral(
                                                            TriviaList(),
                                                            "more minimalism",
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
                            List([
                                Tag(
                                    Token(TriviaList(), SyntaxKind.AtToken, TriviaList()),
                                    Literal(TriviaList(), "comment_tag1", TriviaList([Space])))
                            ]),
                            Token(
                                TriviaList([Comment("#a comment"), EnvironmentNewline]),
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
                                    "comments",
                                    TriviaList([EnvironmentNewline]))),
                            steps: List([
                                Step(
                                    Token(
                                        TriviaList([Whitespace("    ")]),
                                        SyntaxKind.GivenKeyword,
                                        "Given",
                                        TriviaList([Space])),
                                    LiteralText(
                                        Literal(
                                            TriviaList(),
                                            "a comment",
                                            TriviaList([EnvironmentNewline]))))
                            ])),
                        Scenario(
                            List([
                                Tag(
                                    Token(TriviaList(), SyntaxKind.AtToken, TriviaList()),
                                    Literal(TriviaList(), "comment_tag#2", TriviaList([Space])))
                            ]),
                            Token(
                                TriviaList([Comment("#a comment"), EnvironmentNewline]),
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
                                    "hash in tags",
                                    TriviaList([EnvironmentNewline]))),
                            steps: List([
                                Step(
                                    Token(
                                        TriviaList([Whitespace("    ")]),
                                        SyntaxKind.GivenKeyword,
                                        "Given",
                                        TriviaList([Space])),
                                    LiteralText(
                                        Literal(
                                            TriviaList(),
                                            "a comment is preceded by a space",
                                            TriviaList([EnvironmentNewline]))))
                            ]))
                    ]),
                    rules: List([
                        Rule(
                            List([
                                Tag(
                                    Token(TriviaList(), SyntaxKind.AtToken, TriviaList()),
                                    Literal(TriviaList(), "rule_tag", TriviaList([EnvironmentNewline])))
                            ]),
                            Token(
                                TriviaList(),
                                SyntaxKind.RuleKeyword,
                                "Rule",
                                TriviaList([EnvironmentNewline])),
                            Token(
                                TriviaList(),
                                SyntaxKind.ColonToken,
                                TriviaList([EnvironmentNewline])),
                            scenarios: List([
                                Scenario(
                                    List([
                                        Tag(
                                            Token(TriviaList(), SyntaxKind.AtToken, TriviaList()),
                                            Literal(TriviaList(), "joined_tag3@joined_tag4", TriviaList([EnvironmentNewline])))
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
                                            "joined tags",
                                            TriviaList([EnvironmentNewline]))),
                                    steps: List([
                                        Step(
                                            Token(
                                                TriviaList([Whitespace("    ")]),
                                                SyntaxKind.GivenKeyword,
                                                "Given",
                                                TriviaList([Space])),
                                            LiteralText(
                                                Literal(
                                                    TriviaList(),
                                                    "the @delimits tags",
                                                    TriviaList([EnvironmentNewline]))))
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
