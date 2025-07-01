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
                                LanguageCommentTrivia(
                                    Token(TriviaList(), SyntaxKind.HashToken, TriviaList()),
                                    Token(TriviaList(), SyntaxKind.LanguageKeyword, "language", TriviaList()),
                                    Token(TriviaList(), SyntaxKind.ColonToken, TriviaList()),
                                    Token(TriviaList(), SyntaxKind.IdentifierToken, "ht", TriviaList([EnvironmentNewline])))),
                            EnvironmentNewline
                        ]),
                        SyntaxKind.FeatureKeyword,
                        "Karakteristik",
                        TriviaList()),
                    Token(
                        TriviaList(),
                        SyntaxKind.ColonToken,
                        TriviaList([Space])),
                    LiteralText(
                        Literal(
                            TriviaList(),
                            "Keywords can be a prefix of another",
                            TriviaList([EnvironmentNewline]))),
                    description: LiteralText(
                        TokenList([
                            Literal(
                                TriviaList([Whitespace("  ")]),
                                "Some times keywords are a prefix of another keyword.",
                                TriviaList([EnvironmentNewline])),
                            Literal(
                                TriviaList([Whitespace("  ")]),
                                "In this scenario the parser should prefer the longest keyword.",
                                TriviaList([EnvironmentNewline, EnvironmentNewline]))
                        ])),
                    scenarios: List([
                        Scenario(
                            Token(
                                TriviaList([Whitespace("  ")]),
                                SyntaxKind.ScenarioKeyword,
                                "Senaryo",
                                TriviaList()),
                            Token(
                                TriviaList(),
                                SyntaxKind.ColonToken,
                                TriviaList([Space])),
                            LiteralText(
                                Literal(
                                    TriviaList(),
                                    "Erasing agent memory",
                                    TriviaList([EnvironmentNewline]))),
                            steps: List([
                                Step(
                                    Token(
                                        TriviaList([Whitespace("    ")]),
                                        SyntaxKind.GivenKeyword,
                                        "Sipoze ke",
                                        TriviaList([Space])),
                                    LiteralText(
                                        Literal(
                                            TriviaList(),
                                            "there is agent J",
                                            TriviaList([EnvironmentNewline])))),
                                Step(
                                    Token(
                                        TriviaList([Whitespace("    ")]),
                                        SyntaxKind.AndKeyword,
                                        "Ak",
                                        TriviaList([Space])),
                                    LiteralText(
                                        Literal(
                                            TriviaList(),
                                            "there is agent K",
                                            TriviaList([EnvironmentNewline])))),
                                Step(
                                    Token(
                                        TriviaList([Whitespace("    ")]),
                                        SyntaxKind.WhenKeyword,
                                        "Le",
                                        TriviaList([Space])),
                                    LiteralText(
                                        Literal(
                                            TriviaList(),
                                            "I erase agent K's memory",
                                            TriviaList([EnvironmentNewline])))),
                                Step(
                                    Token(
                                        TriviaList([Whitespace("    ")]),
                                        SyntaxKind.ThenKeyword,
                                        "Le sa a",
                                        TriviaList([Space])),
                                    LiteralText(
                                        Literal(
                                            TriviaList(),
                                            "there should be agent J",
                                            TriviaList([EnvironmentNewline])))),
                                Step(
                                    Token(
                                        TriviaList([Whitespace("    ")]),
                                        SyntaxKind.ButKeyword,
                                        "Men",
                                        TriviaList([Space])),
                                    LiteralText(
                                        Literal(
                                            TriviaList(),
                                            "there should not be agent K",
                                            TriviaList([EnvironmentNewline]))))
                            ]))
                    ])),
                Token(
                    TriviaList(),
                    SyntaxKind.EndOfFileToken,
                    TriviaList())));

        tree.ToString().Should().Be(source);
    }
}
