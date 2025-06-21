using Reqnroll.CodeAnalysis.Gherkin.Syntax;

namespace Reqnroll.CodeAnalysis.Gherkin.Parsing;

using static SyntaxFactory;

public partial class DataTablesTests
{
    [Fact]
    public void MoreComplexDataTablesIsRepresentedInTree()
    {
        const string source =
            """
            Feature: More Complex DataTables

              Scenario: table with comments and empty lines
                Given a table
                  | foo | bar |
                  | boz | boo |

                  # a comment
                  | baz | bop |

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
                            "More Complex DataTables",
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
                                    "table with comments and empty lines",
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
                                                        TriviaList([
                                                            EnvironmentNewLine,
                                                            EnvironmentNewLine,
                                                            Whitespace("    "),
                                                            Comment("# a comment"),
                                                            EnvironmentNewLine
                                                        ]))),
                                                TableRow(
                                                    Token(
                                                        TriviaList([Whitespace("    ")]),
                                                        SyntaxKind.VerticalBarToken,
                                                        TriviaList()),
                                                    SeparatedList<PlainTextSyntax>([
                                                        LiteralText(
                                                            TableLiteral(
                                                                TriviaList([Space]),
                                                                "baz",
                                                                TriviaList([Space]))),
                                                        Token(
                                                            TriviaList(),
                                                            SyntaxKind.VerticalBarToken,
                                                            TriviaList()),
                                                        LiteralText(
                                                            TableLiteral(
                                                                TriviaList([Space]),
                                                                "bop",
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
