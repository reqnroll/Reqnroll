using Microsoft.CodeAnalysis.Text;
using Reqnroll.Bindings;

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

    [Fact]
    public void GeneratedRegistryImplementsIStepDefinitionProvider()
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

        var registryType = assemblyContext.Assembly.ExportedTypes.Single(type => type.Name == "ReqnrollStepRegistry");

        registryType.Should().Implement<IStepDefinitionProvider>();
    }

    [Fact]
    public void GeneratedRegistryIsAvailableThroughInstanceProperty()
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

        var registryType = assemblyContext.Assembly.ExportedTypes.Single(type => type.Name == "ReqnrollStepRegistry");

        registryType.Should().HaveProperty(registryType, "Instance").Which.Should().BeReadable();

        var registry = registryType.GetProperty("Instance")!.GetGetMethod()!.Invoke(null, null) as IStepDefinitionProvider;

        registry.Should().NotBeNull();
    }
}
