using Reqnroll.CodeAnalysis.Gherkin.Syntax;

namespace Reqnroll.CodeAnalysis.Gherkin.Parsing;

using static SyntaxFactory;

public partial class DataTablesTests
{
    [Fact]
    public void MinimalisticDataTablesIsRepresentedInTree()
    {
        const string source =
            """
            Feature: Minimalistic DataTables

              Scenario: minimal
                Given a table
                  | foo | bar |
                  | boz | boo |

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
                            "Minimalistic DataTables",
                            TriviaList([EnvironmentNewLine]))),
                    scenarios: List([
                        Scenario(
                            Token(
                                TriviaList([EnvironmentNewLine, Whitespace("  ")]),
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
                                    "minimal",
                                    TriviaList([EnvironmentNewLine]))),
                            steps: List([
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
                                                "a table",
                                                TriviaList([EnvironmentNewLine]))
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
                                                                "foo",
                                                                TriviaList([Space]))),
                                                        Token(
                                                            TriviaList(),
                                                            SyntaxKind.VerticalBarToken,
                                                            TriviaList()),
                                                        LiteralText(
                                                            TableLiteral(
                                                                TriviaList([Space]),
                                                                "bar",
                                                                TriviaList([Space])))
                                                    ]),
                                                    Token(
                                                        TriviaList(),
                                                        SyntaxKind.VerticalBarToken,
                                                        TriviaList([EnvironmentNewLine]))),
                                                TableRow(
                                                    Token(
                                                        TriviaList([Whitespace("    ")]),
                                                        SyntaxKind.VerticalBarToken,
                                                        TriviaList()),
                                                    SeparatedList<PlainTextSyntax>([
                                                        LiteralText(
                                                            TableLiteral(
                                                                TriviaList([Space]),
                                                                "boz",
                                                                TriviaList([Space]))),
                                                        Token(
                                                            TriviaList(),
                                                            SyntaxKind.VerticalBarToken,
                                                            TriviaList()),
                                                        LiteralText(
                                                            TableLiteral(
                                                                TriviaList([Space]),
                                                                "boo",
                                                                TriviaList([Space])))
                                                    ]),
                                                    Token(
                                                        TriviaList(),
                                                        SyntaxKind.VerticalBarToken,
                                                        TriviaList([EnvironmentNewLine])))
                                            ]))))
                            ]))
                    ])),
                Token(
                    TriviaList(),
                    SyntaxKind.EndOfFileToken,
                    TriviaList()))
        );

        tree.ToString().Should().Be(source);
    }
}
