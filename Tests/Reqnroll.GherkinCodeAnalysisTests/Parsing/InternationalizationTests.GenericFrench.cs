using Reqnroll.CodeAnalysis.Gherkin.Syntax;

namespace Reqnroll.CodeAnalysis.Gherkin.Parsing;

using static SyntaxFactory;

public partial class InternationalizationTests
{
    [Fact]
    public void GenericFrenchScenarioIsRepresentedInTree()
    {
        const string source =
            """
            #language:fr
            Fonctionnalité: i18n support

              Scénario: Support des caractères spéciaux
               
            """;

        var tree = GherkinSyntaxTree.ParseText(source);

        tree.GetRoot().Should().BeEquivalentTo(
            GherkinDocument(
                Feature(
                    Token(
                        TriviaList([
                            Trivia(
                                DirectiveCommentTrivia(
                                    Token(
                                        TriviaList(),
                                        SyntaxKind.HashToken,
                                        TriviaList()),
                                    DirectiveIdentifier(
                                        TriviaList(),
                                        "language",
                                        TriviaList()),
                                    Token(
                                        TriviaList(),
                                        SyntaxKind.ColonToken,
                                        TriviaList()),
                                    Literal(
                                        TriviaList(),
                                        "fr",
                                        TriviaList([EnvironmentNewLine]))))]),
                        SyntaxKind.FeatureKeyword,
                        "Fonctionnalité",
                        TriviaList()),
                    Token(
                        TriviaList(),
                        SyntaxKind.ColonToken,
                        TriviaList([Space])),
                    LiteralText(
                        Literal(
                            TriviaList(),
                            "i18n support",
                            TriviaList([EnvironmentNewLine]))),
                    examples: List([
                        Example(
                            Token(
                                TriviaList([EnvironmentNewLine, Whitespace("  ")]),
                                SyntaxKind.ExampleKeyword,
                                "Scénario",
                                TriviaList()),
                            Token(
                                TriviaList(),
                                SyntaxKind.ColonToken,
                                TriviaList([Space])),
                            LiteralText(
                                Literal(
                                    TriviaList(),
                                    "Support des caractères spéciaux",
                                    TriviaList([EnvironmentNewLine, Whitespace("  "), EnvironmentNewLine]))
                            ),
                            steps: List<StepSyntax>()
                        )
                    ])
                ),
                Token(
                    TriviaList(),
                    SyntaxKind.EndOfFileToken,
                    TriviaList()))
        );

        tree.ToString().Should().Be(source);
    }
}
