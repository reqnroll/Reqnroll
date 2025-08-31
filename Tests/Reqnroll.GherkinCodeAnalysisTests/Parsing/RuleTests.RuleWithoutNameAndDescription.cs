using Reqnroll.CodeAnalysis.Gherkin.Syntax;

namespace Reqnroll.CodeAnalysis.Gherkin.Parsing;

using static SyntaxFactory;

public partial class RuleTests
{
    [Fact]
    public void RuleWithoutNameAndDescriptionIsRepresentedInTree()
    {
        // Taken from good/rule_without_name_and_description.feature
        const string source =
            """
            Feature:

              Rule:
              Scenario:
                Given text

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
                    MissingToken(
                        TriviaList(),
                        SyntaxKind.NameToken,
                        TriviaList([EnvironmentNewLine])),
                    rules: List([
                        Rule(
                            Token(
                                TriviaList([EnvironmentNewLine, Whitespace("  ")]),
                                SyntaxKind.RuleKeyword,
                                "Rule",
                                TriviaList()),
                            ColonWithSpace,
                            MissingToken(
                                TriviaList(),
                                SyntaxKind.NameToken,
                                TriviaList([EnvironmentNewLine])),
                            examples: List([
                                Example(
                                    Token(
                                        TriviaList([Whitespace("  ")]),
                                        SyntaxKind.ExampleKeyword,
                                        "Scenario",
                                        TriviaList()),
                                    ColonWithSpace,
                                    MissingToken(
                                        TriviaList(),
                                        SyntaxKind.NameToken,
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
                                                "text",
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
