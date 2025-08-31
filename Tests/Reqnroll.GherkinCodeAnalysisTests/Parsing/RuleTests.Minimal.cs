using Reqnroll.CodeAnalysis.Gherkin.Syntax;

namespace Reqnroll.CodeAnalysis.Gherkin.Parsing;

using static SyntaxFactory;

public partial class RuleTests
{
    [Fact]
    public void RuleIsRepresentedInTree()
    {
        const string source =
            """
            Feature: Rule support

              Rule: A rule
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
                        "Rule support",
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
                            examples: List([
                                Example(
                                    Token(
                                        TriviaList([Whitespace("    ")]),
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
