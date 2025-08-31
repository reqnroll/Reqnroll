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
                                    Token(
                                        TriviaList(),
                                        SyntaxKind.DirectiveIdentifierToken,
                                        "language",
                                        TriviaList()),
                                    Token(
                                        TriviaList(),
                                        SyntaxKind.ColonToken,
                                        TriviaList()),
                                    Token(
                                        TriviaList(),
                                        SyntaxKind.DirectiveValueToken,
                                        "fr",
                                        TriviaList([EnvironmentNewLine]))))]),
                        SyntaxKind.FeatureKeyword,
                        "Fonctionnalit�",
                        TriviaList()),
                    ColonWithSpace,
                    Name(
                        TriviaList(),
                        "i18n support",
                        TriviaList([EnvironmentNewLine])),
                    examples: List([
                        Example(
                            Token(
                                TriviaList([EnvironmentNewLine, Whitespace("  ")]),
                                SyntaxKind.ExampleKeyword,
                                "Sc�nario",
                                TriviaList()),
                            ColonWithSpace,
                            Name(
                                TriviaList(),
                                "Support des caract�res sp�ciaux",
                                TriviaList([EnvironmentNewLine, Whitespace("  "), EnvironmentNewLine])),
                            steps: default)
                    ])),
                Token(
                    TriviaList(),
                    SyntaxKind.EndOfFileToken,
                    TriviaList())));

        tree.ToString().Should().Be(source);
    }
}
