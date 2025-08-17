using Reqnroll.CodeAnalysis.Gherkin.Syntax;

namespace Reqnroll.CodeAnalysis.Gherkin.Parsing;

using static SyntaxFactory;

public partial class ScenarioOutlineTests
{
    [Fact]
    public void ScenarioOutlineWithValueWithDollarSignIsRepresentedInTree()
    {
        // Taken from good/scenario_outline_with_value_with_dollar_sign.feature
        const string source =
            """
            Feature: Scenario Outline with a value with a dollar sign ($)

            Scenario Outline: minimalistic
                Given the <what>

            Examples:
              | what     |
              | pa$$word |

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
                            "Example Outline with a value with a dollar sign ($)",
                            TriviaList([EnvironmentNewLine, EnvironmentNewLine]))),
                    examples: List([
                        Example(
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
                                    Token(
                                        TriviaList([EnvironmentNewLine]),
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
                                                    TriviaList([Whitespace("  ")]),
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
                                                    TriviaList([Whitespace("     ")]),
                                                    SyntaxKind.VerticalBarToken,
                                                    TriviaList([EnvironmentNewLine]))),
                                            TableRow(
                                                Token(
                                                    TriviaList([Whitespace("  ")]),
                                                    SyntaxKind.VerticalBarToken,
                                                    TriviaList([Space])),
                                                TableCellList([
                                                    TextTableCell(
                                                        TableLiteral(
                                                            TriviaList(),
                                                            "pa$$word",
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
