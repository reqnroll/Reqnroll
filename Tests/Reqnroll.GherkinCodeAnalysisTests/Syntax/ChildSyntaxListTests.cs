namespace Reqnroll.CodeAnalysis.Gherkin.Syntax;

using static SyntaxFactory;

public class ChildSyntaxListTests
{
    [Fact]
    public void CanEnumerateSparselyPopulatedChildren()
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

        var children = feature.ChildNodesAndTokens();

        children.Should().HaveCount(5);
    }

    [Fact]
    public void CanWalkWholeTreeWithSyntaxWalker()
    {
        var document = GherkinDocument(
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

        var walker = new CountingSyntaxWalker();

        walker.Visit(document);

        walker.NodeCount.Should().Be(5);
        walker.TokenCount.Should().Be(12);
        walker.TriviaCount.Should().Be(13);
    }
}

public class CountingSyntaxWalker : SyntaxWalker
{
    public int NodeCount { get; private set; }

    public int TokenCount { get; private set; }

    public int TriviaCount { get; private set; }

    public override void Visit(SyntaxNode node)
    {
        NodeCount++;
        base.Visit(node);
    }

    public override void Visit(SyntaxToken token)
    {
        TokenCount++;
        base.Visit(token);
    }

    public override void Visit(SyntaxTrivia trivia)
    {
        TriviaCount++;
        base.Visit(trivia);
    }
}
