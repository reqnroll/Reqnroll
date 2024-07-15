using FluentAssertions;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.Text;
using Xunit.Abstractions;

namespace Reqnroll.Analyzers;

public class StepAnalyzerTests(ITestOutputHelper output)
{
    private readonly AnalyzerTestHarness<StepAnalyzer> _testHarness = new(output);

    [Fact]
    public async Task StepWithNoGroupsOnMethodWithNoArguments_GeneratesNoDiagnostics()
    {
        const string source = 
            """
            using Reqnroll;

            namespace Test;

            [Binding]
            public class Steps
            {
                [Given("a step exists with no arguments")]
                public void AStepExistsWithNoArguments()
                {
                }
            }
            """;

        var syntax = SyntaxFactory.ParseSyntaxTree(SourceText.From(source));

        var result = await _testHarness.RunAnalyzerAsync(syntax);

        result.GetAllDiagnostics().Should().BeEmpty();
    }

    [Fact]
    public async Task StepWithCucumberParameterOnMethodWithNoArguments_GeneratesExpressionParameterCountMismatchDiagnostic()
    {
        const string source =
            """
            using Reqnroll;

            namespace Test;

            [Binding]
            public class Steps
            {
                [Given("a step exists with {int} arguments")]
                public void AStepExistsWithArguments()
                {
                }
            }
            """;

        var syntax = SyntaxFactory.ParseSyntaxTree(SourceText.From(source));

        var result = await _testHarness.RunAnalyzerAsync(syntax);
        var expected = Diagnostic.Create(
                DiagnosticDescriptors.ExpressionParameterCountMismatch,
                Location.Create(syntax, TextSpan.FromBounds(83, 83 + 36)));

        result.GetAllDiagnostics().Should().BeEquivalentTo([expected]);
    }

    [Fact]
    public async Task StepWithRegexCaptureGroupOnMethodWithNoArguments_GeneratesExpressionParameterCountMismatchDiagnostic()
    {
        const string source =
            """
            using Reqnroll;

            namespace Test;

            [Binding]
            public class Steps
            {
                [Given("a step exists with (\n+) arguments")]
                public void AStepExistsWithArguments()
                {
                }
            }
            """;

        var syntax = SyntaxFactory.ParseSyntaxTree(SourceText.From(source));

        var result = await _testHarness.RunAnalyzerAsync(syntax);
        var expected = Diagnostic.Create(
                DiagnosticDescriptors.ExpressionParameterCountMismatch,
                Location.Create(syntax, TextSpan.FromBounds(83, 83 + 36)));

        result.GetAllDiagnostics().Should().BeEquivalentTo([expected]);
    }
}
