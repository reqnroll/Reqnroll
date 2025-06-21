using Reqnroll.CodeAnalysis.Gherkin.Syntax;

namespace Reqnroll.CodeAnalysis.Gherkin.Parsing;

using static SyntaxFactory;

public partial class FeatureTests
{
    [Fact]
    public void LanguageFeatureIsRepresentedInTree()
    {
        // Taken from good/language.feature
        const string source =
            """
            #language:en

            Feature: Explicit language specification

              Scenario: minimalistic
                Given the minimalism

            """;

        var tree = GherkinSyntaxTree.ParseText(source);

        tree.GetRoot().Should().BeEquivalentTo(
            GherkinDocument(
                Feature(
                    Token(
                        TriviaList([
                            Trivia(
                                DirectiveCommentTrivia(
                                    Token(TriviaList(), SyntaxKind.HashToken, TriviaList()),
                                    DirectiveIdentifier(TriviaList(), "language", TriviaList()),
                                    Token(TriviaList(), SyntaxKind.ColonToken, TriviaList()),
                                    Literal(TriviaList(), "en", TriviaList([EnvironmentNewLine])))),
                            EnvironmentNewLine
                        ]),
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
                            "Explicit language specification",
                            TriviaList([EnvironmentNewLine]))),
                    scenarios: List([
                        Scenario(
                            Token(
                                TriviaList([EnvironmentNewLine, Whitespace("  ")]),
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
                                    "minimalistic",
                                    TriviaList([EnvironmentNewLine]))),
                            steps: List([
                                Step(
                                    Token(
                                        TriviaList([Whitespace("    ")]),
                                        SyntaxKind.GivenKeyword,
                                        "Given",
                                        TriviaList([Space])),
                                    LiteralText(
                                        Literal(
                                            TriviaList(),
                                            "the minimalism",
                                            TriviaList([EnvironmentNewLine]))))
                            ]))
                    ])),
                Token(
                    TriviaList([EnvironmentNewLine]),
                    SyntaxKind.EndOfFileToken,
                    TriviaList())));

        tree.ToString().Should().Be(source);
    }
}
