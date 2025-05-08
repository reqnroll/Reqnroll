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
        var test = new ReqnrollCSharpAnalyzerTest<StepTextAnalyzer>
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
            new DiagnosticResult(StepTextAnalyzer.StepTextShouldNotHaveLeadingWhitespaceRule)
                .WithLocation(0));

        await test.RunAsync();
    }

    [Theory]
    [InlineData(" ")]
    [InlineData("\t")]
    [InlineData("    ")]
    public async Task StepBindingWithTrailingWhitespaceTextRaisesDiagnostic(string whitespace)
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
                    [When({|#0:"maker starts a game{{whitespace}}"|})]
                    public void WhenMakerStartsAGame()
                    {
                    }
                }
                """"
        };

        test.ExpectedDiagnostics.Add(
            new DiagnosticResult(StepTextAnalyzer.StepTextShouldNotHaveTrailingWhitespaceRule)
                .WithLocation(0));

        await test.RunAsync();
    }
}
