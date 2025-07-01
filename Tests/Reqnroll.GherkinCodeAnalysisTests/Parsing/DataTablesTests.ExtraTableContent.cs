using Reqnroll.CodeAnalysis.Gherkin.Syntax;

namespace Reqnroll.CodeAnalysis.Gherkin.Parsing;

using static SyntaxFactory;

public partial class DataTablesTests
{
    [Fact]
    public void ExtraTableContentIsRepresentedInTree()
    {
        // Taken from good/extra_table_content.feature
        const string source =
            """
            Feature: Extra table content
              Tables are delimited by pipes on both sides.
              Anything that isn't enclosed is not part of
              the table.

              It is not recommended to use this feature, but
              it is how the implementation currently works.

              Scenario: We're a bit extra
                Given a pirate crew
                  | Luffy | Zorro | Doflamingo \
                  | Nami  | Brook | BlackBeard
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
                            "Extra table content",
                            TriviaList([EnvironmentNewline]))),
                    LiteralText(
                        TokenList([
                            Literal(
                                TriviaList([Whitespace("  ")]),
                                "Tables are delimited by pipes on both sides.",
                                TriviaList([EnvironmentNewline])),
                            Literal(
                                TriviaList([Whitespace("  ")]),
                                "Anything that isn't enclosed is not part of",
                                TriviaList([EnvironmentNewline])),
                            Literal(
                                TriviaList([Whitespace("  ")]),
                                "the table.",
                                TriviaList([EnvironmentNewline])),
                            Literal(
                                TriviaList([Whitespace("  ")]),
                                Environment.NewLine,
                                TriviaList([EnvironmentNewline])),
                            Literal(
                                TriviaList([Whitespace("  ")]),
                                "It is not recommended to use this feature, but",
                                TriviaList([EnvironmentNewline])),
                            Literal(
                                TriviaList([Whitespace("  ")]),
                                "it is how the implementation currently works.",
                                TriviaList([EnvironmentNewline, EnvironmentNewline]))
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
                                    "We're a bit extra",
                                    TriviaList([EnvironmentNewline]))),
                            steps: List([
                                Step(
                                    Token(
                                        TriviaList([Whitespace("    ")]),
                                        SyntaxKind.GivenKeyword,
                                        "Given",
                                        TriviaList([Space])),
                                    LiteralText(
                                        Literal(
                                            TriviaList(),
                                            "a pirate crew",
                                            TriviaList([EnvironmentNewline]))),
                                    StepTable(
                                        Table(
                                            List([
                                                TableRow(
                                                    Token(
                                                        TriviaList([Whitespace("      ")]),
                                                        SyntaxKind.VerticalBarToken,
                                                        TriviaList()),
                                                    SeparatedList<PlainTextSyntax>([
                                                        LiteralText(
                                                            TableLiteral(
                                                                TriviaList([Space]),
                                                                "Luffy",
                                                                TriviaList([Space]))),
                                                        Token(
                                                            TriviaList(),
                                                            SyntaxKind.VerticalBarToken,
                                                            TriviaList()),
                                                        LiteralText(
                                                            TableLiteral(
                                                                TriviaList([Space]),
                                                                "Zorro",
                                                                TriviaList([Space]))),
                                                        Token(
                                                            TriviaList(),
                                                            SyntaxKind.VerticalBarToken,
                                                            TriviaList()),
                                                        LiteralText(
                                                            TableLiteral(
                                                                TriviaList([Space]),
                                                                "Doflamingo \\",
                                                                TriviaList()))
                                                    ]),
                                                    MissingToken(
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
                                                                "Nami",
                                                                TriviaList([Whitespace("  ")]))),
                                                        Token(
                                                            TriviaList(),
                                                            SyntaxKind.VerticalBarToken,
                                                            TriviaList()),
                                                        LiteralText(
                                                            TableLiteral(
                                                                TriviaList(),
                                                                "Brook",
                                                                TriviaList([Space]))),
                                                        Token(
                                                            TriviaList(),
                                                            SyntaxKind.VerticalBarToken,
                                                            TriviaList()),
                                                        LiteralText(
                                                            TableLiteral(
                                                                TriviaList([Space]),
                                                                "BlackBeard",
                                                                TriviaList()))
                                                    ]),
                                                    MissingToken(
                                                        TriviaList(),
                                                        SyntaxKind.VerticalBarToken,
                                                        TriviaList([EnvironmentNewline])))
                                            ]))))
                            ]))
                    ])),
                Token(
                    TriviaList(),
                    SyntaxKind.EndOfFileToken,
                    TriviaList())));

        tree.ToString().Should().Be(source);
    }
}
