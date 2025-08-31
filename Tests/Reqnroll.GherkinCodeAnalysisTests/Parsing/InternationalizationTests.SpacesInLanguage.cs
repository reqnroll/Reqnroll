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
                                    Token(TriviaList(), SyntaxKind.DirectiveIdentifierToken, "language", TriviaList([Whitespace("  ")])),
                                    Token(TriviaList(), SyntaxKind.ColonToken, TriviaList([Whitespace("   ")])),
                                    Literal(TriviaList(), SyntaxKind.DirectiveValueToken, "en-lol", "en-lol", TriviaList([EnvironmentNewLine])))),
                            EnvironmentNewLine
                        ]),
                        SyntaxKind.FeatureKeyword,
                        "OH HAI",
                        TriviaList()),
                    ColonWithSpace,
                    Name(
                        TriviaList(),
                        "STUFFING",
                        TriviaList([EnvironmentNewLine])),
                    default),
                Token(
                    TriviaList(),
                    SyntaxKind.EndOfFileToken,
                    TriviaList())));

        tree.ToString().Should().Be(source);
    }
}
