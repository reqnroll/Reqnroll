using Reqnroll.CodeAnalysis.Gherkin.Syntax;

namespace Reqnroll.CodeAnalysis.Gherkin.Parsing;

using static SyntaxFactory;

public partial class ScenarioOutlineTests
{
    [Fact]
    public void ExampleTokensEverywhereIsRepresentedInTree()
    {
        // Taken from good/example_tokens_everywhere.feature
        const string source =
            """"
            Feature: Example tokens everywhere

              Scenario Outline: the <one>
                Given the <two>:
                  """
                  <three>
                  """
                Given the <four>:
                  | <five> |

                Examples:
                  | one | two  | three | four   | five  |
                  | un  | deux | trois | quatre | cinq  |
                  | uno | dos  | tres  | quatro | cinco |
            """";

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
                            "Example tokens everywhere",
                            TriviaList([EnvironmentNewline]))),
                    scenarios: List([
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
                                            "one",
                                            TriviaList()),
                                        Token(
                                            TriviaList(),
                                            SyntaxKind.GreaterThanToken,
                                            TriviaList([EnvironmentNewline])))
                                ])),
                            steps: List([
                                Step(
                                    Token(
                                        TriviaList([Whitespace("  ")]),
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
                                                    "two",
                                                    TriviaList()),
                                                Token(
                                                    TriviaList(),
                                                    SyntaxKind.GreaterThanToken,
                                                    TriviaList([EnvironmentNewline])))
                                        ])),
                                    StepDocString(
                                        DocString(
                                            Token(
                                                TriviaList([Whitespace("    ")]),
                                                SyntaxKind.DocStringDelimiterToken,
                                                "\"\"\"",
                                                TriviaList([EnvironmentNewline])),
                                            InterpolatedText(
                                                List<InterpolatedTextContentSyntax>([
                                                    Interpolation(
                                                        Token(
                                                            TriviaList([Whitespace("    ")]),
                                                            SyntaxKind.LessThanToken,
                                                            TriviaList()),
                                                        Identifier(
                                                            TriviaList(),
                                                            "three",
                                                            TriviaList()),
                                                        Token(
                                                            TriviaList(),
                                                            SyntaxKind.GreaterThanToken,
                                                            TriviaList([EnvironmentNewline])))
                                                ])),
                                            Token(
                                                TriviaList([Whitespace("    ")]),
                                                SyntaxKind.DocStringDelimiterToken,
                                                "\"\"\"",
                                                TriviaList([EnvironmentNewline]))))),
                                Step(
                                    Token(
                                        TriviaList([Whitespace("  ")]),
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
                                                    "four",
                                                    TriviaList()),
                                                Token(
                                                    TriviaList(),
                                                    SyntaxKind.GreaterThanToken,
                                                    TriviaList())),
                                            InterpolatedTextLiteral(
                                                Literal(
                                                    TriviaList(),
                                                    ":",
                                                    TriviaList([EnvironmentNewline])))
                                        ])),
                                    StepTable(
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
                                                                "<five>",
                                                                TriviaList([Whitespace("  ")])))
                                                    ]),
                                                    Token(
                                                        TriviaList(),
                                                        SyntaxKind.VerticalBarToken,
                                                        TriviaList([EnvironmentNewline])))
                                            ]))))
                            ]),
                            examples: List([
                                Examples(
                                    Token(
                                        TriviaList([EnvironmentNewline, Whitespace("  ")]),
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
                                                    TriviaList([Whitespace("    ")]),
                                                    SyntaxKind.VerticalBarToken,
                                                    TriviaList()),
                                                SeparatedList<PlainTextSyntax>([
                                                    LiteralText(
                                                        TableLiteral(
                                                            TriviaList([Space]),
                                                            "one",
                                                            TriviaList([Space]))),
                                                    Token(
                                                        TriviaList(),
                                                        SyntaxKind.VerticalBarToken,
                                                        TriviaList()),
                                                    LiteralText(
                                                        TableLiteral(
                                                            TriviaList([Space]),
                                                            "two",
                                                            TriviaList([Whitespace("  ")]))),
                                                    Token(
                                                        TriviaList(),
                                                        SyntaxKind.VerticalBarToken,
                                                        TriviaList()),
                                                    LiteralText(
                                                        TableLiteral(
                                                            TriviaList([Space]),
                                                            "three",
                                                            TriviaList([Space]))),
                                                    Token(
                                                        TriviaList(),
                                                        SyntaxKind.VerticalBarToken,
                                                        TriviaList()),
                                                    LiteralText(
                                                        TableLiteral(
                                                            TriviaList([Space]),
                                                            "four",
                                                            TriviaList([Whitespace("  ")]))),
                                                    Token(
                                                        TriviaList(),
                                                        SyntaxKind.VerticalBarToken,
                                                        TriviaList()),
                                                    LiteralText(
                                                        TableLiteral(
                                                            TriviaList([Space]),
                                                            "five",
                                                            TriviaList([Whitespace("  ")])))
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
                                                            "un",
                                                            TriviaList([Space]))),
                                                    Token(
                                                        TriviaList(),
                                                        SyntaxKind.VerticalBarToken,
                                                        TriviaList([Space])),
                                                    LiteralText(
                                                        TableLiteral(
                                                            TriviaList(),
                                                            "deux",
                                                            TriviaList([Space, Space]))),
                                                    Token(
                                                        TriviaList(),
                                                        SyntaxKind.VerticalBarToken,
                                                        TriviaList([Space])),
                                                    LiteralText(
                                                        TableLiteral(
                                                            TriviaList(),
                                                            "trois",
                                                            TriviaList([Space]))),
                                                    Token(
                                                        TriviaList(),
                                                        SyntaxKind.VerticalBarToken,
                                                        TriviaList([Space])),
                                                    LiteralText(
                                                        TableLiteral(
                                                            TriviaList(),
                                                            "quatre",
                                                            TriviaList([Space, Space]))),
                                                    Token(
                                                        TriviaList(),
                                                        SyntaxKind.VerticalBarToken,
                                                        TriviaList([Space])),
                                                    LiteralText(
                                                        TableLiteral(
                                                            TriviaList(),
                                                            "cinq",
                                                            TriviaList([Space, Space])))
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
                                                            "uno",
                                                            TriviaList([Space]))),
                                                    Token(
                                                        TriviaList(),
                                                        SyntaxKind.VerticalBarToken,
                                                        TriviaList([Space])),
                                                    LiteralText(
                                                        TableLiteral(
                                                            TriviaList(),
                                                            "dos",
                                                            TriviaList([Space, Space]))),
                                                    Token(
                                                        TriviaList(),
                                                        SyntaxKind.VerticalBarToken,
                                                        TriviaList([Space])),
                                                    LiteralText(
                                                        TableLiteral(
                                                            TriviaList(),
                                                            "tres",
                                                            TriviaList([Space]))),
                                                    Token(
                                                        TriviaList(),
                                                        SyntaxKind.VerticalBarToken,
                                                        TriviaList([Space])),
                                                    LiteralText(
                                                        TableLiteral(
                                                            TriviaList(),
                                                            "quatro",
                                                            TriviaList([Space, Space]))),
                                                    Token(
                                                        TriviaList(),
                                                        SyntaxKind.VerticalBarToken,
                                                        TriviaList([Space])),
                                                    LiteralText(
                                                        TableLiteral(
                                                            TriviaList(),
                                                            "cinco",
                                                            TriviaList([Space, Space])))
                                                ]),
                                                Token(
                                                    TriviaList(),
                                                    SyntaxKind.VerticalBarToken,
                                                    TriviaList([EnvironmentNewline])))
                                        ]))
                                )
                            ])
                        )
                    ])
                ),
                Token(
                    TriviaList(),
                    SyntaxKind.EndOfFileToken,
                    TriviaList())));

        tree.ToString().Should().Be(source);
    }
}
