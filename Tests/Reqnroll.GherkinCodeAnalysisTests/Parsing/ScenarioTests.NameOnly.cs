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

        var expected = GherkinDocument(
            Feature(
                Token(
                    TriviaList(),
                    SyntaxKind.FeatureKeyword,
                    "Feature",
                    TriviaList()),
                ColonWithSpace,
                Name(
                    TriviaList(),
                    "Incomplete scenarios",
                    TriviaList([EnvironmentNewLine])),
                background: Background(
                    Token(
                        TriviaList([EnvironmentNewLine, Whitespace("  ")]),
                        SyntaxKind.BackgroundKeyword,
                        "Background",
                        TriviaList()),
                    ColonWithSpace,
                    Name(
                        TriviaList(),
                        "Adding a background won't make a pickle",
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
                            "no steps",
                            TriviaList([EnvironmentNewLine])),
                        steps: default)
                ])),
            Token(
                TriviaList(),
                SyntaxKind.EndOfFileToken,
                TriviaList()));

        var actual = (GherkinDocumentSyntax)tree.GetRoot();

        var actualExample = actual.FeatureDeclaration!.Examples.Single();
        var expectedExample = expected.FeatureDeclaration!.Examples.Single();

        var actualTrailing = actualExample.GetTrailingTrivia();
        var expectedTrailing = expectedExample.GetTrailingTrivia();

        actualTrailing.Should().BeEquivalentTo(expectedTrailing);

        actualExample.Should().BeEquivalentTo(expectedExample);

        tree.GetRoot().Should().BeEquivalentTo(expected);

        tree.ToString().Should().Be(source);
    }
}
