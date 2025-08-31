using Reqnroll.CodeAnalysis.Gherkin.Syntax;

namespace Reqnroll.CodeAnalysis.Gherkin.Parsing;

using static SyntaxFactory;

public partial class BackgroundTests
{
    [Fact]
    public void DescriptionIsRepresentedInTree()
    {
        // Taken from good/incomplete_background_2.feature
        const string source =
            """
            Feature: Incomplete backgrounds, Part 2

              Background: just a description
                A short description

              Scenario: still pickles up
                * a step

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
                    Name(
                        TriviaList(),
                        "Incomplete backgrounds, Part 2",
                        TriviaList([EnvironmentNewLine])),
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
                        Name(
                            TriviaList(),
                            "just a description",
                            TriviaList([EnvironmentNewLine])),
                        description: Description(
                            TokenList([
                                DescriptionText(
                                    TriviaList([Whitespace("    ")]),
                                    "A short description",
                                    TriviaList([EnvironmentNewLine]))
                            ])
                        ),
                        steps: List<StepSyntax>()
                    ),
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
                            Name(
                                TriviaList(),
                                "still pickles up",
                                TriviaList([EnvironmentNewLine])),
                            steps: List([
                                Step(
                                    Token(
                                        TriviaList([Whitespace("    ")]),
                                        SyntaxKind.WildcardStepKeyword,
                                        "*",
                                        TriviaList([Space])),
                                    StepText(
                                        TriviaList(),
                                        "a step",
                                        TriviaList([EnvironmentNewLine])))
                            ])
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
