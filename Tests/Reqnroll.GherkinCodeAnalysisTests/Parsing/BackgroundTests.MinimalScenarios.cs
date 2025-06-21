﻿namespace Reqnroll.CodeAnalysis.Gherkin.Parsing;

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
                                    SyntaxKind.GivenKeyword,
                                    "Given",
                                    TriviaList([Space])),
                                LiteralText(
                                    TokenList(
                                        Token(
                                            TriviaList(),
                                            SyntaxKind.LiteralToken,
                                            "the minimalism inside a background",
                                            TriviaList([EnvironmentNewLine])))))
                            ])),
                    members: List([
                        Example(
                            Token(
                                TriviaList([EnvironmentNewLine, EnvironmentNewLine]),
                                SyntaxKind.ExampleKeyword,
                                "Example",
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
                                        TriviaList([Whitespace("  ")]),
                                        SyntaxKind.GivenKeyword,
                                        "Given",
                                        TriviaList([Space])),
                                    LiteralText(
                                        TokenList([
                                            Token(
                                                TriviaList(),
                                                SyntaxKind.LiteralToken,
                                                "the minimalism",
                                                TriviaList([EnvironmentNewLine]))
                                            ])))
                                ])),
                        Example(
                            Token(
                                TriviaList([EnvironmentNewLine, EnvironmentNewLine]),
                                SyntaxKind.ExampleKeyword,
                                "Example",
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
                                        TriviaList([Whitespace("  ")]),
                                        SyntaxKind.GivenKeyword,
                                        "Given",
                                        TriviaList([Space])),
                                    LiteralText(
                                        TokenList([
                                            Token(
                                                TriviaList(),
                                                SyntaxKind.LiteralToken,
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
