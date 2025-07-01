using Reqnroll.CodeAnalysis.Gherkin.Syntax;

namespace Reqnroll.CodeAnalysis.Gherkin.Parsing;

using static SyntaxFactory;

public partial class InternationalizationTests
{
    [Fact]
    public void GenericNorwegianIsRepresentedInTree()
    {
        const string source =
            """
            #language:no
            Egenskap: i18n støtte

              Scenario: Støtte for spesialtegn
               
            """;

        var tree = GherkinSyntaxTree.ParseText(source);

        tree.GetRoot().Should().BeEquivalentTo(
            GherkinDocument(
                Feature(
                    Token(
                        TriviaList([
                            Trivia(
                                LanguageCommentTrivia(
                                    Token(
                                        TriviaList(),
                                        SyntaxKind.HashToken,
                                        TriviaList()),
                                    Token(
                                        TriviaList(),
                                        SyntaxKind.LanguageKeyword,
                                        TriviaList()),
                                    Token(
                                        TriviaList(),
                                        SyntaxKind.ColonToken,
                                        TriviaList()),
                                    Identifier(
                                        TriviaList(),
                                        "no",
                                        TriviaList([EnvironmentNewline]))))]),
                        SyntaxKind.FeatureKeyword,
                        "Egenskap",
                        TriviaList()),
                    Token(
                        TriviaList(),
                        SyntaxKind.ColonToken,
                        TriviaList([Space])),
                    LiteralText(
                        Literal(
                            TriviaList(),
                            "i18n støtte",
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
                                    "Støtte for spesialtegn",
                                    TriviaList([EnvironmentNewline, Whitespace("  "), EnvironmentNewline]))
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
