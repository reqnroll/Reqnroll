using Reqnroll.CodeAnalysis.Gherkin.Syntax;

namespace Reqnroll.CodeAnalysis.Gherkin.Parsing;

using static SyntaxFactory;

public partial class ScenarioTests
{
    [Fact]
    public void MinimalExampleIsRepresentedInTree()
    {
        // Taken from good/minimal-example.feature
        const string source =
            """
            Feature: Minimal

              Example: minimalistic
                Given the minimalism

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
                        "Minimal",
                        TriviaList([EnvironmentNewLine])),
                    examples: List([
                        Example(
                            Token(
                                TriviaList([EnvironmentNewLine, Whitespace("  ")]),
                                SyntaxKind.ExampleKeyword,
                                "Example",
                                TriviaList()),
                            ColonWithSpace,
                            Name(
                                TriviaList(),
                                "minimalistic",
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
                                        "the minimalism",
                                        TriviaList([EnvironmentNewLine])))
                            ]))
                    ])),
                Token(
                    TriviaList(),
                    SyntaxKind.EndOfFileToken,
                    TriviaList())));

        tree.ToString().Should().Be(source);
    }
}
