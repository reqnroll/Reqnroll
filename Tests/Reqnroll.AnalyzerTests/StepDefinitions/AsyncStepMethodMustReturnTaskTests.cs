using Microsoft.CodeAnalysis.Testing;

namespace Reqnroll.Analyzers.StepDefinitions;

public class AsyncStepMethodMustReturnTaskTests
{
    [Fact]
    public async Task AsyncStepMethodReturningTaskDoesNotRaiseDiagnostic()
    {
        var test = new ReqnrollCSharpAnalyzerTest<AsyncStepMethodMustReturnTaskAnalyzer>
        {
            TestCode =
                """"
                using System.Threading.Tasks;
                using Reqnroll;

                namespace Sample.Tests;

                [Binding]
                public class GameSteps
                {
                    [When]
                    public async Task WhenMakerStartsAGame()
                    {
                        await Task.CompletedTask;
                    }
                }
                """"
        };

        await test.RunAsync();
    }

    [Fact]
    public async Task AsyncStepMethodReturningVoidRaisesDiagnostic()
    {
        var test = new ReqnrollCSharpAnalyzerTest<AsyncStepMethodMustReturnTaskAnalyzer>
        {
            TestCode =
                """"
                using System.Threading.Tasks;
                using Reqnroll;

                namespace Sample.Tests;

                [Binding]
                public class GameSteps
                {
                    [When]
                    public async void {|#0:WhenMakerStartsAGame|}()
                    {
                        await Task.CompletedTask;
                    }
                }
                """"
        };

        test.ExpectedDiagnostics.Add(
            new DiagnosticResult(AsyncStepMethodMustReturnTaskAnalyzer.Rule)
                .WithLocation(0)
                .WithArguments("GameSteps.WhenMakerStartsAGame", "async"));

        await test.RunAsync();
    }
}
