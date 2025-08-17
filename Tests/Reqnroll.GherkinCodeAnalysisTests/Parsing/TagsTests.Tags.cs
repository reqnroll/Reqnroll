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
                            Literal(TriviaList(), "feature_tag2", TriviaList([EnvironmentNewLine]))),
                        Tag(
                            Token(TriviaList([Whitespace("  ")]), SyntaxKind.AtToken, TriviaList()),
                            Literal(TriviaList(), "feature_tag3", TriviaList([EnvironmentNewLine])))
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
                            "Minimal Example Outline",
                            TriviaList([EnvironmentNewLine]))),
                    examples: List([
                        Example(
                            List([
                                Tag(
                                    Token(TriviaList([EnvironmentNewLine]), SyntaxKind.AtToken, TriviaList()),
                                    Literal(TriviaList(), "scenario_tag1", TriviaList([Space]))),
                                Tag(
                                    Token(TriviaList(), SyntaxKind.AtToken, TriviaList()),
                                    Literal(TriviaList(), "scenario_tag2", TriviaList([EnvironmentNewLine]))),
                                Tag(
                                    Token(TriviaList([Whitespace("  ")]), SyntaxKind.AtToken, TriviaList()),
                                    Literal(TriviaList(), "scenario_tag3", TriviaList([EnvironmentNewLine])))
                            ]),
                            Token(
                                TriviaList(),
                                SyntaxKind.ExampleKeyword,
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
                                        TriviaList([Whitespace("    ")]),
                                        SyntaxKind.ContextStepKeyword,
                                        "Given",
                                        TriviaList([Space])),
                                    LiteralText(
                                        Literal(
                                            TriviaList(),
                                            "the minimalism",
                                            TriviaList([EnvironmentNewLine]))))
                            ])),
                        Example(
                            List([
                                Tag(
                                    Token(TriviaList(), SyntaxKind.AtToken, TriviaList()),
                                    Literal(TriviaList(), "so_tag1", TriviaList([Space, Space]))),
                                Tag(
                                    Token(TriviaList(), SyntaxKind.AtToken, TriviaList()),
                                    Literal(TriviaList(), "so_tag2", TriviaList([Space, Space, EnvironmentNewLine]))),
                                Tag(
                                    Token(TriviaList([Whitespace("  ")]), SyntaxKind.AtToken, TriviaList()),
                                    Literal(TriviaList(), "so_tag3", TriviaList([EnvironmentNewLine])))
                            ]),
                            Token(
                                TriviaList(),
                                SyntaxKind.ExampleKeyword,
                                "Example Outline",
                                TriviaList()),
                            Token(
                                TriviaList(),
                                SyntaxKind.ColonToken,
                                TriviaList([Space])),
                            LiteralText(
                                Literal(
                                    TriviaList(),
                                    "minimalistic outline",
                                    TriviaList([EnvironmentNewLine]))),
                            steps: List([
                                Step(
                                    Token(
                                        TriviaList([Whitespace("    ")]),
                                        SyntaxKind.ContextStepKeyword,
                                        "Given",
                                        TriviaList([Space])),
                                    LiteralText(
                                        TokenList([
                                            Literal(
                                                TriviaList(),
                                                "the <what>",
                                                TriviaList([EnvironmentNewLine]))
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
                                            Literal(TriviaList(), "ex_tag2", TriviaList([EnvironmentNewLine]))),
                                        Tag(
                                            Token(TriviaList([Whitespace("  ")]), SyntaxKind.AtToken, TriviaList()),
                                            Literal(TriviaList(), "ex_tag3", TriviaList([EnvironmentNewLine])))
                                    ]),
                                    Token(
                                        TriviaList(),
                                        SyntaxKind.ExamplesKeyword,
                                        "Examples",
                                        TriviaList([Space])),
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
                                                    TextTableCell(
                                                        TableLiteral(
                                                            TriviaList(),
                                                            "what",
                                                            TriviaList()))
                                                ]),
                                                Token(
                                                    TriviaList([Whitespace("       ")]),
                                                    SyntaxKind.VerticalBarToken,
                                                    TriviaList([EnvironmentNewLine]))),
                                            TableRow(
                                                Token(
                                                    TriviaList([Whitespace("    ")]),
                                                    SyntaxKind.VerticalBarToken,
                                                    TriviaList([Space])),
                                                TableCellList([
                                                    TextTableCell(
                                                        TableLiteral(
                                                            TriviaList(),
                                                            "minimalism",
                                                            TriviaList()))
                                                ]),
                                                Token(
                                                    TriviaList([Space]),
                                                    SyntaxKind.VerticalBarToken,
                                                    TriviaList([EnvironmentNewLine])))
                                        ]))
                                ),
                                Examples(
                                    List([
                                        Tag(
                                            Token(TriviaList(), SyntaxKind.AtToken, TriviaList()),
                                            Literal(TriviaList(), "ex_tag4", TriviaList([Space]))),
                                        Tag(
                                            Token(TriviaList(), SyntaxKind.AtToken, TriviaList()),
                                            Literal(TriviaList(), "ex_tag5", TriviaList([EnvironmentNewLine]))),
                                        Tag(
                                            Token(TriviaList([Whitespace("  ")]), SyntaxKind.AtToken, TriviaList()),
                                            Literal(TriviaList(), "ex_tag6", TriviaList([EnvironmentNewLine])))
                                    ]),
                                    Token(
                                        TriviaList(),
                                        SyntaxKind.ExamplesKeyword,
                                        "Examples",
                                        TriviaList([Space])),
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
                                                    TextTableCell(
                                                        TableLiteral(
                                                            TriviaList(),
                                                            "what",
                                                            TriviaList()))
                                                ]),
                                                Token(
                                                    TriviaList([Whitespace("          ")]),
                                                    SyntaxKind.VerticalBarToken,
                                                    TriviaList([EnvironmentNewLine]))),
                                            TableRow(
                                                Token(
                                                    TriviaList([Whitespace("    ")]),
                                                    SyntaxKind.VerticalBarToken,
                                                    TriviaList([Space])),
                                                TableCellList([
                                                    TextTableCell(
                                                        TableLiteral(
                                                            TriviaList(),
                                                            "more minimalism",
                                                            TriviaList()))
                                                ]),
                                                Token(
                                                    TriviaList([Space]),
                                                    SyntaxKind.VerticalBarToken,
                                                    TriviaList([EnvironmentNewLine])))
                                        ])))
                            ])),
                        Example(
                            List([
                                Tag(
                                    Token(TriviaList(), SyntaxKind.AtToken, TriviaList()),
                                    Literal(TriviaList(), "comment_tag1", TriviaList([Space])))
                            ]),
                            Token(
                                TriviaList([Comment("#a comment"), EnvironmentNewLine]),
                                SyntaxKind.ExampleKeyword,
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
                                    TriviaList([EnvironmentNewLine]))),
                            steps: List([
                                Step(
                                    Token(
                                        TriviaList([Whitespace("    ")]),
                                        SyntaxKind.ContextStepKeyword,
                                        "Given",
                                        TriviaList([Space])),
                                    LiteralText(
                                        Literal(
                                            TriviaList(),
                                            "a comment",
                                            TriviaList([EnvironmentNewLine]))))
                            ])),
                        Example(
                            List([
                                Tag(
                                    Token(TriviaList(), SyntaxKind.AtToken, TriviaList()),
                                    Literal(TriviaList(), "comment_tag#2", TriviaList([Space])))
                            ]),
                            Token(
                                TriviaList([Comment("#a comment"), EnvironmentNewLine]),
                                SyntaxKind.ExampleKeyword,
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
                                    TriviaList([EnvironmentNewLine]))),
                            steps: List([
                                Step(
                                    Token(
                                        TriviaList([Whitespace("    ")]),
                                        SyntaxKind.ContextStepKeyword,
                                        "Given",
                                        TriviaList([Space])),
                                    LiteralText(
                                        Literal(
                                            TriviaList(),
                                            "a comment is preceded by a space",
                                            TriviaList([EnvironmentNewLine]))))
                            ]))
                    ]),
                    rules: List([
                        Rule(
                            List([
                                Tag(
                                    Token(TriviaList(), SyntaxKind.AtToken, TriviaList()),
                                    Literal(TriviaList(), "rule_tag", TriviaList([EnvironmentNewLine])))
                            ]),
                            Token(
                                TriviaList(),
                                SyntaxKind.RuleKeyword,
                                "Rule",
                                TriviaList([EnvironmentNewLine])),
                            Token(
                                TriviaList(),
                                SyntaxKind.ColonToken,
                                TriviaList([EnvironmentNewLine])),
                            examples: List([
                                Example(
                                    List([
                                        Tag(
                                            Token(TriviaList(), SyntaxKind.AtToken, TriviaList()),
                                            Literal(TriviaList(), "joined_tag3@joined_tag4", TriviaList([EnvironmentNewLine])))
                                    ]),
                                    Token(
                                        TriviaList(),
                                        SyntaxKind.ExampleKeyword,
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
                                            TriviaList([EnvironmentNewLine]))),
                                    steps: List([
                                        Step(
                                            Token(
                                                TriviaList([Whitespace("    ")]),
                                                SyntaxKind.ContextStepKeyword,
                                                "Given",
                                                TriviaList([Space])),
                                            LiteralText(
                                                Literal(
                                                    TriviaList(),
                                                    "the @delimits tags",
                                                    TriviaList([EnvironmentNewLine]))))
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
