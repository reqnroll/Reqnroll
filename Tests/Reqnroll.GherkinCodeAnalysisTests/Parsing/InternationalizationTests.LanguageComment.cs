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
                                LanguageCommentTrivia(
                                    Token(TriviaList(), SyntaxKind.HashToken, TriviaList()),
                                    Token(TriviaList(), SyntaxKind.LanguageKeyword, "language", TriviaList()),
                                    Token(TriviaList(), SyntaxKind.ColonToken, TriviaList()),
                                    Token(TriviaList(), SyntaxKind.IdentifierToken, "en", TriviaList([EnvironmentNewline])))),
                            EnvironmentNewline
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
                            TriviaList([EnvironmentNewline]))),
                    scenarios: List([
                        Scenario(
                            Token(
                                TriviaList([EnvironmentNewline, Whitespace("  ")]),
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
                                    TriviaList([EnvironmentNewline]))),
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
                                            TriviaList([EnvironmentNewline]))))
                            ]))
                    ])),
                Token(
                    TriviaList(),
                    SyntaxKind.EndOfFileToken,
                    TriviaList())));

        tree.ToString().Should().Be(source);
    }
}
