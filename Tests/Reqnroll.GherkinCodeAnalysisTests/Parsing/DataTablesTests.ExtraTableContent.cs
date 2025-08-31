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
                    ColonWithSpace,
                    Name(
                        TriviaList(),
                        "Extra table content",
                        TriviaList([EnvironmentNewLine])),
                    Description(
                        TokenList([
                            DescriptionText(
                                TriviaList([Whitespace("  ")]),
                                "Tables are delimited by pipes on both sides.",
                                TriviaList([EnvironmentNewLine])),
                            DescriptionText(
                                TriviaList([Whitespace("  ")]),
                                "Anything that isn't enclosed is not part of",
                                TriviaList([EnvironmentNewLine])),
                            DescriptionText(
                                TriviaList([Whitespace("  ")]),
                                "the table.",
                                TriviaList([EnvironmentNewLine])),
                            DescriptionText(
                                TriviaList([EnvironmentNewLine, Whitespace("  ")]),
                                "It is not recommended to use this feature, but",
                                TriviaList([EnvironmentNewLine])),
                            DescriptionText(
                                TriviaList([Whitespace("  ")]),
                                "it is how the implementation currently works.",
                                TriviaList([EnvironmentNewLine]))
                        ])),
                    examples: List([
                        Example(
                            Token(
                                TriviaList([EnvironmentNewLine, Whitespace("  ")]),
                                SyntaxKind.ExampleKeyword,
                                "Scenario",
                                TriviaList()),
                            ColonWithSpace,
                            Name(
                                TriviaList(),
                                "We're a bit extra",
                                TriviaList([EnvironmentNewLine])),
                            steps: List([
                                Step(
                                    Token(
                                        TriviaList([Whitespace("    ")]),
                                        SyntaxKind.ContextStepKeyword,
                                        "Given",
                                        TriviaList([Space])),
                                    StepText(
                                        TriviaList(),
                                        "a pirate crew",
                                        TriviaList([EnvironmentNewLine])),
                                    StepTable(
                                        Table(
                                            List([
                                                TableRow(
                                                    Token(
                                                        TriviaList([Whitespace("      ")]),
                                                        SyntaxKind.VerticalBarToken,
                                                        TriviaList([Space])),
                                                    TableCellList([
                                                        TableCell(
                                                            TableLiteral(
                                                                TriviaList(),
                                                                "Luffy",
                                                                TriviaList())),
                                                        Token(
                                                            TriviaList([Space]),
                                                            SyntaxKind.VerticalBarToken,
                                                            TriviaList([Space])),
                                                        TableCell(
                                                            TableLiteral(
                                                                TriviaList(),
                                                                "Zorro",
                                                                TriviaList())),
                                                        Token(
                                                            TriviaList([Space]),
                                                            SyntaxKind.VerticalBarToken,
                                                            TriviaList([Space])),
                                                        TableCell(
                                                            TableLiteral(
                                                                TriviaList(),
                                                                "Doflamingo \\",
                                                                "Doflamingo \\",
                                                                TriviaList()))
                                                    ]),
                                                    MissingToken(
                                                        TriviaList(),
                                                        SyntaxKind.VerticalBarToken,
                                                        TriviaList([EnvironmentNewLine]))),
                                                TableRow(
                                                    Token(
                                                        TriviaList([Whitespace("      ")]),
                                                        SyntaxKind.VerticalBarToken,
                                                        TriviaList([Space])),
                                                    TableCellList([
                                                        TableCell(
                                                            TableLiteral(
                                                                TriviaList(),
                                                                "Nami",
                                                                TriviaList())),
                                                        Token(
                                                            TriviaList([Whitespace("  ")]),
                                                            SyntaxKind.VerticalBarToken,
                                                            TriviaList([Space])),
                                                        TableCell(
                                                            TableLiteral(
                                                                TriviaList(),
                                                                "Brook",
                                                                TriviaList())),
                                                        Token(
                                                            TriviaList([Space]),
                                                            SyntaxKind.VerticalBarToken,
                                                            TriviaList([Space])),
                                                        TableCell(
                                                            TableLiteral(
                                                                TriviaList(),
                                                                "BlackBeard",
                                                                TriviaList()))
                                                    ]),
                                                    MissingToken(
                                                        TriviaList(),
                                                        SyntaxKind.VerticalBarToken,
                                                        TriviaList([EnvironmentNewLine])))
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
