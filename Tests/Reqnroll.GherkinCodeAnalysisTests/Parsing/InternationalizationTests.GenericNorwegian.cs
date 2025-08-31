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
                                        "no",
                                        TriviaList([EnvironmentNewLine]))))]),
                        SyntaxKind.FeatureKeyword,
                        "Egenskap",
                        TriviaList()),
                    ColonWithSpace,
                    Name(
                        TriviaList(),
                        "i18n støtte",
                        TriviaList([EnvironmentNewLine])),
                    examples: List([
                        Example(
                            Token(
                                TriviaList([EnvironmentNewLine, Whitespace("  ")]),
                                SyntaxKind.ExampleKeyword,
                                "Scenario",
                                TriviaList()),
                            ColonWithSpace,
                                Name(
                                    TriviaList(),
                                    "Støtte for spesialtegn",
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
