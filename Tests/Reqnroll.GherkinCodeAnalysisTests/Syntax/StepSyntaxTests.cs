namespace Reqnroll.CodeAnalysis.Gherkin.Syntax;

using Microsoft.CodeAnalysis.Text;
using static SyntaxFactory;

public class StepSyntaxTests
{
    [Fact]
    public void StepWithTableDataHasCorrectOffsets()
    {
        var step = Step(
            Token(TriviaList(), SyntaxKind.ContextStepKeyword, "Given", TriviaList([Space])),
            StepText(TriviaList(), "a table with data", TriviaList([EnvironmentNewLine])),
            StepTable(
                Table(
                    List([
                        TableRow(
                            VerticalBar,
                            TableCellList([ TableCell("foo"), TableCell("bar") ]),
                            VerticalBar.WithTrailingTrivia(TriviaList([EnvironmentNewLine]))),
                        TableRow(
                            VerticalBar,
                            TableCellList([TableCell("baz"), TableCell("bop") ]),
                            VerticalBar.WithTrailingTrivia(TriviaList([EnvironmentNewLine])))
                    ]))));

        step.StepKeyword.Span.Should().Be(new TextSpan(0, 5));
        step.StepKeyword.FullSpan.Should().Be(new TextSpan(0, 6));

        step.Text.Span.Should().Be(new TextSpan(6, 17));
        step.Text.FullSpan.Should().Be(new TextSpan(6, 19));

        var data = (StepTableSyntax)step.Data!;

        data.Span.Should().Be(new TextSpan(25, 20));
        data.FullSpan.Should().Be(new TextSpan(25, 22));

        data.Table.Span.Should().Be(new TextSpan(25, 20));
        data.Table.FullSpan.Should().Be(new TextSpan(25, 22));

        var row0 = data.Table.Rows[0];
        var row1 = data.Table.Rows[1];

        row0.Span.Should().Be(new TextSpan(25, 9));
        row0.FullSpan.Should().Be(new TextSpan(25, 11));

        row1.Span.Should().Be(new TextSpan(36, 9));
        row1.FullSpan.Should().Be(new TextSpan(36, 11));
    }
}
