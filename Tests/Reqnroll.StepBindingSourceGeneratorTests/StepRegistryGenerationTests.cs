using Microsoft.CodeAnalysis.Text;

namespace Reqnroll.StepBindingSourceGenerator;

public class StepRegistryGenerationTests
{
    [Fact]
    public void HavingAStepMethodCausesAStepRegistryToBeGenerated()
    {
        var source = SourceText.From(
            $$"""
            using Reqnroll;

            namespace Sample.Tests;

            [Binding]
            public class GameSteps
            {
                [When("Maker starts a game")]
                public void WhenMakerStartsAGame()
                {
                }
            }
            """);

        var result = GeneratorDriver.RunGenerator(source, "/spec/Sample.feature");

        result.Diagnostics.Should().BeEmpty();

        using var assemblyContext = result.CompileAssembly();

        assemblyContext.Assembly.ExportedTypes.Should()
            .Contain(type => type.Name == "ReqnrollStepRegistry" && type.Namespace == "Sample.Tests");
    }
}
