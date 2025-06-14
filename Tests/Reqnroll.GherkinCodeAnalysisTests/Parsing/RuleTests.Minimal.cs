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
                    Token(
                        TriviaList(),
                        SyntaxKind.ColonToken,
                        TriviaList([Space])),
                    LiteralText(
                        Literal(
                            TriviaList(),
                            "Rule support",
                            TriviaList([EnvironmentNewline]))),
                    rules: List([
                        Rule(
                            Token(
                                TriviaList([EnvironmentNewline, Whitespace("  ")]),
                                SyntaxKind.RuleKeyword,
                                "Rule",
                                TriviaList()),
                            Token(
                                TriviaList(),
                                SyntaxKind.ColonToken,
                                TriviaList([Space])),
                            LiteralText(
                                Literal(
                                    TriviaList(),
                                    "A rule",
                                    TriviaList([EnvironmentNewline]))),
                            scenarios: List([
                                Scenario(
                                    Token(
                                        TriviaList([Whitespace("    ")]),
                                        SyntaxKind.ScenarioKeyword,
                                        "Scenario",
                                        TriviaList()),
                                    Token(
                                        TriviaList(),
                                        SyntaxKind.ColonToken,
                                        TriviaList([Space])),
                                    LiteralText(
                                        Literal(
                                            TriviaList(),
                                            "scenario in a rule",
                                            TriviaList([EnvironmentNewline]))),
                                    steps: List([
                                        Step(
                                            Token(
                                                TriviaList([Whitespace("      ")]),
                                                SyntaxKind.GivenKeyword,
                                                "Given",
                                                TriviaList([Space])),
                                            LiteralText(
                                                TokenList([
                                                    Token(
                                                        TriviaList(),
                                                        SyntaxKind.LiteralToken,
                                                        "the minimalism",
                                                        TriviaList([EnvironmentNewline]))
                                                ])))
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
