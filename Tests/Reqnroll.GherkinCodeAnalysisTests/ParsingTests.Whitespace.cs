using FluentAssertions;
using Reqnroll.CodeAnalysis.Gherkin.Syntax;

namespace Reqnroll.CodeAnalysis.Gherkin;

using static SyntaxFactory;

public partial class ParsingTests
{
    [Fact]
    public void LeadingWhitespaceIsRepresentedInTree()
    {
        var source =
            """
                
                Feature: Guess the word
            
            """;

        var tree = GherkinSyntaxTree.ParseText(source);

        tree.GetRoot().Should().BeEquivalentTo(
            FeatureFile(
                FeatureDeclaration(
                    Token(
                        TriviaList([Whitespace("    "), CarriageReturnLineFeed, Whitespace("    ")]),
                        SyntaxKind.FeatureKeyword,
                        "Feature",
                        TriviaList()),
                    Token(
                        TriviaList(),
                        SyntaxKind.ColonToken,
                        TriviaList([Space])),
                    Identifier(
                        TriviaList(),
                        "Guess the word",
                        TriviaList([CarriageReturnLineFeed]))),
                Token(
                    TriviaList(),
                    SyntaxKind.EndOfFileToken,
                    TriviaList())));

        tree.GetRoot().ToFullString().Should().Be(source);
    }

    [Fact]
    public void TrailingWhitespaceisRepresentedInTree()
    {
        var source =
            """
                Feature: Guess the word
                
            
            """;

        var tree = GherkinSyntaxTree.ParseText(source);

        tree.GetRoot().Should().BeEquivalentTo(
            FeatureFile(
                FeatureDeclaration(
                    Token(
                        TriviaList([Whitespace("    ")]),
                        SyntaxKind.FeatureKeyword,
                        "Feature",
                        TriviaList()),
                    Token(
                        TriviaList(),
                        SyntaxKind.ColonToken,
                        TriviaList([Space])),
                    Identifier(
                        TriviaList(),
                        "Guess the word",
                        TriviaList([CarriageReturnLineFeed]))),
                Token(
                    TriviaList([Whitespace("    "), CarriageReturnLineFeed]),
                    SyntaxKind.EndOfFileToken,
                    TriviaList())));

        tree.GetRoot().ToFullString().Should().Be(source);
    }
}
