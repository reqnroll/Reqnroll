using Reqnroll.CodeAnalysis.Gherkin.Syntax;

namespace Reqnroll.CodeAnalysis.Gherkin.Parsing;

using static SyntaxFactory;

public partial class InternationalizationTests
{
    [Fact]
    public void PrefixedKeywordsIsRepresentedInTree()
    {
        // Taken from good/prefixed-keywords.feature
        const string source =
            """
            # language: ht
            Karakteristik: Keywords can be a prefix of another
              Some times keywords are a prefix of another keyword.
              In this scenario the parser should prefer the longest keyword.

              Senaryo: Erasing agent memory
                Sipoze ke there is agent J
                Ak there is agent K
                Le I erase agent K's memory
                Le sa a there should be agent J
                Men there should not be agent K

            """;

        var tree = GherkinSyntaxTree.ParseText(source);

        tree.GetRoot().Should().BeEquivalentTo(
            GherkinDocument(
                Feature(
                    Token(
                        TriviaList([
                            Trivia(
                                DirectiveCommentTrivia(
                                    Token(TriviaList(), SyntaxKind.HashToken, TriviaList()),
                                    Token(TriviaList(), SyntaxKind.DirectiveIdentifierToken, "language", TriviaList()),
                                    Token(TriviaList(), SyntaxKind.ColonToken, TriviaList()),
                                    Literal(TriviaList(), SyntaxKind.DirectiveValueToken, "ht", "ht", TriviaList([EnvironmentNewLine])))),
                            EnvironmentNewLine
                        ]),
                        SyntaxKind.FeatureKeyword,
                        "Karakteristik",
                        TriviaList()),
                    ColonWithSpace,
                    Name(
                        TriviaList(),
                        "Keywords can be a prefix of another",
                        TriviaList([EnvironmentNewLine])),
                    description: Description(
                        TokenList([
                            DescriptionText(
                                TriviaList([Whitespace("  ")]),
                                "Some times keywords are a prefix of another keyword.",
                                TriviaList([EnvironmentNewLine])),
                            DescriptionText(
                                TriviaList([Whitespace("  ")]),
                                "In this scenario the parser should prefer the longest keyword.",
                                TriviaList([EnvironmentNewLine, EnvironmentNewLine]))
                        ])),
                    examples: List([
                        Example(
                            Token(
                                TriviaList([Whitespace("  ")]),
                                SyntaxKind.ExampleKeyword,
                                "Senaryo",
                                TriviaList()),
                            ColonWithSpace,
                            Name(
                                TriviaList(),
                                "Erasing agent memory",
                                TriviaList([EnvironmentNewLine])),
                            steps: List([
                                Step(
                                    Token(
                                        TriviaList([Whitespace("    ")]),
                                        SyntaxKind.ContextStepKeyword,
                                        "Sipoze ke",
                                        TriviaList([Space])),
                                    StepText(
                                        TriviaList(),
                                        "there is agent J",
                                        TriviaList([EnvironmentNewLine]))),
                                Step(
                                    Token(
                                        TriviaList([Whitespace("    ")]),
                                        SyntaxKind.ConjunctionStepKeyword,
                                        "Ak",
                                        TriviaList([Space])),
                                    StepText(
                                        TriviaList(),
                                        "there is agent K",
                                        TriviaList([EnvironmentNewLine]))),
                                Step(
                                    Token(
                                        TriviaList([Whitespace("    ")]),
                                        SyntaxKind.ActionStepKeyword,
                                        "Le",
                                        TriviaList([Space])),
                                    StepText(
                                        TriviaList(),
                                        "I erase agent K's memory",
                                        TriviaList([EnvironmentNewLine]))),
                                Step(
                                    Token(
                                        TriviaList([Whitespace("    ")]),
                                        SyntaxKind.OutcomeStepKeyword,
                                        "Le sa a",
                                        TriviaList([Space])),
                                    StepText(
                                        TriviaList(),
                                        "there should be agent J",
                                        TriviaList([EnvironmentNewLine]))),
                                Step(
                                    Token(
                                        TriviaList([Whitespace("    ")]),
                                        SyntaxKind.ConjunctionStepKeyword,
                                        "Men",
                                        TriviaList([Space])),
                                    StepText(
                                        TriviaList(),
                                        "there should not be agent K",
                                        TriviaList([EnvironmentNewLine])))
                            ]))
                    ])),
                Token(
                    TriviaList(),
                    SyntaxKind.EndOfFileToken,
                    TriviaList())));

        tree.ToString().Should().Be(source);
    }
}
