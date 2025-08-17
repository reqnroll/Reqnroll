using Reqnroll.CodeAnalysis.Gherkin.Syntax;

namespace Reqnroll.CodeAnalysis.Gherkin.Parsing;

using static SyntaxFactory;

public partial class ScenarioTests
{
    [Fact]
    public void IncompleteScenarioIsRepresentedInTree()
    {
        // Taken from good/incomplete_scenario.feature
        const string source =
            """
            Feature: Incomplete scenarios

              Background: Adding a background won't make a pickle
                * a step

              Scenario: no steps

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
                            "Incomplete scenarios",
                            TriviaList([EnvironmentNewLine]))),
                    background: Background(
                        Token(
                            TriviaList([EnvironmentNewLine, Whitespace("  ")]),
                            SyntaxKind.BackgroundKeyword,
                            "Background",
                            TriviaList()),
                        Token(
                            TriviaList(),
                            SyntaxKind.ColonToken,
                            TriviaList([Space])),
                        LiteralText(
                            Literal(
                                TriviaList(),
                                "Adding a background won't make a pickle",
                                TriviaList([EnvironmentNewLine]))),
                        steps: List([
                            Step(
                                Token(
                                    TriviaList([Whitespace("    ")]),
                                    SyntaxKind.WildcardStepKeyword,
                                    TriviaList([Space])),
                                LiteralText(
                                    TokenList([
                                        Token(
                                            TriviaList(),
                                            SyntaxKind.LiteralToken,
                                            "a step",
                                            TriviaList([EnvironmentNewLine]))
                                    ])))
                        ])),
                    examples: List([
                        Example(
                            Token(
                                TriviaList([EnvironmentNewLine, Whitespace("  ")]),
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
                                    "no steps",
                                    TriviaList([EnvironmentNewLine]))),
                            steps: List<StepSyntax>()
                        )
                    ])
                ),
                Token(
                    TriviaList(),
                    SyntaxKind.EndOfFileToken,
                    TriviaList()))
        );

        tree.ToString().Should().Be(source);
    }
}
