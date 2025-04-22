namespace Reqnroll.CodeAnalysis.Gherkin.Parsing;

using Microsoft.CodeAnalysis;
using Reqnroll.CodeAnalysis.Gherkin.Syntax;
using static SyntaxFactory;

public partial class BackgroundTests
{
    [Fact]
    public void BackgroundIsRepresentedInTree()
    {
        const string source =
            """
            Feature: Background

              Background: a simple background
                Given the minimalism inside a background
              
              
              Scenario: minimalistic
                Given the minimalism
              
              Scenario: also minimalistic
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
                    Identifier(
                        TriviaList(),
                        "Background",
                        TriviaList([EnvironmentNewline])),
                    List([
                        Background(
                            Token(
                                TriviaList([EnvironmentNewline, Whitespace("  ")]),
                                SyntaxKind.BackgroundKeyword,
                                "Background",
                                TriviaList()),
                            Token(
                                TriviaList(),
                                SyntaxKind.ColonToken,
                                TriviaList([Space])),
                            Identifier(
                                TriviaList(),
                                "a simple background",
                                TriviaList([EnvironmentNewline])),
                            List([
                                Step(
                                    Token(
                                        TriviaList([Whitespace("    ")]),
                                        SyntaxKind.GivenKeyword,
                                        "Given",
                                        TriviaList()),
                                    TokenList(
                                        Literal(
                                            TriviaList([Space]),
                                            "the minimalism inside a background",
                                            TriviaList([EnvironmentNewline])))),
                            ]))
                    ])),
                Token(
                    TriviaList(),
                    SyntaxKind.EndOfFileToken,
                    TriviaList())));

        tree.ToString().Should().Be(source);
    }
}
