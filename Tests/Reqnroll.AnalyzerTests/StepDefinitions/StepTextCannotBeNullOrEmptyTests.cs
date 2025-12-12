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
    public async Task FixingStepBindingWithNullTextRemovesTextArgument()
    {
        var test = new ReqnrollCSharpCodeFixTest<StepTextAnalyzer, StepTextCodeFixProvider>
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
                """",

            FixedCode =
                """"
                using Reqnroll;
                namespace Sample.Tests;
                [Binding]
                public class GameSteps
                {
                    [When]
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
    public async Task FixingStepBindingWithDefaultTextRemovesTextArgument()
    {
        var test = new ReqnrollCSharpCodeFixTest<StepTextAnalyzer, StepTextCodeFixProvider>
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
                """",

            FixedCode =
                """"
                using Reqnroll;
                namespace Sample.Tests;
                [Binding]
                public class GameSteps
                {
                    [When]
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

    [Fact]
    public async Task FixingStepBindingWithEmptyTextRemovesTextArgument()
    {
        var test = new ReqnrollCSharpCodeFixTest<StepTextAnalyzer, StepTextCodeFixProvider>
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
                """",

            FixedCode =
                """"
                using Reqnroll;
                namespace Sample.Tests;
                [Binding]
                public class GameSteps
                {
                    [When]
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
