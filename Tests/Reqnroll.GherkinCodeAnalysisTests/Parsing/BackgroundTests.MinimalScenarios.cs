namespace Reqnroll.CodeAnalysis.Gherkin.Parsing;

using Reqnroll.CodeAnalysis.Gherkin.Syntax;
using static SyntaxFactory;

public partial class BackgroundTests
{
    [Fact]
    public void BackgroundIsRepresentedInTree()
    {
        // Taken from good/background.feature
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
                    LiteralText(
                        Literal(
                            TriviaList(),
                            "Background",
                            TriviaList([EnvironmentNewLine]))),
                    background: Background(
                        Token(
                            TriviaList([EnvironmentNewLine, Whitespace("  ")]),
                            SyntaxKind.BackgroundKeyword,
                            "Background",
                            TriviaList()),
                        Token(
                            TriviaList(),
                            SyntaxKind.ColonToken,
                            TriviaList([Space])),
                        LiteralText(
                            Literal(
                                TriviaList(),
                                "a simple background",
                                TriviaList([EnvironmentNewLine]))),
                        steps: List([
                            Step(
                                Token(
                                    TriviaList([Whitespace("    ")]),
                                    SyntaxKind.ContextStepKeyword,
                                    "Given",
                                    TriviaList([Space])),
                                LiteralText(
                                    TokenList(
                                        Literal(
                                            TriviaList(),
                                            "the minimalism inside a background",
                                            TriviaList([EnvironmentNewLine])))))
                            ])),
                    examples: List([
                        Example(
                            Token(
                                TriviaList([
                                    Whitespace("  "), EnvironmentNewLine,
                                    Whitespace("  "), EnvironmentNewLine,
                                    Whitespace("  ")
                                ]),
                                SyntaxKind.ExampleKeyword,
                                "Scenario",
                                TriviaList()),
                            Token(
                                TriviaList(),
                                SyntaxKind.ColonToken,
                                TriviaList([Space])),
                            LiteralText(
                                Literal(
                                    TriviaList(),
                                    "minimalistic",
                                    TriviaList([EnvironmentNewLine]))),
                            steps : List([
                                Step(
                                    Token(
                                        TriviaList([Whitespace("    ")]),
                                        SyntaxKind.ContextStepKeyword,
                                        "Given",
                                        TriviaList([Space])),
                                    LiteralText(
                                        TokenList([
                                            Literal(
                                                TriviaList(),
                                                "the minimalism",
                                                TriviaList([EnvironmentNewLine]))
                                            ])))
                                ])),
                        Example(
                            Token(
                                TriviaList([
                                    Whitespace("  "), EnvironmentNewLine,
                                    Whitespace("  ")
                                ]),
                                SyntaxKind.ExampleKeyword,
                                "Scenario",
                                TriviaList()),
                            Token(
                                TriviaList(),
                                SyntaxKind.ColonToken,
                                TriviaList([Space])),
                            LiteralText(
                                Literal(
                                    TriviaList(),
                                    "also minimalistic",
                                    TriviaList([EnvironmentNewLine]))),
                            steps : List([
                                Step(
                                    Token(
                                        TriviaList([Whitespace("    ")]),
                                        SyntaxKind.ContextStepKeyword,
                                        "Given",
                                        TriviaList([Space])),
                                    LiteralText(
                                        TokenList([
                                            Literal(
                                                TriviaList(),
                                                "the minimalism",
                                                TriviaList([EnvironmentNewLine]))
                                            ])))
                            ]))
                    ])),
                Token(
                    TriviaList(),
                    SyntaxKind.EndOfFileToken,
                    TriviaList())));

        tree.ToString().Should().Be(source);
    }
}
