using Reqnroll.CodeAnalysis.Gherkin.Syntax;

namespace Reqnroll.CodeAnalysis.Gherkin.Parsing;

using static SyntaxFactory;

public partial class InternationalizationTests
{
    [Fact]
    public void SpacesInLanguageCommentIsRepresentedInTree()
    {
        // Taken from good/spaces_in_language.feature
        const string source =
            """
              #  language  :   en-lol
            OH HAI: STUFFING

            """;

        var tree = GherkinSyntaxTree.ParseText(source);

        tree.GetRoot().Should().BeEquivalentTo(
            GherkinDocument(
                Feature(
                    Token(
                        TriviaList([
                            Trivia(
                                LanguageCommentTrivia(
                                    Token(TriviaList([Whitespace("  ")]), SyntaxKind.HashToken, TriviaList()),
                                    Token(TriviaList([Whitespace("  ")]), SyntaxKind.LanguageKeyword, "language", TriviaList([Whitespace("  ")])),
                                    Token(TriviaList(), SyntaxKind.ColonToken, TriviaList([Whitespace("   ")])),
                                    Token(TriviaList(), SyntaxKind.IdentifierToken, "en-lol", TriviaList([EnvironmentNewline])))),
                            EnvironmentNewline
                        ]),
                        SyntaxKind.FeatureKeyword,
                        "OH HAI",
                        TriviaList()),
                    Token(
                        TriviaList(),
                        SyntaxKind.ColonToken,
                        TriviaList([Space])),
                    LiteralText(
                        Literal(
                            TriviaList(),
                            "STUFFING",
                            TriviaList([EnvironmentNewline]))),
                    scenarios: List<ScenarioSyntax>()),
                Token(
                    TriviaList(),
                    SyntaxKind.EndOfFileToken,
                    TriviaList())));

        tree.ToString().Should().Be(source);
    }
}
