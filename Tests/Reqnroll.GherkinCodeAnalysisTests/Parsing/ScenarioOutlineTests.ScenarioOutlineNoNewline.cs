using Reqnroll.CodeAnalysis.Gherkin.Syntax;

namespace Reqnroll.CodeAnalysis.Gherkin.Parsing;

using static SyntaxFactory;

public partial class ScenarioOutlineTests
{
    [Fact]
    public void ScenarioOutlineNoNewlineIsRepresentedInTree()
    {
        // Taken from good/scenario_outline_no_newline.feature
        const string source =
            """
            Feature: Minimal Scenario Outline

            Scenario Outline: minimalistic
                Given the <what>

            Examples:
              | what       |
              | minimalism |
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
                            "Minimal Scenario Outline",
                            TriviaList([EnvironmentNewline, EnvironmentNewline]))),
                    scenarios: List([
                        Scenario(
                            Token(
                                TriviaList(),
                                SyntaxKind.ScenarioKeyword,
                                "Scenario Outline",
                                TriviaList()),
                            Token(
                                TriviaList(),
                                SyntaxKind.ColonToken,
                                TriviaList([Space])),
                            LiteralText(
                                Literal(
                                    TriviaList(),
                                    "minimalistic",
                                    TriviaList([EnvironmentNewline]))),
                            steps: List([
                                Step(
                                    Token(
                                        TriviaList([Whitespace("    ")]),
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
                                                    "what",
                                                    TriviaList()),
                                                Token(
                                                    TriviaList(),
                                                    SyntaxKind.GreaterThanToken,
                                                    TriviaList([EnvironmentNewline])))
                                        ])))
                            ]),
                            examples: List([
                                Examples(
                                    Token(
                                        TriviaList([EnvironmentNewline]),
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
                                                    TriviaList([Whitespace("  ")]),
                                                    SyntaxKind.VerticalBarToken,
                                                    TriviaList([Space])),
                                                SeparatedList<PlainTextSyntax>([
                                                    LiteralText(
                                                        TableLiteral(
                                                            TriviaList(),
                                                            "what",
                                                            TriviaList([Whitespace("       ")]))
                                                    )
                                                ]),
                                                Token(
                                                    TriviaList(),
                                                    SyntaxKind.VerticalBarToken,
                                                    TriviaList([EnvironmentNewline]))),
                                            TableRow(
                                                Token(
                                                    TriviaList([Whitespace("  ")]),
                                                    SyntaxKind.VerticalBarToken,
                                                    TriviaList([Space])),
                                                SeparatedList<PlainTextSyntax>([
                                                    LiteralText(
                                                        TableLiteral(
                                                            TriviaList(),
                                                            "minimalism",
                                                            TriviaList([Space]))
                                                    )
                                                ]),
                                                Token(
                                                    TriviaList(),
                                                    SyntaxKind.VerticalBarToken,
                                                    TriviaList()))
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
