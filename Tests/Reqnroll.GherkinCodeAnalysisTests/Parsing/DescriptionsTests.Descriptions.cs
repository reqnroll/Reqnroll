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
                            TriviaList([EnvironmentNewline]))),
                    description: LiteralText(
                        TokenList([
                            Literal(
                                TriviaList([Whitespace("  ")]),
                                "This is a single line description",
                                TriviaList([EnvironmentNewline]))
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
                                        TriviaList([EnvironmentNewline])),
                                    Literal(
                                        TriviaList(Whitespace("  ")),
                                        "has two lines and indented with two spaces",
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
                            LiteralText(
                                TokenList([
                                    Literal(
                                        TriviaList(),
                                        "This is a description without indentation",
                                        TriviaList([EnvironmentNewline]))
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
                                        TriviaList(),
                                        "This description",
                                        TriviaList([EnvironmentNewline, EnvironmentNewline])),
                                    Literal(
                                        TriviaList(),
                                        "has an empty line in the middle",
                                        TriviaList([EnvironmentNewline]))
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
                                    "empty lines around",
                                    TriviaList([EnvironmentNewline]))),
                            description: LiteralText(
                                TokenList([
                                    Literal(
                                        TriviaList([EnvironmentNewline]),
                                        "This description",
                                        TriviaList([EnvironmentNewline])),
                                    Literal(
                                        TriviaList(),
                                        "has an empty lines around",
                                        TriviaList([EnvironmentNewline, EnvironmentNewline]))
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
                                    "comment after description",
                                    TriviaList([EnvironmentNewline]))),
                            description: LiteralText(
                                TokenList([
                                    Literal(
                                        TriviaList(),
                                        "This description",
                                        TriviaList([EnvironmentNewline])),
                                    Literal(
                                        TriviaList(),
                                        "has a comment after",
                                        TriviaList([
                                            EnvironmentNewline,
                                            EnvironmentNewline,
                                            Comment("# this is a comment"),
                                            EnvironmentNewline]))
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
                                    "comment right after description",
                                    TriviaList([EnvironmentNewline]))),
                            description: LiteralText(
                                TokenList([
                                    Literal(
                                        TriviaList(),
                                        "This description",
                                        TriviaList([EnvironmentNewline])),
                                    Literal(
                                        TriviaList(),
                                        "has a comment right after",
                                        TriviaList([
                                            EnvironmentNewline,
                                            Whitespace("  "),
                                            Comment("#  this is another comment"),
                                            EnvironmentNewline]))
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
                                    "description with escaped docstring separator",
                                    TriviaList([EnvironmentNewline]))),
                            description: LiteralText(
                                TokenList([
                                    Literal(
                                        TriviaList(),
                                        "This description has an \"\"\" (escaped docstring sparator)",
                                        TriviaList([EnvironmentNewline]))
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
                            scenarioKeyword: Token(
                                TriviaList([EnvironmentNewline, Whitespace("  ")]),
                                SyntaxKind.ScenarioKeyword,
                                "Scenario Outline",
                                TriviaList()),
                            colonToken: Token(
                                TriviaList(),
                                SyntaxKind.ColonToken,
                                TriviaList([Space])),
                            LiteralText(
                                Literal(
                                    TriviaList(),
                                    "scenario outline with a description",
                                    TriviaList([EnvironmentNewline]))),
                            LiteralText(
                                TokenList([
                                    Literal(
                                        TriviaList(),
                                        "This is a scenario outline description",
                                        TriviaList([EnvironmentNewline]))
                                ])
                            ),
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
                                    LiteralText(
                                        TokenList([
                                            Literal(
                                                TriviaList(),
                                                "This is an examples description",
                                                TriviaList([EnvironmentNewline]))
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
                                                    TriviaList()),
                                                SeparatedList<PlainTextSyntax>([
                                                    LiteralText(
                                                        TableLiteral(
                                                            TriviaList([Space]),
                                                            "bar",
                                                            TriviaList([Space])))
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
