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
                            TriviaList([EnvironmentNewLine, EnvironmentNewLine]))),
                    examples: List([
                        Example(
                            Token(
                                TriviaList([Whitespace("  ")]),
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
                                        TokenList([
                                            StepTextLiteral(
                                                TriviaList(),
                                                "the <what>",
                                                TriviaList([EnvironmentNewLine]))
                                        ])))
                            ]),
                            examples: List([
                                Examples(
                                    List([
                                        Tag(
                                            Token(TriviaList([EnvironmentNewLine, Whitespace("    ")]), SyntaxKind.AtToken, TriviaList()),
                                            Literal(TriviaList(), "foo", TriviaList([EnvironmentNewLine])))
                                    ]),
                                    Token(
                                        TriviaList(),
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
                                                    TriviaList([Space])),
                                                TableCellList([
                                                    TextTableCell(
                                                        TableLiteral(
                                                            TriviaList(),
                                                            "what",
                                                            TriviaList()))
                                                ]),
                                                Token(
                                                    TriviaList([Space]),
                                                    SyntaxKind.VerticalBarToken,
                                                    TriviaList([EnvironmentNewLine]))),
                                            TableRow(
                                                Token(
                                                    TriviaList([Whitespace("      ")]),
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
                                                    TriviaList([Whitespace("  ")]),
                                                    SyntaxKind.VerticalBarToken,
                                                    TriviaList([EnvironmentNewLine])))
                                        ]))
                                ),
                                Examples(
                                    List([
                                        Tag(
                                            Token(TriviaList([EnvironmentNewLine, Whitespace("    ")]), SyntaxKind.AtToken, TriviaList()),
                                            Literal(TriviaList(), "bar", TriviaList([EnvironmentNewLine])))
                                    ]),
                                    Token(
                                        TriviaList(),
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
                                                    TriviaList([Space])),
                                                TableCellList([
                                                    TextTableCell(
                                                        TableLiteral(
                                                            TriviaList(),
                                                            "what",
                                                            TriviaList()))
                                                ]),
                                                Token(
                                                    TriviaList([Space]),
                                                    SyntaxKind.VerticalBarToken,
                                                    TriviaList([EnvironmentNewLine]))),
                                            TableRow(
                                                Token(
                                                    TriviaList([Whitespace("      ")]),
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
                                                    TriviaList([Whitespace("  ")]),
                                                    SyntaxKind.VerticalBarToken,
                                                    TriviaList([EnvironmentNewLine])))
                                        ]))
                                )
                            ])
                        ),
                        Example(
                            List([
                                Tag(
                                    Token(TriviaList([EnvironmentNewLine, Whitespace("  ")]), SyntaxKind.AtToken, TriviaList()),
                                    Literal(TriviaList(), "zap", TriviaList([EnvironmentNewLine])))
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
                                    "ha ok",
                                    TriviaList([EnvironmentNewLine]))),
                            steps: List<StepSyntax>())
                    ])),
                Token(
                    TriviaList(),
                    SyntaxKind.EndOfFileToken,
                    TriviaList())));

        tree.ToString().Should().Be(source);
    }
}
