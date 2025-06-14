using Reqnroll.CodeAnalysis.Gherkin.Syntax;

namespace Reqnroll.CodeAnalysis.Gherkin.Parsing;

using static SyntaxFactory;

public partial class TagsTests
{
    [Fact]
    public void RuleWithTagIsRepresentedInTree()
    {
        // Taken from good/rule_with_tag.feature
        const string source =
            """
            @tag_feature
            Feature: Some tagged rules

              Rule: Untagged rule
              The untagged rule description

                Scenario: Scenario with only a feature tag
                  Given a

              @tag_rule
              Rule: Tagged rule
              The tagged rule description

                Scenario: Scenario with feature and rule tags
                  Given b

                @tag_scenario
                Scenario: Scenario with feature, rule and scenario tags
                  Given b

                @tag_outline
                Scenario Outline: Tagged Scenario outline
                  Given b

                  @examples_tag
                  Examples:
                    | header |
                    | a      |

            """;

        var tree = GherkinSyntaxTree.ParseText(source);

        tree.GetRoot().Should().BeEquivalentTo(
            GherkinDocument(
                Feature(
                    List([
                        Tag(
                            Token(TriviaList(), SyntaxKind.AtToken, TriviaList()),
                            Literal(TriviaList(), "tag_feature", TriviaList([EnvironmentNewline])))
                    ]),
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
                            "Some tagged rules",
                            TriviaList([EnvironmentNewline]))),
                    rules: List([
                        Rule(
                            Token(
                                TriviaList([EnvironmentNewline, Whitespace("  ")]),
                                SyntaxKind.RuleKeyword,
                                "Rule",
                                TriviaList()),
                            Token(
                                TriviaList(),
                                SyntaxKind.ColonToken,
                                TriviaList([Space])),
                            LiteralText(
                                Literal(
                                    TriviaList(),
                                    "Untagged rule",
                                    TriviaList([EnvironmentNewline]))),
                            description: LiteralText(
                                TokenList([
                                    Literal(
                                        TriviaList([Whitespace("  ")]),
                                        "The untagged rule description",
                                        TriviaList([EnvironmentNewline, EnvironmentNewline]))
                                ])),
                            scenarios: List([
                                Scenario(
                                    Token(
                                        TriviaList([Whitespace("    ")]),
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
                                            "Scenario with only a feature tag",
                                            TriviaList([EnvironmentNewline]))),
                                    steps: List([
                                        Step(
                                            Token(
                                                TriviaList([Whitespace("      ")]),
                                                SyntaxKind.GivenKeyword,
                                                "Given",
                                                TriviaList([Space])),
                                            LiteralText(
                                                Literal(
                                                    TriviaList(),
                                                    "a",
                                                    TriviaList([EnvironmentNewline]))))
                                    ]))
                            ])
                        ),
                        Rule(
                            List([
                                Tag(
                                    Token(TriviaList([EnvironmentNewline, Whitespace("  ")]), SyntaxKind.AtToken, TriviaList()),
                                    Literal(TriviaList(), "tag_rule", TriviaList([EnvironmentNewline])))
                            ]),
                            Token(
                                TriviaList(),
                                SyntaxKind.RuleKeyword,
                                "Rule",
                                TriviaList()),
                            Token(
                                TriviaList(),
                                SyntaxKind.ColonToken,
                                TriviaList([Space])),
                            LiteralText(
                                Literal(
                                    TriviaList(),
                                    "Tagged rule",
                                    TriviaList([EnvironmentNewline]))),
                            description: LiteralText(
                                TokenList([
                                    Literal(
                                        TriviaList([Whitespace("  ")]),
                                        "The tagged rule description",
                                        TriviaList([EnvironmentNewline, EnvironmentNewline]))
                                ])),
                            scenarios: List([
                                Scenario(
                                    Token(
                                        TriviaList([Whitespace("    ")]),
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
                                            "Scenario with feature and rule tags",
                                            TriviaList([EnvironmentNewline]))),
                                    steps: List([
                                        Step(
                                            Token(
                                                TriviaList([Whitespace("      ")]),
                                                SyntaxKind.GivenKeyword,
                                                "Given",
                                                TriviaList([Space])),
                                            LiteralText(
                                                Literal(
                                                    TriviaList(),
                                                    "b",
                                                    TriviaList([EnvironmentNewline]))))
                                    ])),
                                Scenario(
                                    List([
                                        Tag(
                                            Token(TriviaList([EnvironmentNewline, Whitespace("    ")]), SyntaxKind.AtToken, TriviaList()),
                                            Literal(TriviaList(), "tag_scenario", TriviaList([EnvironmentNewline])))
                                    ]),
                                    Token(
                                        TriviaList([Whitespace("    ")]),
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
                                            "Scenario with feature, rule and scenario tags",
                                            TriviaList([EnvironmentNewline]))),
                                    steps: List([
                                        Step(
                                            Token(
                                                TriviaList([Whitespace("      ")]),
                                                SyntaxKind.GivenKeyword,
                                                "Given",
                                                TriviaList([Space])),
                                            LiteralText(
                                                Literal(
                                                    TriviaList(),
                                                    "b",
                                                    TriviaList([EnvironmentNewline]))))
                                    ])),
                                Scenario(
                                    List([
                                        Tag(
                                            Token(TriviaList([EnvironmentNewline, Whitespace("    ")]), SyntaxKind.AtToken, TriviaList()),
                                            Literal(TriviaList(), "tag_outline", TriviaList([EnvironmentNewline])))
                                    ]),
                                    Token(
                                        TriviaList([Whitespace("    ")]),
                                        SyntaxKind.ScenarioKeyword,
                                        "Scenario Outline",
                                        TriviaList()),
                                    Token(
                                        TriviaList(),
                                        SyntaxKind.ColonToken,
                                        TriviaList([Space])),
                                    LiteralText(
                                        Literal(
                                            TriviaList(),
                                            "Tagged Scenario outline",
                                            TriviaList([EnvironmentNewline]))),
                                    steps: List([
                                        Step(
                                            Token(
                                                TriviaList([Whitespace("      ")]),
                                                SyntaxKind.GivenKeyword,
                                                "Given",
                                                TriviaList([Space])),
                                            LiteralText(
                                                Literal(
                                                    TriviaList(),
                                                    "b",
                                                    TriviaList([EnvironmentNewline]))))
                                    ]),
                                    examples: List([
                                        Examples(
                                            List([
                                                Tag(
                                                    Token(TriviaList([EnvironmentNewline, Whitespace("    ")]), SyntaxKind.AtToken, TriviaList()),
                                                    Literal(TriviaList(), "examples_tag", TriviaList([EnvironmentNewline])))
                                            ]),
                                            Token(
                                                TriviaList([Whitespace("      ")]),
                                                SyntaxKind.ExamplesKeyword,
                                                "Examples",
                                                TriviaList()),
                                            Token(
                                                TriviaList(),
                                                SyntaxKind.ColonToken,
                                                TriviaList([EnvironmentNewline])),
                                            table: Table(
                                                List([
                                                    TableRow(
                                                        Token(
                                                            TriviaList([Whitespace("        ")]),
                                                            SyntaxKind.VerticalBarToken,
                                                            TriviaList()),
                                                        SeparatedList<PlainTextSyntax>([
                                                            LiteralText(
                                                                TableLiteral(
                                                                    TriviaList([Space]),
                                                                    "header",
                                                                    TriviaList([Space]))
                                                            )
                                                        ]),
                                                        Token(
                                                            TriviaList(),
                                                            SyntaxKind.VerticalBarToken,
                                                            TriviaList([EnvironmentNewline]))),
                                                    TableRow(
                                                        Token(
                                                            TriviaList([Whitespace("        ")]),
                                                            SyntaxKind.VerticalBarToken,
                                                            TriviaList()),
                                                        SeparatedList<PlainTextSyntax>([
                                                            LiteralText(
                                                                TableLiteral(
                                                                    TriviaList([Space]),
                                                                    "a",
                                                                    TriviaList([Whitespace("      ")]))
                                                            )
                                                        ]),
                                                        Token(
                                                            TriviaList(),
                                                            SyntaxKind.VerticalBarToken,
                                                            TriviaList([EnvironmentNewline])))
                                                ])))
                                    ]))
                            ]))
                    ])),
                Token(
                    TriviaList(),
                    SyntaxKind.EndOfFileToken,
                    TriviaList())));

        tree.ToString().Should().Be(source);
    }
}
