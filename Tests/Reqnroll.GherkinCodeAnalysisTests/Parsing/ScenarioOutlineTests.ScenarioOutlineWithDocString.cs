using Reqnroll.CodeAnalysis.Gherkin.Syntax;

namespace Reqnroll.CodeAnalysis.Gherkin.Parsing;

using static SyntaxFactory;

public partial class ScenarioOutlineTests
{
    [Fact]
    public void ScenarioOutlineWithDocStringIsRepresentedInTree()
    {
        // Taken from good/scenario_outline_with_docstring.feature
        const string source =
            """"
            Feature: Scenario Outline with a docstring

            Scenario Outline: Greetings come in many forms
                Given this file:
                """<type>
                Greeting:<content>
                """

            Examples:
              | type  | content |
              | en    | Hello   |
              | fr    | Bonjour |

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
                            "Example Outline with a docstring",
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
                                    "Greetings come in many forms",
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
                                            "this file:",
                                            TriviaList([EnvironmentNewLine]))),
                                    StepDocString(
                                        DocString(
                                            Token(
                                                TriviaList([Whitespace("    ")]),
                                                SyntaxKind.DocStringDelimiterToken,
                                                "\"\"\"",
                                                TriviaList()),
                                            LiteralText(
                                                TokenList([
                                                    Literal(
                                                        TriviaList(),
                                                        "<type>",
                                                        TriviaList([EnvironmentNewLine])),
                                                    Literal(
                                                        TriviaList([Whitespace("    ")]),
                                                        "Greeting:<content>",
                                                        TriviaList([EnvironmentNewLine]))
                                                ])),
                                            Token(
                                                TriviaList([Whitespace("    ")]),
                                                SyntaxKind.DocStringDelimiterToken,
                                                "\"\"\"",
                                                TriviaList([EnvironmentNewLine])))))
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
                                                            "type",
                                                            TriviaList())),
                                                    Token(
                                                        TriviaList([Whitespace("  ")]),
                                                        SyntaxKind.VerticalBarToken,
                                                        TriviaList([Space])),
                                                    TextTableCell(
                                                        TableLiteral(
                                                            TriviaList(),
                                                            "content",
                                                            TriviaList()))
                                                ]),
                                                Token(
                                                    TriviaList([Space]),
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
                                                            "en",
                                                            TriviaList())),
                                                    Token(
                                                        TriviaList([Whitespace("  ")]),
                                                        SyntaxKind.VerticalBarToken,
                                                        TriviaList([Space])),
                                                    TextTableCell(
                                                        TableLiteral(
                                                            TriviaList(),
                                                            "Hello",
                                                            TriviaList())
                                                    )
                                                ]),
                                                Token(
                                                    TriviaList([Space]),
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
                                                            "fr",
                                                            TriviaList())),
                                                    Token(
                                                        TriviaList([Whitespace("  ")]),
                                                        SyntaxKind.VerticalBarToken,
                                                        TriviaList([Space])),
                                                    TextTableCell(
                                                        TableLiteral(
                                                            TriviaList(),
                                                            "Bonjour",
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
