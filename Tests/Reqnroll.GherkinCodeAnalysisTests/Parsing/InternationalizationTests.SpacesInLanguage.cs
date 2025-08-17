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
                                DirectiveCommentTrivia(
                                    Token(TriviaList([Whitespace("  ")]), SyntaxKind.HashToken, TriviaList([Whitespace("  ")])),
                                    DirectiveIdentifier(TriviaList(), "language", TriviaList([Whitespace("  ")])),
                                    Token(TriviaList(), SyntaxKind.ColonToken, TriviaList([Whitespace("   ")])),
                                    Literal(TriviaList(), "en-lol", TriviaList([EnvironmentNewLine])))),
                            EnvironmentNewLine
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
                            TriviaList([EnvironmentNewLine]))),
                    examples: List<ExampleSyntax>()),
                Token(
                    TriviaList(),
                    SyntaxKind.EndOfFileToken,
                    TriviaList())));

        tree.ToString().Should().Be(source);
    }
}
