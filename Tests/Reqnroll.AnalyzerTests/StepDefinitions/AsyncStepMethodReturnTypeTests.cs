using Microsoft.CodeAnalysis.Testing;

namespace Reqnroll.Analyzers.StepDefinitions;

public class AsyncStepMethodReturnTypeTests
{
    [Fact]
    public async Task AsyncStepMethodReturningTaskDoesNotRaiseDiagnostic()
    {
        var test = new ReqnrollCSharpAnalyzerTest<AsyncStepMethodReturnTypeAnalyzer>
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
    public async Task AsyncStepMethodReturningTaskOfTDoesNotRaiseDiagnostic()
    {
        var test = new ReqnrollCSharpAnalyzerTest<AsyncStepMethodReturnTypeAnalyzer>
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
                    public async Task<string> WhenMakerStartsAGame()
                    {
                        return await Task.FromResult("Foo");
                    }
                }
                """"
        };

        await test.RunAsync();
    }

    [Fact]
    public async Task AsyncStepMethodReturningValueTaskDoesNotRaiseDiagnostic()
    {
        var test = new ReqnrollCSharpAnalyzerTest<AsyncStepMethodReturnTypeAnalyzer>
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
                    public async ValueTask WhenMakerStartsAGame()
                    {
                        await ValueTask.CompletedTask;
                    }
                }
                """"
        };

        await test.RunAsync();
    }

    [Fact]
    public async Task AsyncStepMethodReturningValueTaskOfTDoesNotRaiseDiagnostic()
    {
        var test = new ReqnrollCSharpAnalyzerTest<AsyncStepMethodReturnTypeAnalyzer>
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
                    public async ValueTask<string> WhenMakerStartsAGame()
                    {
                        return await new ValueTask<string>("Foo");
                    }
                }
                """"
        };

        await test.RunAsync();
    }

    [Fact]
    public async Task AsyncStepMethodReturningVoidRaisesDiagnostic()
    {
        var test = new ReqnrollCSharpAnalyzerTest<AsyncStepMethodReturnTypeAnalyzer>
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
            new DiagnosticResult(AsyncStepMethodReturnTypeAnalyzer.Rule)
                .WithLocation(0)
                .WithArguments("GameSteps.WhenMakerStartsAGame", "async"));

        await test.RunAsync();
    }
}
