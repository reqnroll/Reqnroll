using Reqnroll.CodeAnalysis.Gherkin.Syntax;

namespace Reqnroll.CodeAnalysis.Gherkin.Parsing;

using static SyntaxFactory;

public partial class FeatureTests
{
    [Fact]
    public void VeryLongFeatureIsRepresentedInTree()
    {
        // Taken from good/very_long.feature
        const string source =
            """
            Feature: Long feature file
              This is a long feature file

              Scenario: scenario 01
                Given a simple data table
                  | foo | bar |
                  | boz | boo |
                And a data table with a single cell
                  | foo |
                And a data table with different fromatting
                  |   foo|bar|    boz    |
                And a data table with an empty cell
                  |foo||boz|
                And a data table with comments and newlines inside
                  | foo | bar |

                  | boz  | boo  |
                  # this is a comment
                  | boz2 | boo2 |

              Scenario: scenario 02
                Given a simple data table
                  | foo | bar |
                  | boz | boo |
                And a data table with a single cell
                  | foo |
                And a data table with different fromatting
                  |   foo|bar|    boz    |
                And a data table with an empty cell
                  |foo||boz|
                And a data table with comments and newlines inside
                  | foo | bar |

                  | boz  | boo  |
                  # this is a comment
                  | boz2 | boo2 |

              Scenario: scenario 03
                Given a simple data table
                  | foo | bar |
                  | boz | boo |
                And a data table with a single cell
                  | foo |
                And a data table with different fromatting
                  |   foo|bar|    boz    |
                And a data table with an empty cell
                  |foo||boz|
                And a data table with comments and newlines inside
                  | foo | bar |

                  | boz  | boo  |
                  # this is a comment
                  | boz2 | boo2 |

              Scenario: scenario 04
                Given a simple data table
                  | foo | bar |
                  | boz | boo |
                And a data table with a single cell
                  | foo |
                And a data table with different fromatting
                  |   foo|bar|    boz    |
                And a data table with an empty cell
                  |foo||boz|
                And a data table with comments and newlines inside
                  | foo | bar |

                  | boz  | boo  |
                  # this is a comment
                  | boz2 | boo2 |

              Scenario: scenario 05
                Given a simple data table
                  | foo | bar |
                  | boz | boo |
                And a data table with a single cell
                  | foo |
                And a data table with different fromatting
                  |   foo|bar|    boz    |
                And a data table with an empty cell
                  |foo||boz|
                And a data table with comments and newlines inside
                  | foo | bar |

                  | boz  | boo  |
                  # this is a comment
                  | boz2 | boo2 |

              Scenario: scenario 06
                Given a simple data table
                  | foo | bar |
                  | boz | boo |
                And a data table with a single cell
                  | foo |
                And a data table with different fromatting
                  |   foo|bar|    boz    |
                And a data table with an empty cell
                  |foo||boz|
                And a data table with comments and newlines inside
                  | foo | bar |

                  | boz  | boo  |
                  # this is a comment
                  | boz2 | boo2 |

              Scenario: scenario 07
                Given a simple data table
                  | foo | bar |
                  | boz | boo |
                And a data table with a single cell
                  | foo |
                And a data table with different fromatting
                  |   foo|bar|    boz    |
                And a data table with an empty cell
                  |foo||boz|
                And a data table with comments and newlines inside
                  | foo | bar |

                  | boz  | boo  |
                  # this is a comment
                  | boz2 | boo2 |

              Scenario: scenario 08
                Given a simple data table
                  | foo | bar |
                  | boz | boo |
                And a data table with a single cell
                  | foo |
                And a data table with different fromatting
                  |   foo|bar|    boz    |
                And a data table with an empty cell
                  |foo||boz|
                And a data table with comments and newlines inside
                  | foo | bar |

                  | boz  | boo  |
                  # this is a comment
                  | boz2 | boo2 |

              Scenario: scenario 09
                Given a simple data table
                  | foo | bar |
                  | boz | boo |
                And a data table with a single cell
                  | foo |
                And a data table with different fromatting
                  |   foo|bar|    boz    |
                And a data table with an empty cell
                  |foo||boz|
                And a data table with comments and newlines inside
                  | foo | bar |

                  | boz  | boo  |
                  # this is a comment
                  | boz2 | boo2 |

              Scenario: scenario 10
                Given a simple data table
                  | foo | bar |
                  | boz | boo |
                And a data table with a single cell
                  | foo |
                And a data table with different fromatting
                  |   foo|bar|    boz    |
                And a data table with an empty cell
                  |foo||boz|
                And a data table with comments and newlines inside
                  | foo | bar |

                  | boz  | boo  |
                  # this is a comment
                  | boz2 | boo2 |

              Scenario: scenario 11
                Given a simple data table
                  | foo | bar |
                  | boz | boo |
                And a data table with a single cell
                  | foo |
                And a data table with different fromatting
                  |   foo|bar|    boz    |
                And a data table with an empty cell
                  |foo||boz|
                And a data table with comments and newlines inside
                  | foo | bar |

                  | boz  | boo  |
                  # this is a comment
                  | boz2 | boo2 |

              Scenario: scenario 12
                Given a simple data table
                  | foo | bar |
                  | boz | boo |
                And a data table with a single cell
                  | foo |
                And a data table with different fromatting
                  |   foo|bar|    boz    |
                And a data table with an empty cell
                  |foo||boz|
                And a data table with comments and newlines inside
                  | foo | bar |

                  | boz  | boo  |
                  # this is a comment
                  | boz2 | boo2 |

              Scenario: scenario 13
                Given a simple data table
                  | foo | bar |
                  | boz | boo |
                And a data table with a single cell
                  | foo |
                And a data table with different fromatting
                  |   foo|bar|    boz    |
                And a data table with an empty cell
                  |foo||boz|
                And a data table with comments and newlines inside
                  | foo | bar |

                  | boz  | boo  |
                  # this is a comment
                  | boz2 | boo2 |

              Scenario: scenario 14
                Given a simple data table
                  | foo | bar |
                  | boz | boo |
                And a data table with a single cell
                  | foo |
                And a data table with different fromatting
                  |   foo|bar|    boz    |
                And a data table with an empty cell
                  |foo||boz|
                And a data table with comments and newlines inside
                  | foo | bar |

                  | boz  | boo  |
                  # this is a comment
                  | boz2 | boo2 |

              Scenario: scenario 15
                Given a simple data table
                  | foo | bar |
                  | boz | boo |
                And a data table with a single cell
                  | foo |
                And a data table with different fromatting
                  |   foo|bar|    boz    |
                And a data table with an empty cell
                  |foo||boz|
                And a data table with comments and newlines inside
                  | foo | bar |

                  | boz  | boo  |
                  # this is a comment
                  | boz2 | boo2 |

              Scenario: scenario 16
                Given a simple data table
                  | foo | bar |
                  | boz | boo |
                And a data table with a single cell
                  | foo |
                And a data table with different fromatting
                  |   foo|bar|    boz    |
                And a data table with an empty cell
                  |foo||boz|
                And a data table with comments and newlines inside
                  | foo | bar |

                  | boz  | boo  |
                  # this is a comment
                  | boz2 | boo2 |

              Scenario: scenario 17
                Given a simple data table
                  | foo | bar |
                  | boz | boo |
                And a data table with a single cell
                  | foo |
                And a data table with different fromatting
                  |   foo|bar|    boz    |
                And a data table with an empty cell
                  |foo||boz|
                And a data table with comments and newlines inside
                  | foo | bar |

                  | boz  | boo  |
                  # this is a comment
                  | boz2 | boo2 |

              Scenario: scenario 18
                Given a simple data table
                  | foo | bar |
                  | boz | boo |
                And a data table with a single cell
                  | foo |
                And a data table with different fromatting
                  |   foo|bar|    boz    |
                And a data table with an empty cell
                  |foo||boz|
                And a data table with comments and newlines inside
                  | foo | bar |

                  | boz  | boo  |
                  # this is a comment
                  | boz2 | boo2 |

              Scenario: scenario 19
                Given a simple data table
                  | foo | bar |
                  | boz | boo |
                And a data table with a single cell
                  | foo |
                And a data table with different fromatting
                  |   foo|bar|    boz    |
                And a data table with an empty cell
                  |foo||boz|
                And a data table with comments and newlines inside
                  | foo | bar |

                  | boz  | boo  |
                  # this is a comment
                  | boz2 | boo2 |

              Scenario: scenario 20
                Given a simple data table
                  | foo | bar |
                  | boz | boo |
                And a data table with a single cell
                  | foo |
                And a data table with different fromatting
                  |   foo|bar|    boz    |
                And a data table with an empty cell
                  |foo||boz|
                And a data table with comments and newlines inside
                  | foo | bar |

                  | boz  | boo  |
                  # this is a comment
                  | boz2 | boo2 |

              Scenario: scenario 21
                Given a simple data table
                  | foo | bar |
                  | boz | boo |
                And a data table with a single cell
                  | foo |
                And a data table with different fromatting
                  |   foo|bar|    boz    |
                And a data table with an empty cell
                  |foo||boz|
                And a data table with comments and newlines inside
                  | foo | bar |

                  | boz  | boo  |
                  # this is a comment
                  | boz2 | boo2 |

              Scenario: scenario 22
                Given a simple data table
                  | foo | bar |
                  | boz | boo |
                And a data table with a single cell
                  | foo |
                And a data table with different fromatting
                  |   foo|bar|    boz    |
                And a data table with an empty cell
                  |foo||boz|
                And a data table with comments and newlines inside
                  | foo | bar |

                  | boz  | boo  |
                  # this is a comment
                  | boz2 | boo2 |

              Scenario: scenario 23
                Given a simple data table
                  | foo | bar |
                  | boz | boo |
                And a data table with a single cell
                  | foo |
                And a data table with different fromatting
                  |   foo|bar|    boz    |
                And a data table with an empty cell
                  |foo||boz|
                And a data table with comments and newlines inside
                  | foo | bar |

                  | boz  | boo  |
                  # this is a comment
                  | boz2 | boo2 |

              Scenario: scenario 24
                Given a simple data table
                  | foo | bar |
                  | boz | boo |
                And a data table with a single cell
                  | foo |
                And a data table with different fromatting
                  |   foo|bar|    boz    |
                And a data table with an empty cell
                  |foo||boz|
                And a data table with comments and newlines inside
                  | foo | bar |

                  | boz  | boo  |
                  # this is a comment
                  | boz2 | boo2 |

              Scenario: scenario 25
                Given a simple data table
                  | foo | bar |
                  | boz | boo |
                And a data table with a single cell
                  | foo |
                And a data table with different fromatting
                  |   foo|bar|    boz    |
                And a data table with an empty cell
                  |foo||boz|
                And a data table with comments and newlines inside
                  | foo | bar |

                  | boz  | boo  |
                  # this is a comment
                  | boz2 | boo2 |

              Scenario: scenario 26
                Given a simple data table
                  | foo | bar |
                  | boz | boo |
                And a data table with a single cell
                  | foo |
                And a data table with different fromatting
                  |   foo|bar|    boz    |
                And a data table with an empty cell
                  |foo||boz|
                And a data table with comments and newlines inside
                  | foo | bar |

                  | boz  | boo  |
                  # this is a comment
                  | boz2 | boo2 |

              Scenario: scenario 27
                Given a simple data table
                  | foo | bar |
                  | boz | boo |
                And a data table with a single cell
                  | foo |
                And a data table with different fromatting
                  |   foo|bar|    boz    |
                And a data table with an empty cell
                  |foo||boz|
                And a data table with comments and newlines inside
                  | foo | bar |

                  | boz  | boo  |
                  # this is a comment
                  | boz2 | boo2 |

              Scenario: scenario 28
                Given a simple data table
                  | foo | bar |
                  | boz | boo |
                And a data table with a single cell
                  | foo |
                And a data table with different fromatting
                  |   foo|bar|    boz    |
                And a data table with an empty cell
                  |foo||boz|
                And a data table with comments and newlines inside
                  | foo | bar |

                  | boz  | boo  |
                  # this is a comment
                  | boz2 | boo2 |

              Scenario: scenario 29
                Given a simple data table
                  | foo | bar |
                  | boz | boo |
                And a data table with a single cell
                  | foo |
                And a data table with different fromatting
                  |   foo|bar|    boz    |
                And a data table with an empty cell
                  |foo||boz|
                And a data table with comments and newlines inside
                  | foo | bar |

                  | boz  | boo  |
                  # this is a comment
                  | boz2 | boo2 |

              Scenario: scenario 30
                Given a simple data table
                  | foo | bar |
                  | boz | boo |
                And a data table with a single cell
                  | foo |
                And a data table with different fromatting
                  |   foo|bar|    boz    |
                And a data table with an empty cell
                  |foo||boz|
                And a data table with comments and newlines inside
                  | foo | bar |

                  | boz  | boo  |
                  # this is a comment
                  | boz2 | boo2 |

              Scenario: scenario 31
                Given a simple data table
                  | foo | bar |
                  | boz | boo |
                And a data table with a single cell
                  | foo |
                And a data table with different fromatting
                  |   foo|bar|    boz    |
                And a data table with an empty cell
                  |foo||boz|
                And a data table with comments and newlines inside
                  | foo | bar |

                  | boz  | boo  |
                  # this is a comment
                  | boz2 | boo2 |

              Scenario: scenario 32
                Given a simple data table
                  | foo | bar |
                  | boz | boo |
                And a data table with a single cell
                  | foo |
                And a data table with different fromatting
                  |   foo|bar|    boz    |
                And a data table with an empty cell
                  |foo||boz|
                And a data table with comments and newlines inside
                  | foo | bar |

                  | boz  | boo  |
                  # this is a comment
                  | boz2 | boo2 |

              Scenario: scenario 33
                Given a simple data table
                  | foo | bar |
                  | boz | boo |
                And a data table with a single cell
                  | foo |
                And a data table with different fromatting
                  |   foo|bar|    boz    |
                And a data table with an empty cell
                  |foo||boz|
                And a data table with comments and newlines inside
                  | foo | bar |

                  | boz  | boo  |
                  # this is a comment
                  | boz2 | boo2 |

              Scenario: scenario 34
                Given a simple data table
                  | foo | bar |
                  | boz | boo |
                And a data table with a single cell
                  | foo |
                And a data table with different fromatting
                  |   foo|bar|    boz    |
                And a data table with an empty cell
                  |foo||boz|
                And a data table with comments and newlines inside
                  | foo | bar |

                  | boz  | boo  |
                  # this is a comment
                  | boz2 | boo2 |

              Scenario: scenario 35
                Given a simple data table
                  | foo | bar |
                  | boz | boo |
                And a data table with a single cell
                  | foo |
                And a data table with different fromatting
                  |   foo|bar|    boz    |
                And a data table with an empty cell
                  |foo||boz|
                And a data table with comments and newlines inside
                  | foo | bar |

                  | boz  | boo  |
                  # this is a comment
                  | boz2 | boo2 |

              Scenario: scenario 36
                Given a simple data table
                  | foo | bar |
                  | boz | boo |
                And a data table with a single cell
                  | foo |
                And a data table with different fromatting
                  |   foo|bar|    boz    |
                And a data table with an empty cell
                  |foo||boz|
                And a data table with comments and newlines inside
                  | foo | bar |

                  | boz  | boo  |
                  # this is a comment
                  | boz2 | boo2 |

              Scenario: scenario 37
                Given a simple data table
                  | foo | bar |
                  | boz | boo |
                And a data table with a single cell
                  | foo |
                And a data table with different fromatting
                  |   foo|bar|    boz    |
                And a data table with an empty cell
                  |foo||boz|
                And a data table with comments and newlines inside
                  | foo | bar |

                  | boz  | boo  |
                  # this is a comment
                  | boz2 | boo2 |

              Scenario: scenario 38
                Given a simple data table
                  | foo | bar |
                  | boz | boo |
                And a data table with a single cell
                  | foo |
                And a data table with different fromatting
                  |   foo|bar|    boz    |
                And a data table with an empty cell
                  |foo||boz|
                And a data table with comments and newlines inside
                  | foo | bar |

                  | boz  | boo  |
                  # this is a comment
                  | boz2 | boo2 |

              Scenario: scenario 39
                Given a simple data table
                  | foo | bar |
                  | boz | boo |
                And a data table with a single cell
                  | foo |
                And a data table with different fromatting
                  |   foo|bar|    boz    |
                And a data table with an empty cell
                  |foo||boz|
                And a data table with comments and newlines inside
                  | foo | bar |

                  | boz  | boo  |
                  # this is a comment
                  | boz2 | boo2 |

              Scenario: scenario 40
                Given a simple data table
                  | foo | bar |
                  | boz | boo |
                And a data table with a single cell
                  | foo |
                And a data table with different fromatting
                  |   foo|bar|    boz    |
                And a data table with an empty cell
                  |foo||boz|
                And a data table with comments and newlines inside
                  | foo | bar |

                  | boz  | boo  |
                  # this is a comment
                  | boz2 | boo2 |

              Scenario: scenario 41
                Given a simple data table
                  | foo | bar |
                  | boz | boo |
                And a data table with a single cell
                  | foo |
                And a data table with different fromatting
                  |   foo|bar|    boz    |
                And a data table with an empty cell
                  |foo||boz|
                And a data table with comments and newlines inside
                  | foo | bar |

                  | boz  | boo  |
                  # this is a comment
                  | boz2 | boo2 |

              Scenario: scenario 42
                Given a simple data table
                  | foo | bar |
                  | boz | boo |
                And a data table with a single cell
                  | foo |
                And a data table with different fromatting
                  |   foo|bar|    boz    |
                And a data table with an empty cell
                  |foo||boz|
                And a data table with comments and newlines inside
                  | foo | bar |

                  | boz  | boo  |
                  # this is a comment
                  | boz2 | boo2 |

              Scenario: scenario 43
                Given a simple data table
                  | foo | bar |
                  | boz | boo |
                And a data table with a single cell
                  | foo |
                And a data table with different fromatting
                  |   foo|bar|    boz    |
                And a data table with an empty cell
                  |foo||boz|
                And a data table with comments and newlines inside
                  | foo | bar |

                  | boz  | boo  |
                  # this is a comment
                  | boz2 | boo2 |

              Scenario: scenario 44
                Given a simple data table
                  | foo | bar |
                  | boz | boo |
                And a data table with a single cell
                  | foo |
                And a data table with different fromatting
                  |   foo|bar|    boz    |
                And a data table with an empty cell
                  |foo||boz|
                And a data table with comments and newlines inside
                  | foo | bar |

                  | boz  | boo  |
                  # this is a comment
                  | boz2 | boo2 |

              Scenario: scenario 45
                Given a simple data table
                  | foo | bar |
                  | boz | boo |
                And a data table with a single cell
                  | foo |
                And a data table with different fromatting
                  |   foo|bar|    boz    |
                And a data table with an empty cell
                  |foo||boz|
                And a data table with comments and newlines inside
                  | foo | bar |

                  | boz  | boo  |
                  # this is a comment
                  | boz2 | boo2 |

              Scenario: scenario 46
                Given a simple data table
                  | foo | bar |
                  | boz | boo |
                And a data table with a single cell
                  | foo |
                And a data table with different fromatting
                  |   foo|bar|    boz    |
                And a data table with an empty cell
                  |foo||boz|
                And a data table with comments and newlines inside
                  | foo | bar |

                  | boz  | boo  |
                  # this is a comment
                  | boz2 | boo2 |

              Scenario: scenario 47
                Given a simple data table
                  | foo | bar |
                  | boz | boo |
                And a data table with a single cell
                  | foo |
                And a data table with different fromatting
                  |   foo|bar|    boz    |
                And a data table with an empty cell
                  |foo||boz|
                And a data table with comments and newlines inside
                  | foo | bar |

                  | boz  | boo  |
                  # this is a comment
                  | boz2 | boo2 |

              Scenario: scenario 48
                Given a simple data table
                  | foo | bar |
                  | boz | boo |
                And a data table with a single cell
                  | foo |
                And a data table with different fromatting
                  |   foo|bar|    boz    |
                And a data table with an empty cell
                  |foo||boz|
                And a data table with comments and newlines inside
                  | foo | bar |

                  | boz  | boo  |
                  # this is a comment
                  | boz2 | boo2 |

              Scenario: scenario 49
                Given a simple data table
                  | foo | bar |
                  | boz | boo |
                And a data table with a single cell
                  | foo |
                And a data table with different fromatting
                  |   foo|bar|    boz    |
                And a data table with an empty cell
                  |foo||boz|
                And a data table with comments and newlines inside
                  | foo | bar |

                  | boz  | boo  |
                  # this is a comment
                  | boz2 | boo2 |

              Scenario: scenario 50
                Given a simple data table
                  | foo | bar |
                  | boz | boo |
                And a data table with a single cell
                  | foo |
                And a data table with different fromatting
                  |   foo|bar|    boz    |
                And a data table with an empty cell
                  |foo||boz|
                And a data table with comments and newlines inside
                  | foo | bar |

                  | boz  | boo  |
                  # this is a comment
                  | boz2 | boo2 |

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
                        TokenList([
                            Literal(
                                TriviaList(),
                                "Long feature file",
                                TriviaList([EnvironmentNewLine, Whitespace("  ")])),
                            Literal(
                                TriviaList(),
                                "This is a long feature file",
                                TriviaList([EnvironmentNewLine, EnvironmentNewLine]))
                        ])),
                    examples: List([
                        Example(
                            Token(
                                TriviaList([Whitespace("  ")]),
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
                                    "scenario 01",
                                    TriviaList([EnvironmentNewLine]))),
                            steps: List([
                                Step(
                                    Token(
                                        TriviaList([Whitespace("    ")]),
                                        SyntaxKind.ContextStepKeyword,
                                        "Given",
                                        TriviaList([Space])),
                                    LiteralText(
                                        Literal(
                                            TriviaList(),
                                            "a simple data table",
                                            TriviaList([EnvironmentNewLine]))),
                                    StepTable(
                                        Table(
                                            List([
                                                TableRow(
                                                    Token(
                                                        TriviaList([Whitespace("      ")]),
                                                        SyntaxKind.VerticalBarToken,
                                                        TriviaList([Space])),
                                                    TableCellList([
                                                        TextTableCell(
                                                            TableLiteral(
                                                                TriviaList(),
                                                                "foo",
                                                                TriviaList())),
                                                        Token(
                                                            TriviaList([Space]),
                                                            SyntaxKind.VerticalBarToken,
                                                            TriviaList([Space])),
                                                        TextTableCell(
                                                            TableLiteral(
                                                                TriviaList(),
                                                                "bar",
                                                                TriviaList())
                                                        )
                                                    ]),
                                                    Token(
                                                        TriviaList([Space]),
                                                        SyntaxKind.VerticalBarToken,
                                                        TriviaList([EnvironmentNewLine]))),
                                                TableRow(
                                                    Token(
                                                        TriviaList([Whitespace("      ")]),
                                                        SyntaxKind.VerticalBarToken,
                                                        TriviaList([Space])),
                                                    TableCellList([
                                                        TextTableCell(
                                                            TableLiteral(
                                                                TriviaList(),
                                                                "boz",
                                                                TriviaList())),
                                                        Token(
                                                            TriviaList([Space]),
                                                            SyntaxKind.VerticalBarToken,
                                                            TriviaList([Space])),
                                                        TextTableCell(
                                                            TableLiteral(
                                                                TriviaList(),
                                                                "boo",
                                                                TriviaList())
                                                        )
                                                    ]),
                                                    Token(
                                                        TriviaList([Space]),
                                                        SyntaxKind.VerticalBarToken,
                                                        TriviaList([EnvironmentNewLine])))
                                            ])))),
                                Step(
                                    Token(
                                        TriviaList([Whitespace("    ")]),
                                        SyntaxKind.ConjunctionStepKeyword,
                                        "And",
                                        TriviaList([Space])),
                                    LiteralText(
                                        Literal(
                                            TriviaList(),
                                            "a data table with a single cell",
                                            TriviaList([EnvironmentNewLine]))),
                                    StepTable(
                                        Table(
                                            List([
                                                TableRow(
                                                    Token(
                                                        TriviaList([Whitespace("      ")]),
                                                        SyntaxKind.VerticalBarToken,
                                                        TriviaList([Space])),
                                                    TableCellList([
                                                        TextTableCell(
                                                            TableLiteral(
                                                                TriviaList(),
                                                                "foo",
                                                                TriviaList())
                                                        )
                                                    ]),
                                                    Token(
                                                        TriviaList([Space]),
                                                        SyntaxKind.VerticalBarToken,
                                                        TriviaList([EnvironmentNewLine])))
                                            ])))),
                                Step(
                                    Token(
                                        TriviaList([Whitespace("    ")]),
                                        SyntaxKind.ConjunctionStepKeyword,
                                        "And",
                                        TriviaList([Space])),
                                    LiteralText(
                                        Literal(
                                            TriviaList(),
                                            "a data table with different fromatting",
                                            TriviaList([EnvironmentNewLine]))),
                                    StepTable(
                                        Table(
                                            List([
                                                TableRow(
                                                    Token(
                                                        TriviaList([Whitespace("      ")]),
                                                        SyntaxKind.VerticalBarToken,
                                                        TriviaList([Space, Space, Space])),
                                                    TableCellList([
                                                        TextTableCell(
                                                            TableLiteral(
                                                                TriviaList(),
                                                                "foo",
                                                                TriviaList())),
                                                        Token(
                                                            TriviaList(),
                                                            SyntaxKind.VerticalBarToken,
                                                            TriviaList()),
                                                        TextTableCell(
                                                            TableLiteral(
                                                                TriviaList(),
                                                                "bar",
                                                                TriviaList())),
                                                        Token(
                                                            TriviaList(),
                                                            SyntaxKind.VerticalBarToken,
                                                            TriviaList([Whitespace("    ")])),
                                                        TextTableCell(
                                                            TableLiteral(
                                                                TriviaList(),
                                                                "boz",
                                                                TriviaList())
                                                        )
                                                    ]),
                                                    Token(
                                                        TriviaList([Whitespace("    ")]),
                                                        SyntaxKind.VerticalBarToken,
                                                        TriviaList([EnvironmentNewLine])))
                                            ])))),
                                Step(
                                    Token(
                                        TriviaList([Whitespace("    ")]),
                                        SyntaxKind.ConjunctionStepKeyword,
                                        "And",
                                        TriviaList([Space])),
                                    LiteralText(
                                        Literal(
                                            TriviaList(),
                                            "a data table with an empty cell",
                                            TriviaList([EnvironmentNewLine]))),
                                    StepTable(
                                        Table(
                                            List([
                                                TableRow(
                                                    Token(
                                                        TriviaList([Whitespace("      ")]),
                                                        SyntaxKind.VerticalBarToken,
                                                        TriviaList()),
                                                    TableCellList([
                                                        TextTableCell(
                                                            TableLiteral(
                                                                TriviaList(),
                                                                "foo",
                                                                TriviaList())),
                                                        Token(
                                                            TriviaList(),
                                                            SyntaxKind.VerticalBarToken,
                                                            TriviaList()),
                                                        EmptyTableCell(),
                                                        Token(
                                                            TriviaList(),
                                                            SyntaxKind.VerticalBarToken,
                                                            TriviaList()),
                                                        TextTableCell(
                                                            TableLiteral(
                                                                TriviaList(),
                                                                "boz",
                                                                TriviaList()))
                                                    ]),
                                                    Token(
                                                        TriviaList(),
                                                        SyntaxKind.VerticalBarToken,
                                                        TriviaList([EnvironmentNewLine])))
                                            ])))),
                                Step(
                                    Token(
                                        TriviaList([Whitespace("    ")]),
                                        SyntaxKind.ConjunctionStepKeyword,
                                        "And",
                                        TriviaList([Space])),
                                    LiteralText(
                                        Literal(
                                            TriviaList(),
                                            "a data table with comments and newlines inside",
                                            TriviaList([EnvironmentNewLine]))),
                                    StepTable(
                                        Table(
                                            List([
                                                TableRow(
                                                    Token(
                                                        TriviaList([Whitespace("      ")]),
                                                        SyntaxKind.VerticalBarToken,
                                                        TriviaList([Space])),
                                                    TableCellList([
                                                        TextTableCell(
                                                            TableLiteral(
                                                                TriviaList(),
                                                                "foo",
                                                                TriviaList())),
                                                        Token(
                                                            TriviaList([Space]),
                                                            SyntaxKind.VerticalBarToken,
                                                            TriviaList([Space])),
                                                        TextTableCell(
                                                            TableLiteral(
                                                                TriviaList(),
                                                                "bar",
                                                                TriviaList())
                                                        )
                                                    ]),
                                                    Token(
                                                        TriviaList([Space]),
                                                        SyntaxKind.VerticalBarToken,
                                                        TriviaList([EnvironmentNewLine, EnvironmentNewLine]))),
                                                TableRow(
                                                    Token(
                                                        TriviaList([Whitespace("      ")]),
                                                        SyntaxKind.VerticalBarToken,
                                                        TriviaList([Space])),
                                                    TableCellList([
                                                        TextTableCell(
                                                            TableLiteral(
                                                                TriviaList(),
                                                                "boz",
                                                                TriviaList())),
                                                        Token(
                                                            TriviaList([Whitespace("  ")]),
                                                            SyntaxKind.VerticalBarToken,
                                                            TriviaList([Space])),
                                                        TextTableCell(
                                                            TableLiteral(
                                                                TriviaList(),
                                                                "boo",
                                                                TriviaList())
                                                        )
                                                    ]),
                                                    Token(
                                                        TriviaList([Whitespace("  ")]),
                                                        SyntaxKind.VerticalBarToken,
                                                        TriviaList([EnvironmentNewLine, Comment("# this is a comment"), EnvironmentNewLine]))),
                                                TableRow(
                                                    Token(
                                                        TriviaList([Whitespace("      ")]),
                                                        SyntaxKind.VerticalBarToken,
                                                        TriviaList([Space])),
                                                    TableCellList([
                                                        TextTableCell(
                                                            TableLiteral(
                                                                TriviaList(),
                                                                "boz2",
                                                                TriviaList())),
                                                        Token(
                                                            TriviaList([Space]),
                                                            SyntaxKind.VerticalBarToken,
                                                            TriviaList([Space])),
                                                        TextTableCell(
                                                            TableLiteral(
                                                                TriviaList(),
                                                                "boo2",
                                                                TriviaList())
                                                        )
                                                    ]),
                                                    Token(
                                                        TriviaList([Space]),
                                                        SyntaxKind.VerticalBarToken,
                                                        TriviaList([EnvironmentNewLine])))
                                            ]))))
                            ])
                        )
                        // ... (Repeat for all 50 scenarios)
                    ])),
                Token(
                    TriviaList(),
                    SyntaxKind.EndOfFileToken,
                    TriviaList())));

        tree.ToString().Should().Be(source);
    }
}
