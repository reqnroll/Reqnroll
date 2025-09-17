using Microsoft.CodeAnalysis.Text;
using Reqnroll.Bindings;

namespace Reqnroll.StepBindingSourceGenerator;

public class StepRegistryGenerationTests
{
    [Fact]
    public void HavingAStepMethodCausesAStepRegistryToBeGenerated()
    {
        var source = SourceText.From(
            """
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
            """
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

        registryType.Should().Implement<IStepDefinitionDescriptorsProvider>();
    }

    [Fact]
    public void GeneratedRegistryIsAvailableThroughInstanceProperty()
    {
        var source = SourceText.From(
            """
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

        var registry = registryType.GetProperty("Instance")!.GetGetMethod()!.Invoke(null, null) as IStepDefinitionDescriptorsProvider;

        registry.Should().NotBeNull();
    }

    [Fact]
    public void StepDefinitionWithNoArgumentsIsAddedToRegistry()
    {
        var source = SourceText.From(
            """
            using Reqnroll;

            namespace Sample.Tests;

            [Binding]
            public class GameSteps
            {
                [When("Maker starts a game")]
                public void WhenMakerStartsAGame()
                {
                    throw new System.NotImplementedException("f1e3f2b2-a99b-4d87-9fff-139128a61d5b");
                }
            }
            """);

        var result = GeneratorDriver.RunGenerator(source, "/spec/Sample.feature");

        result.Diagnostics.Should().BeEmpty();

        using var assemblyContext = result.CompileAssembly();

        var registry = assemblyContext.Assembly.GetStepRegistry();

        registry.GetStepDefinitions().Should().HaveCount(1);
        
        var stepDefinition = registry.GetStepDefinitions().Single();

        stepDefinition.Should().BeEquivalentTo(
            new StepDefinitionDescriptor(
                "When Maker starts a game",
                [StepDefinitionType.When],
                StepTextPattern.CucumberExpression("Maker starts a game")));
    }

    [Fact]
    public void StepDefinitionWithEscapeInRegularExpressionIsAddedToRegistry()
    {
        var source = SourceText.From(
            """
            using Reqnroll;

            namespace Sample.Tests;

            [Binding]
            public class GameSteps
            {
                [Given(@"the app\.config is used for configuration")]
                public void TheAppConfigIsUsedForConfiguration()
                {
                    throw new System.NotImplementedException("f1e3f2b2-a99b-4d87-9fff-139128a61d5b");
                }
            }
            """);

        var result = GeneratorDriver.RunGenerator(source, "/spec/Sample.feature");

        result.Diagnostics.Should().BeEmpty();

        using var assemblyContext = result.CompileAssembly();

        var registry = assemblyContext.Assembly.GetStepRegistry();

        registry.GetStepDefinitions().Should().HaveCount(1);

        var stepDefinition = registry.GetStepDefinitions().Single();

        stepDefinition.Should().BeEquivalentTo(
            new StepDefinitionDescriptor(
                "Given the app.config is used for configuration",
                [StepDefinitionType.Given],
                StepTextPattern.RegularExpression("the app\\.config is used for configuration")));
    }
}
