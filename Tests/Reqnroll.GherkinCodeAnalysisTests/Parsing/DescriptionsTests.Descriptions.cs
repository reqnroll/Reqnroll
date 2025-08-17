using Reqnroll.CodeAnalysis.Gherkin.Syntax;

namespace Reqnroll.CodeAnalysis.Gherkin.Parsing;

using static SyntaxFactory;

public partial class DescriptionsTests
{
    [Fact]
    public void DescriptionsAreRepresentedInTree()
    {
        // Taken from good/descriptions.feature
        const string source =
            """
            Feature: Descriptions everywhere
              This is a single line description

              Scenario: two lines
              This description
              has two lines and indented with two spaces
                Given the minimalism

            Scenario: without indentation
            This is a description without indentation
              Given the minimalism

              Scenario: empty lines in the middle
              This description

              has an empty line in the middle
                Given the minimalism

              Scenario: empty lines around

              This description
              has an empty lines around

                Given the minimalism

              Scenario: comment after description
              This description
              has a comment after

            # this is a comment
                Given the minimalism

              Scenario: comment right after description
              This description
              has a comment right after
                #  this is another comment

                Given the minimalism

              Scenario: description with escaped docstring separator
              This description has an \"\"\" (escaped docstring sparator)

                Given the minimalism

              Scenario Outline: scenario outline with a description
            This is a scenario outline description
                Given the minimalism

              Examples: examples with description
            This is an examples description
                | foo |
                | bar |

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
                            "Descriptions everywhere",
                            TriviaList([EnvironmentNewLine]))),
                    description: LiteralText(
                        TokenList([
                            Literal(
                                TriviaList([Whitespace("  ")]),
                                "This is a single line description",
                                TriviaList([EnvironmentNewLine]))
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
                                        TriviaList([EnvironmentNewLine])),
                                    Literal(
                                        TriviaList(Whitespace("  ")),
                                        "has two lines and indented with two spaces",
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
                            LiteralText(
                                TokenList([
                                    Literal(
                                        TriviaList(),
                                        "This is a description without indentation",
                                        TriviaList([EnvironmentNewLine]))
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
                                        TriviaList(),
                                        "This description",
                                        TriviaList([EnvironmentNewLine, EnvironmentNewLine])),
                                    Literal(
                                        TriviaList(),
                                        "has an empty line in the middle",
                                        TriviaList([EnvironmentNewLine]))
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
                                    "empty lines around",
                                    TriviaList([EnvironmentNewLine]))),
                            description: LiteralText(
                                TokenList([
                                    Literal(
                                        TriviaList([EnvironmentNewLine]),
                                        "This description",
                                        TriviaList([EnvironmentNewLine])),
                                    Literal(
                                        TriviaList(),
                                        "has an empty lines around",
                                        TriviaList([EnvironmentNewLine, EnvironmentNewLine]))
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
                                    "comment after description",
                                    TriviaList([EnvironmentNewLine]))),
                            description: LiteralText(
                                TokenList([
                                    Literal(
                                        TriviaList(),
                                        "This description",
                                        TriviaList([EnvironmentNewLine])),
                                    Literal(
                                        TriviaList(),
                                        "has a comment after",
                                        TriviaList([
                                            EnvironmentNewLine,
                                            EnvironmentNewLine,
                                            Comment("# this is a comment"),
                                            EnvironmentNewLine]))
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
                                    "comment right after description",
                                    TriviaList([EnvironmentNewLine]))),
                            description: LiteralText(
                                TokenList([
                                    Literal(
                                        TriviaList(),
                                        "This description",
                                        TriviaList([EnvironmentNewLine])),
                                    Literal(
                                        TriviaList(),
                                        "has a comment right after",
                                        TriviaList([
                                            EnvironmentNewLine,
                                            Whitespace("  "),
                                            Comment("#  this is another comment"),
                                            EnvironmentNewLine]))
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
                                    "description with escaped docstring separator",
                                    TriviaList([EnvironmentNewLine]))),
                            description: LiteralText(
                                TokenList([
                                    Literal(
                                        TriviaList(),
                                        "This description has an \"\"\" (escaped docstring sparator)",
                                        TriviaList([EnvironmentNewLine]))
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
                            LiteralText(
                                TokenList([
                                    Literal(
                                        TriviaList(),
                                        "This is a scenario outline description",
                                        TriviaList([EnvironmentNewLine]))
                                ])
                            ),
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
                                    LiteralText(
                                        TokenList([
                                            Literal(
                                                TriviaList(),
                                                "This is an examples description",
                                                TriviaList([EnvironmentNewLine]))
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
                                                            TriviaList()))
                                                ]),
                                                Token(
                                                    TriviaList([Space]),
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
