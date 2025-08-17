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
                            Literal(TriviaList(), "tag_feature", TriviaList([EnvironmentNewLine])))
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
                            TriviaList([EnvironmentNewLine]))),
                    rules: List([
                        Rule(
                            Token(
                                TriviaList([EnvironmentNewLine, Whitespace("  ")]),
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
                                    TriviaList([EnvironmentNewLine]))),
                            description: LiteralText(
                                TokenList([
                                    Literal(
                                        TriviaList([Whitespace("  ")]),
                                        "The untagged rule description",
                                        TriviaList([EnvironmentNewLine, EnvironmentNewLine]))
                                ])),
                            examples: List([
                                Example(
                                    Token(
                                        TriviaList([Whitespace("    ")]),
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
                                            "Example with only a feature tag",
                                            TriviaList([EnvironmentNewLine]))),
                                    steps: List([
                                        Step(
                                            Token(
                                                TriviaList([Whitespace("      ")]),
                                                SyntaxKind.ContextStepKeyword,
                                                "Given",
                                                TriviaList([Space])),
                                            LiteralText(
                                                Literal(
                                                    TriviaList(),
                                                    "a",
                                                    TriviaList([EnvironmentNewLine]))))
                                    ]))
                            ])
                        ),
                        Rule(
                            List([
                                Tag(
                                    Token(TriviaList([EnvironmentNewLine, Whitespace("  ")]), SyntaxKind.AtToken, TriviaList()),
                                    Literal(TriviaList(), "tag_rule", TriviaList([EnvironmentNewLine])))
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
                                    TriviaList([EnvironmentNewLine]))),
                            description: LiteralText(
                                TokenList([
                                    Literal(
                                        TriviaList([Whitespace("  ")]),
                                        "The tagged rule description",
                                        TriviaList([EnvironmentNewLine, EnvironmentNewLine]))
                                ])),
                            examples: List([
                                Example(
                                    Token(
                                        TriviaList([Whitespace("    ")]),
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
                                            "Example with feature and rule tags",
                                            TriviaList([EnvironmentNewLine]))),
                                    steps: List([
                                        Step(
                                            Token(
                                                TriviaList([Whitespace("      ")]),
                                                SyntaxKind.ContextStepKeyword,
                                                "Given",
                                                TriviaList([Space])),
                                            LiteralText(
                                                Literal(
                                                    TriviaList(),
                                                    "b",
                                                    TriviaList([EnvironmentNewLine]))))
                                    ])),
                                Example(
                                    List([
                                        Tag(
                                            Token(TriviaList([EnvironmentNewLine, Whitespace("    ")]), SyntaxKind.AtToken, TriviaList()),
                                            Literal(TriviaList(), "tag_scenario", TriviaList([EnvironmentNewLine])))
                                    ]),
                                    Token(
                                        TriviaList([Whitespace("    ")]),
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
                                            "Example with feature, rule and scenario tags",
                                            TriviaList([EnvironmentNewLine]))),
                                    steps: List([
                                        Step(
                                            Token(
                                                TriviaList([Whitespace("      ")]),
                                                SyntaxKind.ContextStepKeyword,
                                                "Given",
                                                TriviaList([Space])),
                                            LiteralText(
                                                Literal(
                                                    TriviaList(),
                                                    "b",
                                                    TriviaList([EnvironmentNewLine]))))
                                    ])),
                                Example(
                                    List([
                                        Tag(
                                            Token(TriviaList([EnvironmentNewLine, Whitespace("    ")]), SyntaxKind.AtToken, TriviaList()),
                                            Literal(TriviaList(), "tag_outline", TriviaList([EnvironmentNewLine])))
                                    ]),
                                    Token(
                                        TriviaList([Whitespace("    ")]),
                                        SyntaxKind.ExampleKeyword,
                                        "Example Outline",
                                        TriviaList()),
                                    Token(
                                        TriviaList(),
                                        SyntaxKind.ColonToken,
                                        TriviaList([Space])),
                                    LiteralText(
                                        Literal(
                                            TriviaList(),
                                            "Tagged Example outline",
                                            TriviaList([EnvironmentNewLine]))),
                                    steps: List([
                                        Step(
                                            Token(
                                                TriviaList([Whitespace("      ")]),
                                                SyntaxKind.ContextStepKeyword,
                                                "Given",
                                                TriviaList([Space])),
                                            LiteralText(
                                                Literal(
                                                    TriviaList(),
                                                    "b",
                                                    TriviaList([EnvironmentNewLine]))))
                                    ]),
                                    examples: List([
                                        Examples(
                                            List([
                                                Tag(
                                                    Token(TriviaList([EnvironmentNewLine, Whitespace("    ")]), SyntaxKind.AtToken, TriviaList()),
                                                    Literal(TriviaList(), "examples_tag", TriviaList([EnvironmentNewLine])))
                                            ]),
                                            Token(
                                                TriviaList([Whitespace("      ")]),
                                                SyntaxKind.ExamplesKeyword,
                                                "Examples",
                                                TriviaList()),
                                            Token(
                                                TriviaList(),
                                                SyntaxKind.ColonToken,
                                                TriviaList([EnvironmentNewLine])),
                                            table: Table(
                                                List([
                                                    TableRow(
                                                        Token(
                                                            TriviaList([Whitespace("        ")]),
                                                            SyntaxKind.VerticalBarToken,
                                                            TriviaList([Space])),
                                                        TableCellList([
                                                            TextTableCell(
                                                                TableLiteral(
                                                                    TriviaList(),
                                                                    "header",
                                                                    TriviaList()))
                                                        ]),
                                                        Token(
                                                            TriviaList([Space]),
                                                            SyntaxKind.VerticalBarToken,
                                                            TriviaList([EnvironmentNewLine]))),
                                                    TableRow(
                                                        Token(
                                                            TriviaList([Whitespace("        ")]),
                                                            SyntaxKind.VerticalBarToken,
                                                            TriviaList([Space])),
                                                        TableCellList([
                                                            TextTableCell(
                                                                TableLiteral(
                                                                    TriviaList(),
                                                                    "a",
                                                                    TriviaList()))
                                                        ]),
                                                        Token(
                                                            TriviaList([Whitespace("      ")]),
                                                            SyntaxKind.VerticalBarToken,
                                                            TriviaList([EnvironmentNewLine])))
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
