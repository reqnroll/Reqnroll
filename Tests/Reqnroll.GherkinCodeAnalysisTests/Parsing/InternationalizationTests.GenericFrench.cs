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
            Fonctionnalit�: i18n support

              Sc�nario: Support des caract�res sp�ciaux
               
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
                        "Fonctionnalit�",
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
                    members: List([
                        Example(
                            Token(
                                TriviaList([EnvironmentNewLine, Whitespace("  ")]),
                                SyntaxKind.ExampleKeyword,
                                "Sc�nario",
                                TriviaList()),
                            Token(
                                TriviaList(),
                                SyntaxKind.ColonToken,
                                TriviaList([Space])),
                            LiteralText(
                                Literal(
                                    TriviaList(),
                                    "Support des caract�res sp�ciaux",
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
