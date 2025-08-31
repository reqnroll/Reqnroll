using Reqnroll.CodeAnalysis.Gherkin.Syntax;

namespace Reqnroll.CodeAnalysis.Gherkin.Parsing;

using static SyntaxFactory;

public partial class RuleTests
{
    [Fact]
    public void RuleWithBackgroundIsRepresentedInTree()
    {
        const string source =
            """
            Feature: Rule with background

              Rule: A rule
                Background: rule background
                  Given a rule background step

                Scenario: scenario in a rule
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
                        "Rule with background",
                        TriviaList([EnvironmentNewLine])),
                    rules: List([
                        Rule(
                            Token(
                                TriviaList([EnvironmentNewLine, Whitespace("  ")]),
                                SyntaxKind.RuleKeyword,
                                "Rule",
                                TriviaList()),
                            ColonWithSpace,
                            Name(
                                TriviaList(),
                                "A rule",
                                TriviaList([EnvironmentNewLine])),
                            background: Background(
                                Token(
                                    TriviaList([Whitespace("    ")]),
                                    SyntaxKind.BackgroundKeyword,
                                    "Background",
                                    TriviaList()),
                                ColonWithSpace,
                                Name(
                                    TriviaList(),
                                    "rule background",
                                    TriviaList([EnvironmentNewLine])),
                                steps: List([
                                    Step(
                                        Token(
                                            TriviaList([Whitespace("      ")]),
                                            SyntaxKind.ContextStepKeyword,
                                            "Given",
                                            TriviaList([Space])),
                                        StepText(
                                            TriviaList(),
                                            "a rule background step",
                                            TriviaList([EnvironmentNewLine])))
                                ])),
                            examples: List([
                                Example(
                                    Token(
                                        TriviaList([EnvironmentNewLine, Whitespace("    ")]),
                                        SyntaxKind.ExampleKeyword,
                                        "Scenario",
                                        TriviaList()),
                                    ColonWithSpace,
                                    Name(
                                        TriviaList(),
                                        "scenario in a rule",
                                        TriviaList([EnvironmentNewLine])),
                                    steps: List([
                                        Step(
                                            Token(
                                                TriviaList([Whitespace("      ")]),
                                                SyntaxKind.ContextStepKeyword,
                                                "Given",
                                                TriviaList([Space])),
                                            StepText(
                                                TriviaList(),
                                                "the minimalism",
                                                TriviaList([EnvironmentNewLine])))
                                    ]))
                            ]))
                    ])),
                Token(
                    TriviaList(),
                    SyntaxKind.EndOfFileToken,
                    TriviaList())));

        tree.ToString().Should().Be(source);
    }
}
