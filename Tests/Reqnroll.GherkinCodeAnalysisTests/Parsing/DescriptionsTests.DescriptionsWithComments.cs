using Reqnroll.CodeAnalysis.Gherkin.Syntax;

namespace Reqnroll.CodeAnalysis.Gherkin.Parsing;

using static SyntaxFactory;

public partial class DescriptionsTests
{
    [Fact]
    public void DescriptionsWithCommentsAreRepresentedInTree()
    {
        // Taken from good/descriptions_with_comments.feature
        const string source =
            """
            Feature: Descriptions with comments everywhere
              This is a description
              # comment
              with a comment in the middle and at the end
              # comment 2

              Scenario: two lines
              This description
              # comment
              # comment 2
              has two lines and two comments in the middle and is indented with two spaces
                Given the minimalism

            Scenario: without indentation
            This is a description without indentation
            # comment
            and a comment in the middle and at the end
            # comment 2

              Given the minimalism

              Scenario: empty lines in the middle
              This description
              # comment

              has an empty line and a comment in the middle
                Given the minimalism

              Scenario: empty lines around

              # comment
              This description
              has an empty lines around
              # comment

                Given the minimalism

              Scenario Outline: scenario outline with a description
            # comment
            This is a scenario outline description with comments
            # comment 2
            in the middle and before and at the end
            # comment 3
                Given the minimalism

              Examples: examples with description
            # comment
            This is an examples description
            # comment
            with a comment in the middle
            # comment

                | foo |
                | bar |

              Scenario: scenario with just a comment
                # comment
                Given the minimalism

              Scenario: scenario with a comment with new lines around

                # comment

                Given the minimalism  
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
                            "Descriptions with comments everywhere",
                            TriviaList([EnvironmentNewline]))),
                    description: LiteralText(
                        TokenList([
                            Literal(
                                TriviaList([Whitespace("  ")]),
                                "This is a description",
                                TriviaList([EnvironmentNewline, Whitespace("  "), Comment("# comment"), EnvironmentNewline])),
                            Literal(
                                TriviaList([Whitespace("  ")]),
                                "with a comment in the middle and at the end",
                                TriviaList([EnvironmentNewline, Whitespace("  "), Comment("# comment 2"), EnvironmentNewline, EnvironmentNewline]))
                        ])),
                    scenarios: List([
                        Scenario(
                            Token(
                                TriviaList([Whitespace("  ")]),
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
                                    "two lines",
                                    TriviaList([EnvironmentNewline]))),
                            description: LiteralText(
                                TokenList([
                                    Literal(
                                        TriviaList([Whitespace("  ")]),
                                        "This description",
                                        TriviaList([EnvironmentNewline, Whitespace("  "), Comment("# comment"), EnvironmentNewline, Whitespace("  "), Comment("# comment 2"), EnvironmentNewline])),
                                    Literal(
                                        TriviaList([Whitespace("  ")]),
                                        "has two lines and two comments in the middle and is indented with two spaces",
                                        TriviaList([EnvironmentNewline]))
                                ])),
                            List([
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
                                                "the minimalism",
                                                TriviaList([EnvironmentNewline]))
                                        ])))
                            ])),
                        Scenario(
                            Token(
                                TriviaList([EnvironmentNewline]),
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
                                    "without indentation",
                                    TriviaList([EnvironmentNewline]))),
                            description: LiteralText(
                                TokenList([
                                    Literal(
                                        TriviaList(),
                                        "This is a description without indentation",
                                        TriviaList([EnvironmentNewline, Comment("# comment"), EnvironmentNewline])),
                                    Literal(
                                        TriviaList(),
                                        "and a comment in the middle and at the end",
                                        TriviaList([EnvironmentNewline, Comment("# comment 2"), EnvironmentNewline, EnvironmentNewline]))
                                ])),
                            List([
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
                                TriviaList([EnvironmentNewline, Whitespace("  ")]),
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
                                    "empty lines in the middle",
                                    TriviaList([EnvironmentNewline]))),
                            description: LiteralText(
                                TokenList([
                                    Literal(
                                        TriviaList([Whitespace("  ")]),
                                        "This description",
                                        TriviaList([EnvironmentNewline, Whitespace("  "), Comment("# comment"), EnvironmentNewline, EnvironmentNewline])),
                                    Literal(
                                        TriviaList([Whitespace("  ")]),
                                        "has an empty line and a comment in the middle",
                                        TriviaList([EnvironmentNewline]))
                                ])),
                            List([
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
                                                "the minimalism",
                                                TriviaList([EnvironmentNewline]))
                                        ])))
                            ])),
                        Scenario(
                            Token(
                                TriviaList([EnvironmentNewline, Whitespace("  ")]),
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
                                    "empty lines around",
                                    TriviaList([EnvironmentNewline]))),
                            description: LiteralText(
                                TokenList([
                                    Literal(
                                        TriviaList([EnvironmentNewline, Whitespace("  "), Comment("# comment"), EnvironmentNewline]),
                                        "This description",
                                        TriviaList([EnvironmentNewline])),
                                    Literal(
                                        TriviaList([Whitespace("  ")]),
                                        "has an empty lines around",
                                        TriviaList([EnvironmentNewline, Whitespace("  "), Comment("# comment"), EnvironmentNewline, EnvironmentNewline]))
                                ])),
                            List([
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
                                                "the minimalism",
                                                TriviaList([EnvironmentNewline]))
                                        ])))
                            ])),
                        Scenario(
                            Token(
                                TriviaList([EnvironmentNewline, Whitespace("  ")]),
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
                                    "scenario outline with a description",
                                    TriviaList([EnvironmentNewline]))),
                            description: LiteralText(
                                TokenList([
                                    Literal(
                                        TriviaList([Comment("# comment"), EnvironmentNewline]),
                                        "This is a scenario outline description with comments",
                                        TriviaList([EnvironmentNewline, Comment("# comment 2"), EnvironmentNewline])),
                                    Literal(
                                        TriviaList(),
                                        "in the middle and before and at the end",
                                        TriviaList([EnvironmentNewline, Comment("# comment 3"), EnvironmentNewline]))
                                ])),
                            List([
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
                                                "the minimalism",
                                                TriviaList([EnvironmentNewline]))
                                        ])))
                            ]),
                            List([
                                Examples(
                                    Token(
                                        TriviaList([EnvironmentNewline, Whitespace("  ")]),
                                        SyntaxKind.ExamplesKeyword,
                                        "Examples",
                                        TriviaList()),
                                    Token(
                                        TriviaList(),
                                        SyntaxKind.ColonToken,
                                        TriviaList([Space])),
                                    LiteralText(
                                        Literal(
                                            TriviaList(),
                                            "examples with description",
                                            TriviaList([EnvironmentNewline]))),
                                    description: LiteralText(
                                        TokenList([
                                            Literal(
                                                TriviaList([Comment("# comment"), EnvironmentNewline]),
                                                "This is an examples description",
                                                TriviaList([EnvironmentNewline, Comment("# comment"), EnvironmentNewline])),
                                            Literal(
                                                TriviaList(),
                                                "with a comment in the middle",
                                                TriviaList([EnvironmentNewline, Comment("# comment"), EnvironmentNewline, EnvironmentNewline]))
                                        ])),
                                    Table(
                                        List([
                                            TableRow(
                                                Token(
                                                    TriviaList([Whitespace("    ")]),
                                                    SyntaxKind.VerticalBarToken,
                                                    TriviaList()),
                                                SeparatedList<PlainTextSyntax>([
                                                    LiteralText(
                                                        TableLiteral(
                                                            TriviaList([Space]),
                                                            "foo",
                                                            TriviaList([Space])))
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
                                                            "bar",
                                                            TriviaList([Space])))
                                                ]),
                                                Token(
                                                    TriviaList(),
                                                    SyntaxKind.VerticalBarToken,
                                                    TriviaList([EnvironmentNewline])))
                                        ])))
                            ])),
                        Scenario(
                            Token(
                                TriviaList([EnvironmentNewline, Whitespace("  ")]),
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
                                    "scenario with just a comment",
                                    TriviaList([EnvironmentNewline]))),
                            description: LiteralText(
                                TokenList([
                                    Literal(
                                        TriviaList([Whitespace("    "), Comment("# comment"), EnvironmentNewline]),
                                        "",
                                        TriviaList())
                                ])),
                            List([
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
                                                "the minimalism",
                                                TriviaList([EnvironmentNewline]))
                                        ])))
                            ])),
                        Scenario(
                            Token(
                                TriviaList([EnvironmentNewline, Whitespace("  ")]),
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
                                    "scenario with a comment with new lines around",
                                    TriviaList([EnvironmentNewline, EnvironmentNewline]))),
                            description: LiteralText(
                                TokenList([
                                    Literal(
                                        TriviaList([Whitespace("    "), Comment("# comment"), EnvironmentNewline, EnvironmentNewline]),
                                        "",
                                        TriviaList())
                                ])),
                            List([
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
                                                "the minimalism",
                                                TriviaList([Space, EnvironmentNewline]))
                                        ])))
                            ]))
                    ])
                ),
                Token(
                    TriviaList(),
                    SyntaxKind.EndOfFileToken,
                    TriviaList()))
        );

        tree.ToString().Should().Be(source);
    }
}
