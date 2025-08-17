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
                            TriviaList([EnvironmentNewLine]))),
                    description: LiteralText(
                        TokenList([
                            Literal(
                                TriviaList([Whitespace("  ")]),
                                "This is a description",
                                TriviaList([EnvironmentNewLine, Whitespace("  "), Comment("# comment"), EnvironmentNewLine])),
                            Literal(
                                TriviaList([Whitespace("  ")]),
                                "with a comment in the middle and at the end",
                                TriviaList([EnvironmentNewLine, Whitespace("  "), Comment("# comment 2"), EnvironmentNewLine, EnvironmentNewLine]))
                        ])),
                    examples: List([
                        Example(
                            Token(
                                TriviaList([Whitespace("  ")]),
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
                                    "two lines",
                                    TriviaList([EnvironmentNewLine]))),
                            description: LiteralText(
                                TokenList([
                                    Literal(
                                        TriviaList([Whitespace("  ")]),
                                        "This description",
                                        TriviaList([EnvironmentNewLine, Whitespace("  "), Comment("# comment"), EnvironmentNewLine, Whitespace("  "), Comment("# comment 2"), EnvironmentNewLine])),
                                    Literal(
                                        TriviaList([Whitespace("  ")]),
                                        "has two lines and two comments in the middle and is indented with two spaces",
                                        TriviaList([EnvironmentNewLine]))
                                ])),
                            List([
                                Step(
                                    Token(
                                        TriviaList([Whitespace("    ")]),
                                        SyntaxKind.ContextStepKeyword,
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
                        Example(
                            Token(
                                TriviaList([EnvironmentNewLine]),
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
                                    "without indentation",
                                    TriviaList([EnvironmentNewLine]))),
                            description: LiteralText(
                                TokenList([
                                    Literal(
                                        TriviaList(),
                                        "This is a description without indentation",
                                        TriviaList([EnvironmentNewLine, Comment("# comment"), EnvironmentNewLine])),
                                    Literal(
                                        TriviaList(),
                                        "and a comment in the middle and at the end",
                                        TriviaList([EnvironmentNewLine, Comment("# comment 2"), EnvironmentNewLine, EnvironmentNewLine]))
                                ])),
                            List([
                                Step(
                                    Token(
                                        TriviaList([Whitespace("  ")]),
                                        SyntaxKind.ContextStepKeyword,
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
                        Example(
                            Token(
                                TriviaList([EnvironmentNewLine, Whitespace("  ")]),
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
                                    "empty lines in the middle",
                                    TriviaList([EnvironmentNewLine]))),
                            description: LiteralText(
                                TokenList([
                                    Literal(
                                        TriviaList([Whitespace("  ")]),
                                        "This description",
                                        TriviaList([EnvironmentNewLine, Whitespace("  "), Comment("# comment"), EnvironmentNewLine, EnvironmentNewLine])),
                                    Literal(
                                        TriviaList([Whitespace("  ")]),
                                        "has an empty line and a comment in the middle",
                                        TriviaList([EnvironmentNewLine]))
                                ])),
                            List([
                                Step(
                                    Token(
                                        TriviaList([Whitespace("    ")]),
                                        SyntaxKind.ContextStepKeyword,
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
                        Example(
                            Token(
                                TriviaList([EnvironmentNewLine, Whitespace("  ")]),
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
                                    "empty lines around",
                                    TriviaList([EnvironmentNewLine]))),
                            description: LiteralText(
                                TokenList([
                                    Literal(
                                        TriviaList([EnvironmentNewLine, Whitespace("  "), Comment("# comment"), EnvironmentNewLine]),
                                        "This description",
                                        TriviaList([EnvironmentNewLine])),
                                    Literal(
                                        TriviaList([Whitespace("  ")]),
                                        "has an empty lines around",
                                        TriviaList([EnvironmentNewLine, Whitespace("  "), Comment("# comment"), EnvironmentNewLine, EnvironmentNewLine]))
                                ])),
                            List([
                                Step(
                                    Token(
                                        TriviaList([Whitespace("    ")]),
                                        SyntaxKind.ContextStepKeyword,
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
                        Example(
                            Token(
                                TriviaList([EnvironmentNewLine, Whitespace("  ")]),
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
                                    "scenario outline with a description",
                                    TriviaList([EnvironmentNewLine]))),
                            description: LiteralText(
                                TokenList([
                                    Literal(
                                        TriviaList([Comment("# comment"), EnvironmentNewLine]),
                                        "This is a scenario outline description with comments",
                                        TriviaList([EnvironmentNewLine, Comment("# comment 2"), EnvironmentNewLine])),
                                    Literal(
                                        TriviaList(),
                                        "in the middle and before and at the end",
                                        TriviaList([EnvironmentNewLine, Comment("# comment 3"), EnvironmentNewLine]))
                                ])),
                            List([
                                Step(
                                    Token(
                                        TriviaList([Whitespace("    ")]),
                                        SyntaxKind.ContextStepKeyword,
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
                            ]),
                            List([
                                Examples(
                                    Token(
                                        TriviaList([EnvironmentNewLine, Whitespace("  ")]),
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
                                            TriviaList([EnvironmentNewLine]))),
                                    description: LiteralText(
                                        TokenList([
                                            Literal(
                                                TriviaList([Comment("# comment"), EnvironmentNewLine]),
                                                "This is an examples description",
                                                TriviaList([EnvironmentNewLine, Comment("# comment"), EnvironmentNewLine])),
                                            Literal(
                                                TriviaList(),
                                                "with a comment in the middle",
                                                TriviaList([EnvironmentNewLine, Comment("# comment"), EnvironmentNewLine, EnvironmentNewLine]))
                                        ])),
                                    Table(
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
                                                            "foo",
                                                            TriviaList()))
                                                ]),
                                                Token(
                                                    TriviaList([Space]),
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
                                                            "bar",
                                                            TriviaList([Space])))
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
                                    TriviaList([EnvironmentNewLine]))),
                            description: LiteralText(
                                TokenList([
                                    Literal(
                                        TriviaList([Whitespace("    "), Comment("# comment"), EnvironmentNewLine]),
                                        "",
                                        TriviaList())
                                ])),
                            List([
                                Step(
                                    Token(
                                        TriviaList([Whitespace("    ")]),
                                        SyntaxKind.ContextStepKeyword,
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
                        Example(
                            Token(
                                TriviaList([EnvironmentNewLine, Whitespace("  ")]),
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
                                    "scenario with a comment with new lines around",
                                    TriviaList([EnvironmentNewLine, EnvironmentNewLine]))),
                            description: LiteralText(
                                TokenList([
                                    Literal(
                                        TriviaList([Whitespace("    "), Comment("# comment"), EnvironmentNewLine, EnvironmentNewLine]),
                                        "",
                                        TriviaList())
                                ])),
                            List([
                                Step(
                                    Token(
                                        TriviaList([Whitespace("    ")]),
                                        SyntaxKind.ContextStepKeyword,
                                        "Given",
                                        TriviaList([Space])),
                                    LiteralText(
                                        TokenList([
                                            Token(
                                                TriviaList(),
                                                SyntaxKind.LiteralToken,
                                                "the minimalism",
                                                TriviaList([Space, EnvironmentNewLine]))
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
