using Microsoft.CodeAnalysis.Testing;

namespace Reqnroll.Analyzers.StepDefinitions;

public class StepTextCannotBeNullOrEmptyTests
{
    [Fact]
    public async Task StepBindingWithNullTextRaisesDiagnostic()
    {
        var test = new ReqnrollCSharpAnalyzerTest<StepTextAnalyzer>
        {
            TestCode =
                """"
                using Reqnroll;

                namespace Sample.Tests;

                [Binding]
                public class GameSteps
                {
                    [When({|#0:null|})]
                    public void WhenMakerStartsAGame()
                    {
                    }
                }
                """"
        };

        test.ExpectedDiagnostics.Add(
            new DiagnosticResult(StepTextAnalyzer.StepTextCannotBeNullOrEmptyRule)
                .WithLocation(0));

        await test.RunAsync();
    }

    [Fact]
    public async Task StepBindingWithDefaultTextRaisesDiagnostic()
    {
        var test = new ReqnrollCSharpAnalyzerTest<StepTextAnalyzer>
        {
            TestCode =
                """"
                using Reqnroll;

                namespace Sample.Tests;

                [Binding]
                public class GameSteps
                {
                    [When({|#0:default|})]
                    public void WhenMakerStartsAGame()
                    {
                    }
                }
                """"
        };

        test.ExpectedDiagnostics.Add(
            new DiagnosticResult(StepTextAnalyzer.StepTextCannotBeNullOrEmptyRule)
                .WithLocation(0));

        await test.RunAsync();
    }

    [Fact]
    public async Task StepBindingWithEmptyTextRaisesDiagnostic()
    {
        var test = new ReqnrollCSharpAnalyzerTest<StepTextAnalyzer>
        {
            TestCode =
                """"
                using Reqnroll;

                namespace Sample.Tests;

                [Binding]
                public class GameSteps
                {
                    [When({|#0:""|})]
                    public void WhenMakerStartsAGame()
                    {
                    }
                }
                """"
        };

        test.ExpectedDiagnostics.Add(
            new DiagnosticResult(StepTextAnalyzer.StepTextCannotBeNullOrEmptyRule)
                .WithLocation(0));

        await test.RunAsync();
    }

    [Theory]
    [InlineData(" ")]
    [InlineData("\t")]
    [InlineData("    ")]
    public async Task StepBindingWithWhitespaceTextRaisesDiagnostic(string whitespace)
    {
        var test = new ReqnrollCSharpAnalyzerTest<StepTextAnalyzer>
        {
            TestCode =
                $$""""
                using Reqnroll;

                namespace Sample.Tests;

                [Binding]
                public class GameSteps
                {
                    [When({|#0:"{{whitespace}}"|})]
                    public void WhenMakerStartsAGame()
                    {
                    }
                }
                """"
        };

        test.ExpectedDiagnostics.Add(
            new DiagnosticResult(StepTextAnalyzer.StepTextCannotBeNullOrEmptyRule)
                .WithLocation(0));

        await test.RunAsync();
    }
}
