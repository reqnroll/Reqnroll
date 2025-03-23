using Microsoft.CodeAnalysis.CSharp.Testing;
using Microsoft.CodeAnalysis.Testing;

namespace Reqnroll.Analyzers.StepDefinitions;

public class StepMethodMustReturnVoidOrTaskTests
{
    [Fact]
    public async Task StepMethodReturningVoidDoesNotRaiseDiagnostic()
    {
        var test = new ReqnrollCSharpAnalyzerTest<StepMethodMustReturnVoidOrTaskAnalyzer>
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
        var test = new ReqnrollCSharpAnalyzerTest<StepMethodMustReturnVoidOrTaskAnalyzer>
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
    public async Task StepMethodReturningValueRaisesDiagnostic()
    {
        var reqnrollAssemblyLocation = typeof(WhenAttribute).Assembly.Location;
        var reqnrollAssembly = Path.Combine(
            Path.GetDirectoryName(reqnrollAssemblyLocation)!,
            Path.GetFileNameWithoutExtension(reqnrollAssemblyLocation));

        var test = new CSharpAnalyzerTest<StepMethodMustReturnVoidOrTaskAnalyzer, DefaultVerifier>
        {
            ReferenceAssemblies = ReferenceAssemblies.Net.Net80.AddAssemblies([reqnrollAssembly]),

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
            new DiagnosticResult(StepMethodMustReturnVoidOrTaskAnalyzer.Rule)
                .WithLocation(0)
                .WithArguments("GameSteps.WhenMakerStartsAGame", "void"));

        await test.RunAsync();
    }
}
