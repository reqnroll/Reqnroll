namespace Reqnroll.CodeAnalysis.Gherkin.Syntax;

using Reqnroll.CodeAnalysis.Gherkin.Assertions;
using static SyntaxFactory;

/// <summary>
/// Although we can't directly test the behaviour of <see cref="SyntaxNode"/> because it's the abstract base for all
/// concrete syntax nodes, there are some things we'd like to validate in isolation using some simple implementations.
/// </summary>
public class SyntaxNodeTests
{
    [Fact]
    public void GetText_ReturnsSourceTextContainingNodeSyntax()
    {
        var node = TableCell(TableLiteral("test"));

        var text = node.GetText();

        text.ToString().Should().Be("test");
    }

    [Fact]
    public void GetTrailingTrivia_ReturnsLastNodesTrivia()
    {
        var feature = Feature(
            Token(
                TriviaList(),
                SyntaxKind.FeatureKeyword,
                "Feature",
                TriviaList()),
            ColonWithSpace,
            Name(
                TriviaList(),
                "Incomplete scenarios",
                TriviaList([EnvironmentNewLine])));

        feature.GetTrailingTrivia().Should().BeEquivalentTo(
            TriviaList([EnvironmentNewLine]), options => options.IgnoringSyntaxPositions());
    }

    [Fact]
    public void GetTrailingTrivia_ReturnsLastNodesTriviaForSingleNodeLists()
    {
        var feature = Feature(
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
            ]));

        feature.GetTrailingTrivia().Should().BeEquivalentTo(
            TriviaList([EnvironmentNewLine]), options => options.IgnoringSyntaxPositions());
    }
}
