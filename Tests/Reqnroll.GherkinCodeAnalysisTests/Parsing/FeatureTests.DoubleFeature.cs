namespace Reqnroll.CodeAnalysis.Gherkin.Parsing;

using Reqnroll.CodeAnalysis.Gherkin.Syntax;

using static SyntaxFactory;

public partial class FeatureTests
{
    [Fact]
    public void DoubleFeatureDeclarationIsRepresentedInTreeAsFeatureDescription()
    {
        const string source =
            """
                Feature: Guess the word

                Feature: Guess the word again 
            
            """;

        var tree = GherkinSyntaxTree.ParseText(source);

        tree.GetRoot().Should().BeEquivalentTo(
            GherkinDocument(
                Feature(
                    default,
                    Token(
                        TriviaList([Whitespace("    ")]),
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
                            "Guess the word",
                            TriviaList([CarriageReturnLineFeed]))),
                    LiteralText(
                        TokenList([
                            Literal(
                                TriviaList([ CarriageReturnLineFeed, Whitespace("    ") ]),
                                "Feature: Guess the word again",
                                TriviaList([ Space, CarriageReturnLineFeed ]))
                        ]))),
                Token(
                    TriviaList(),
                    SyntaxKind.EndOfFileToken,
                    TriviaList())));

        tree.ToString().Should().Be(source);
    }
}
