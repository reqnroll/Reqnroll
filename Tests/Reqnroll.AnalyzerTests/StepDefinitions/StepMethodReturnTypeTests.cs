using Microsoft.CodeAnalysis.Testing;

namespace Reqnroll.Analyzers.StepDefinitions;

public class StepMethodReturnTypeTests
{
    [Fact]
    public async Task StepMethodReturningVoidDoesNotRaiseDiagnostic()
    {
        var test = new ReqnrollCSharpAnalyzerTest<StepDefinitionMethodReturnTypeAnalyzer>
        {
            TestCode =
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

        await test.RunAsync();
    }

    [Fact]
    public async Task StepMethodReturningTaskDoesNotRaiseDiagnostic()
    {
        var test = new ReqnrollCSharpAnalyzerTest<StepDefinitionMethodReturnTypeAnalyzer>
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
                    public Task WhenMakerStartsAGame()
                    {
                        return Task.CompletedTask;
                    }
                }
                """"
        };

        await test.RunAsync();
    }

    [Fact]
    public async Task StepMethodReturningValueTaskDoesNotRaiseDiagnostic()
    {
        var test = new ReqnrollCSharpAnalyzerTest<StepDefinitionMethodReturnTypeAnalyzer>
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
                    public ValueTask WhenMakerStartsAGame()
                    {
                        return ValueTask.CompletedTask;
                    }
                }
                """"
        };

        await test.RunAsync();
    }

    [Fact]
    public async Task StepMethodReturningValueRaisesDiagnostic()
    {
        var test = new ReqnrollCSharpAnalyzerTest<StepDefinitionMethodReturnTypeAnalyzer>
        {
            TestCode =
                """"
                using Reqnroll;

                namespace Sample.Tests;

                [Binding]
                public class GameSteps
                {
                    [When]
                    public string {|#0:WhenMakerStartsAGame|}()
                    {
                        return "Game started";
                    }
                }
                """"
        };

        test.ExpectedDiagnostics.Add(
            new DiagnosticResult(StepDefinitionMethodReturnTypeAnalyzer.Rule)
                .WithLocation(0)
                .WithArguments("GameSteps.WhenMakerStartsAGame", "void"));

        await test.RunAsync();
    }
}
