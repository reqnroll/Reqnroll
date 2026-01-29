using Microsoft.CodeAnalysis.Testing;

namespace Reqnroll.Analyzers.StepDefinitions;

public class StepTextShouldNotHaveLeadingOrTrailingWhitespaceTests
{
    [Theory]
    [InlineData(" ")]
    [InlineData("\t")]
    [InlineData("    ")]
    public async Task StepBindingWithLeadingWhitespaceTextRaisesDiagnostic(string whitespace)
    {
        var test = new ReqnrollCSharpAnalyzerTest<StepDefinitionExpressionAnalyzer>
        {
            TestCode =
                $$""""
                using Reqnroll;

                namespace Sample.Tests;

                [Binding]
                public class GameSteps
                {
                    [When({|#0:"{{whitespace}}maker starts a game"|})]
                    public void WhenMakerStartsAGame()
                    {
                    }
                }
                """"
        };

        test.ExpectedDiagnostics.Add(
            new DiagnosticResult(StepDefinitionExpressionAnalyzer.StepDefinitionExpressionShouldNotHaveLeadingOrTrailingWhitespaceRule)
                .WithLocation(0));

        await test.RunAsync();
    }

    [Theory]
    [InlineData(" ")]
    [InlineData("\t")]
    [InlineData("    ")]
    public async Task FixingStepBindingWithLeadingWhitespaceTrimsStepText(string whitespace)
    {
        var test = new ReqnrollCSharpCodeFixTest<StepDefinitionExpressionAnalyzer, StepDefinitionExpressionCodeFixProvider>
        {
            TestCode =
                $$""""
                using Reqnroll;

                namespace Sample.Tests;

                [Binding]
                public class GameSteps
                {
                    [When({|#0:"{{whitespace}}maker starts a game"|})]
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
                    [When("maker starts a game")]
                    public void WhenMakerStartsAGame()
                    {
                    }
                }
                """"
        };

        test.ExpectedDiagnostics.Add(
            new DiagnosticResult(StepDefinitionExpressionAnalyzer.StepDefinitionExpressionShouldNotHaveLeadingOrTrailingWhitespaceRule)
                .WithLocation(0));

        await test.RunAsync();
    }

    [Theory]
    [InlineData(" ")]
    [InlineData("\t")]
    [InlineData("    ")]
    public async Task StepBindingWithTrailingWhitespaceTextRaisesDiagnostic(string whitespace)
    {
        var test = new ReqnrollCSharpAnalyzerTest<StepDefinitionExpressionAnalyzer>
        {
            TestCode =
                $$""""
                using Reqnroll;

                namespace Sample.Tests;

                [Binding]
                public class GameSteps
                {
                    [When({|#0:"maker starts a game{{whitespace}}"|})]
                    public void WhenMakerStartsAGame()
                    {
                    }
                }
                """"
        };

        test.ExpectedDiagnostics.Add(
            new DiagnosticResult(StepDefinitionExpressionAnalyzer.StepDefinitionExpressionShouldNotHaveLeadingOrTrailingWhitespaceRule)
                .WithLocation(0));

        await test.RunAsync();
    }

    [Theory]
    [InlineData(" ")]
    [InlineData("\t")]
    [InlineData("    ")]
    public async Task FixingStepBindingWithTrailingWhitespaceTrimsStepText(string whitespace)
    {
        var test = new ReqnrollCSharpCodeFixTest<StepDefinitionExpressionAnalyzer, StepDefinitionExpressionCodeFixProvider>
        {
            TestCode =
                $$""""
                using Reqnroll;

                namespace Sample.Tests;

                [Binding]
                public class GameSteps
                {
                    [When({|#0:"maker starts a game{{whitespace}}"|})]
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
                    [When("maker starts a game")]
                    public void WhenMakerStartsAGame()
                    {
                    }
                }
                """"
        };

        test.ExpectedDiagnostics.Add(
            new DiagnosticResult(StepDefinitionExpressionAnalyzer.StepDefinitionExpressionShouldNotHaveLeadingOrTrailingWhitespaceRule)
                .WithLocation(0));

        await test.RunAsync();
    }

    [Theory]
    [InlineData(" ")]
    [InlineData("\t")]
    [InlineData("    ")]
    public async Task StepBindingWithLeadingAndTrailingWhitespaceTextRaisesSingleDiagnostic(string whitespace)
    {
        var test = new ReqnrollCSharpAnalyzerTest<StepDefinitionExpressionAnalyzer>
        {
            TestCode =
                $$""""
                using Reqnroll;

                namespace Sample.Tests;

                [Binding]
                public class GameSteps
                {
                    [When({|#0:"{{whitespace}}maker starts a game{{whitespace}}"|})]
                    public void WhenMakerStartsAGame()
                    {
                    }
                }
                """"
        };

        test.ExpectedDiagnostics.Add(
            new DiagnosticResult(StepDefinitionExpressionAnalyzer.StepDefinitionExpressionShouldNotHaveLeadingOrTrailingWhitespaceRule)
                .WithLocation(0));

        await test.RunAsync();
    }

    [Theory]
    [InlineData(" ")]
    [InlineData("\t")]
    [InlineData("    ")]
    public async Task FixingStepBindingWithLeadingAndTrailingWhitespaceTrimsStepText(string whitespace)
    {
        var test = new ReqnrollCSharpCodeFixTest<StepDefinitionExpressionAnalyzer, StepDefinitionExpressionCodeFixProvider>
        {
            TestCode =
                $$""""
                using Reqnroll;

                namespace Sample.Tests;

                [Binding]
                public class GameSteps
                {
                    [When({|#0:"{{whitespace}}maker starts a game{{whitespace}}"|})]
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
                    [When("maker starts a game")]
                    public void WhenMakerStartsAGame()
                    {
                    }
                }
                """"
        };

        test.ExpectedDiagnostics.Add(
            new DiagnosticResult(StepDefinitionExpressionAnalyzer.StepDefinitionExpressionShouldNotHaveLeadingOrTrailingWhitespaceRule)
                .WithLocation(0));

        await test.RunAsync();
    }
}
