using Reqnroll.CodeAnalysis.Gherkin.Syntax;

namespace Reqnroll.CodeAnalysis.Gherkin.Parsing;

using static SyntaxFactory;

public partial class DocStringsTests
{
    [Fact]
    public void DocStringsAreRepresentedInTree()
    {
        // Taken from good/docstrings.feature
        const string source =
            """"
            Feature: DocString variations

              Scenario: minimalistic
                Given a simple DocString
                  """
                  first line (no indent)
                    second line (indented with two spaces)

                  third line was empty
                  """
                Given a DocString with content type
                  """xml
                  <foo>
                    <bar />
                  </foo>
                  """
                And a DocString with wrong indentation
                  """
                wrongly indented line
                  """
                And a DocString with alternative separator
                  ```
                  first line
                  second line
                  ```
                And a DocString with normal separator inside
                  ```
                  first line
                  """
                  third line
                  ```
                And a DocString with alternative separator inside
                  """
                  first line
                  ```
                  third line
                  """
                And a DocString with escaped separator inside
                  """
                  first line
                  \"\"\" 
                  third line
                  """
                And a DocString with an escaped alternative separator inside
                  ```
                  first line
                  \`\`\`
                  third line
                  ```
            """";

        var tree = GherkinSyntaxTree.ParseText(source);

        tree.GetRoot().Should().BeEquivalentTo(
            GherkinDocument(
                Feature(
                    Token(
                        TriviaList(),
                        SyntaxKind.FeatureKeyword,
                        "Feature",
                        TriviaList()),
                    ColonWithSpace,
                    Name(
                        TriviaList(),
                        "DocString variations",
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
                                "minimalistic",
                                TriviaList([EnvironmentNewLine])),
                            steps: List([
                                Step(
                                    Token(
                                        TriviaList([Whitespace("    ")]),
                                        SyntaxKind.ContextStepKeyword,
                                        "Given",
                                        TriviaList([Space])),
                                    StepText(
                                        TriviaList(),
                                        "a simple DocString",
                                        TriviaList([EnvironmentNewLine])),
                                    StepDocString(
                                        DocString(
                                            Token(
                                                TriviaList([Whitespace("      ")]),
                                                SyntaxKind.DocStringDelimiterToken,
                                                "\"\"\"",
                                                TriviaList([EnvironmentNewLine])),
                                            Literal(
                                                TriviaList(),
                                                SyntaxKind.DocStringContentToken,
                                                "      first line (no indent)" + Environment.NewLine +
                                                "        second line (indented with two spaces)" + Environment.NewLine +
                                                Environment.NewLine +
                                                "      third line was empty" + Environment.NewLine,
                                                "first line (no indent)" + Environment.NewLine +
                                                "  second line (indented with two spaces)" + Environment.NewLine +
                                                Environment.NewLine +
                                                "third line was empty" + Environment.NewLine,
                                                TriviaList()),
                                            Token(
                                                TriviaList([Whitespace("      ")]),
                                                SyntaxKind.DocStringDelimiterToken,
                                                "\"\"\"",
                                                TriviaList([EnvironmentNewLine]))))),
                                Step(
                                    Token(
                                        TriviaList([Whitespace("  ")]),
                                        SyntaxKind.ContextStepKeyword,
                                        "Given",
                                        TriviaList([Space])),
                                    StepText(
                                        TriviaList(),
                                        "a DocString with content type",
                                        TriviaList([EnvironmentNewLine])),
                                    StepDocString(
                                        DocString(
                                            Token(
                                                TriviaList([Whitespace("      ")]),
                                                SyntaxKind.DocStringDelimiterToken,
                                                "\"\"\"",
                                                TriviaList()),
                                            Token(
                                                TriviaList(),
                                                SyntaxKind.DocStringContentTypeIdentifierToken,
                                                "xml",
                                                TriviaList([EnvironmentNewLine])),
                                            Literal(
                                                TriviaList(),
                                                SyntaxKind.DocStringContentToken,
                                                "      <foo>" + Environment.NewLine +
                                                "        <bar />"  + Environment.NewLine +
                                                "      </foo>"  + Environment.NewLine,
                                                "<foo>" + Environment.NewLine +
                                                "  <bar />"  + Environment.NewLine +
                                                "</foo>"  + Environment.NewLine,
                                                TriviaList()),
                                            Token(
                                                TriviaList([Whitespace("    ")]),
                                                SyntaxKind.DocStringDelimiterToken,
                                                "\"\"\"",
                                                TriviaList([EnvironmentNewLine]))))),
                                Step(
                                    Token(
                                        TriviaList([Whitespace("    ")]),
                                        SyntaxKind.ConjunctionStepKeyword,
                                        "And",
                                        TriviaList([Space])),
                                    StepText(
                                        TriviaList(),
                                        "a DocString with wrong indentation",
                                        TriviaList([EnvironmentNewLine])),
                                    StepDocString(
                                        DocString(
                                            Token(
                                                TriviaList([Whitespace("      ")]),
                                                SyntaxKind.DocStringDelimiterToken,
                                                "\"\"\"",
                                                TriviaList([EnvironmentNewLine])),
                                            Literal(
                                                TriviaList(),
                                                SyntaxKind.DocStringContentToken,
                                                "    wrongly indented line" + Environment.NewLine,
                                                "wrongly indented line",
                                                TriviaList()),
                                            Token(
                                                TriviaList([Whitespace("      ")]),
                                                SyntaxKind.DocStringDelimiterToken,
                                                "\"\"\"",
                                                TriviaList([EnvironmentNewLine]))))),
                                Step(
                                    Token(
                                        TriviaList([Whitespace("  ")]),
                                        SyntaxKind.ConjunctionStepKeyword,
                                        "And",
                                        TriviaList([Space])),
                                    StepText(
                                        TriviaList(),
                                        "a DocString with alternative separator",
                                        TriviaList([EnvironmentNewLine])),
                                    StepDocString(
                                        DocString(
                                            Token(
                                                TriviaList([Whitespace("      ")]),
                                                SyntaxKind.DocStringDelimiterToken,
                                                "```",
                                                TriviaList([EnvironmentNewLine])),
                                            Literal(
                                                TriviaList(),
                                                SyntaxKind.DocStringContentToken,
                                                "      first line" + Environment.NewLine +
                                                "      second line" + Environment.NewLine,
                                                "first line" + Environment.NewLine +
                                                "second line" + Environment.NewLine,
                                                TriviaList()),
                                            Token(
                                                TriviaList([Whitespace("      ")]),
                                                SyntaxKind.DocStringDelimiterToken,
                                                "```",
                                                TriviaList([EnvironmentNewLine]))))),
                                Step(
                                    Token(
                                        TriviaList([Whitespace("    ")]),
                                        SyntaxKind.ConjunctionStepKeyword,
                                        "And",
                                        TriviaList([Space])),
                                    StepText(
                                        TriviaList(),
                                        "a DocString with normal separator inside",
                                        TriviaList([EnvironmentNewLine])),
                                    StepDocString(
                                        DocString(
                                            Token(
                                                TriviaList([Whitespace("      ")]),
                                                SyntaxKind.DocStringDelimiterToken,
                                                "```",
                                                TriviaList([EnvironmentNewLine])),
                                            Literal(
                                                TriviaList(),
                                                SyntaxKind.DocStringContentToken,
                                                "      first line" + Environment.NewLine +
                                                "      \"\"\"" + Environment.NewLine +
                                                "      third line" + Environment.NewLine,
                                                "first line" + Environment.NewLine +
                                                "\"\"\"" + Environment.NewLine +
                                                "third line" + Environment.NewLine,
                                                TriviaList()),
                                            Token(
                                                TriviaList([Whitespace("      ")]),
                                                SyntaxKind.DocStringDelimiterToken,
                                                "```",
                                                TriviaList([EnvironmentNewLine]))))),
                                Step(
                                    Token(
                                        TriviaList([Whitespace("    ")]),
                                        SyntaxKind.ConjunctionStepKeyword,
                                        "And",
                                        TriviaList([Space])),
                                    StepText(
                                        TriviaList(),
                                        "a DocString with alternative separator inside",
                                        TriviaList([EnvironmentNewLine])),
                                    StepDocString(
                                        DocString(
                                            Token(
                                                TriviaList([Whitespace("      ")]),
                                                SyntaxKind.DocStringDelimiterToken,
                                                "\"\"\"",
                                                TriviaList([EnvironmentNewLine])),
                                            Literal(
                                                TriviaList(),
                                                SyntaxKind.DocStringContentToken,
                                                "      first line" + Environment.NewLine +
                                                "      ```" + Environment.NewLine +
                                                "      third line" + Environment.NewLine,
                                                "first line" + Environment.NewLine +
                                                "```" + Environment.NewLine +
                                                "third line" + Environment.NewLine,
                                                TriviaList()),
                                            Token(
                                                TriviaList([Whitespace("      ")]),
                                                SyntaxKind.DocStringDelimiterToken,
                                                "\"\"\"",
                                                TriviaList([EnvironmentNewLine]))))),
                                Step(
                                    Token(
                                        TriviaList([Whitespace("    ")]),
                                        SyntaxKind.ConjunctionStepKeyword,
                                        "And",
                                        TriviaList([Space])),
                                    StepText(
                                        TriviaList(),
                                        "a DocString with escaped separator inside",
                                        TriviaList([EnvironmentNewLine])),
                                    StepDocString(
                                        DocString(
                                            Token(
                                                TriviaList([Whitespace("      ")]),
                                                SyntaxKind.DocStringDelimiterToken,
                                                "\"\"\"",
                                                TriviaList([EnvironmentNewLine])),
                                            Literal(
                                                TriviaList(),
                                                SyntaxKind.DocStringContentToken,
                                                "      first line" + Environment.NewLine +
                                                "      \\\"\\\"\\\"" + Environment.NewLine +
                                                "      third line" + Environment.NewLine,
                                                "first line" + Environment.NewLine +
                                                "\"\"\"" + Environment.NewLine +
                                                "third line" + Environment.NewLine,
                                                TriviaList()),
                                            Token(
                                                TriviaList([Whitespace("      ")]),
                                                SyntaxKind.DocStringDelimiterToken,
                                                "\"\"\"",
                                                TriviaList([EnvironmentNewLine]))))),
                                Step(
                                    Token(
                                        TriviaList([Whitespace("  ")]),
                                        SyntaxKind.ConjunctionStepKeyword,
                                        "And",
                                        TriviaList([Space])),
                                    StepText(
                                        TriviaList(),
                                        "a DocString with an escaped alternative separator inside",
                                        TriviaList([EnvironmentNewLine])),
                                    StepDocString(
                                        DocString(
                                            Token(
                                                TriviaList([Whitespace("      ")]),
                                                SyntaxKind.DocStringDelimiterToken,
                                                "```",
                                                TriviaList([EnvironmentNewLine])),
                                            Literal(
                                                TriviaList(),
                                                SyntaxKind.DocStringContentToken,
                                                "      first line" + Environment.NewLine +
                                                "      \\`\\`\\`" + Environment.NewLine +
                                                "      third line" + Environment.NewLine,
                                                "first line" + Environment.NewLine +
                                                "```" + Environment.NewLine +
                                                "third line" + Environment.NewLine,
                                                TriviaList()),
                                            Token(
                                                TriviaList([Whitespace("      ")]),
                                                SyntaxKind.DocStringDelimiterToken,
                                                "```",
                                                TriviaList([EnvironmentNewLine])))))
                            ]))
                    ])),
                Token(
                    TriviaList(),
                    SyntaxKind.EndOfFileToken,
                    TriviaList()))
        );

        tree.ToString().Should().Be(source);
    }
}
