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

        var result = GeneratorDriver.RunGenerator(source);

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

        var result = GeneratorDriver.RunGenerator(source);

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

        var result = GeneratorDriver.RunGenerator(source);

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

        var result = GeneratorDriver.RunGenerator(source);

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
    public void StepDefinitionWithArgumentsIsAddedToRegistry()
    {
        var source = SourceText.From(
            """
            using Reqnroll;

            namespace Sample.Tests;

            [Binding]
            public class GameSteps
            {
                [When("Maker starts {int} games")]
                public void WhenMakerStartsGames(int count)
                {
                    throw new System.NotImplementedException("f1e3f2b2-a99b-4d87-9fff-139128a61d5b");
                }
            }
            """);

        var result = GeneratorDriver.RunGenerator(source);

        result.Diagnostics.Should().BeEmpty();

        using var assemblyContext = result.CompileAssembly();

        var registry = assemblyContext.Assembly.GetStepRegistry();

        registry.GetStepDefinitions().Should().HaveCount(1);

        var stepDefinition = registry.GetStepDefinitions().Single();

        stepDefinition.Should().BeEquivalentTo(
            new StepDefinitionDescriptor(
                "When Maker starts {int} games",
                [StepDefinitionType.When],
                StepTextPattern.CucumberExpression("Maker starts {int} games"),
                [
                    new StepParameterDescriptor("count", typeof(int))
                ]));
    }

    [Theory]
    [InlineData('\\')]
    [InlineData('^')]
    [InlineData('$')]
    [InlineData('.')]
    [InlineData('|')]
    [InlineData('?')]
    [InlineData('*')]
    [InlineData('+')]
    [InlineData('(')]
    [InlineData(')')]
    [InlineData('[')]
    [InlineData(']')]
    [InlineData('{')]
    [InlineData('}')]
    public void StepDefinitionWithEscapedSpecialCharacterInRegularExpressionIsAddedToRegistry(char c)
    {
        var source = SourceText.From(
            $$"""
            using Reqnroll;

            namespace Sample.Tests;

            [Binding]
            public class GameSteps
            {
                [Given(@"the app\{{c}}config is used for (.*) configuration")]
                public void TheAppConfigIsUsedForConfiguration(string s)
                {
                    throw new System.NotImplementedException("f1e3f2b2-a99b-4d87-9fff-139128a61d5b");
                }
            }
            """);

        var result = GeneratorDriver.RunGenerator(source);

        result.Diagnostics.Should().BeEmpty();

        using var assemblyContext = result.CompileAssembly();

        var registry = assemblyContext.Assembly.GetStepRegistry();

        registry.GetStepDefinitions().Should().HaveCount(1);

        var stepDefinition = registry.GetStepDefinitions().Single();

        stepDefinition.Should().BeEquivalentTo(
            new StepDefinitionDescriptor(
                $"Given the app\\{c}config is used for (.*) configuration",
                [StepDefinitionType.Given],
                StepTextPattern.RegularExpression($"the app\\{c}config is used for (.*) configuration"),
                [
                    new StepParameterDescriptor("s", typeof(string))
                ]));
    }

    [Theory]
    [InlineData('(')]
    [InlineData('{')]
    public void StepDefinitionWithEscapedSpecialCharacterInCucumberExpressionIsAddedToRegistry(char c)
    {
        var source = SourceText.From(
            $$"""
            using Reqnroll;

            namespace Sample.Tests;

            [Binding]
            public class GameSteps
            {
                [Given(@"the app\{{c}}config is used for {string} configuration")]
                public void TheAppConfigIsUsedForConfiguration(string s)
                {
                    throw new System.NotImplementedException("f1e3f2b2-a99b-4d87-9fff-139128a61d5b");
                }
            }
            """);

        var result = GeneratorDriver.RunGenerator(source);

        result.Diagnostics.Should().BeEmpty();

        using var assemblyContext = result.CompileAssembly();

        var registry = assemblyContext.Assembly.GetStepRegistry();

        registry.GetStepDefinitions().Should().HaveCount(1);

        var stepDefinition = registry.GetStepDefinitions().Single();

        stepDefinition.Should().BeEquivalentTo(
            new StepDefinitionDescriptor(
                $"Given the app\\{c}config is used for {{string}} configuration",
                [StepDefinitionType.Given],
                StepTextPattern.CucumberExpression($"the app\\{c}config is used for {{string}} configuration"),
                [
                    new StepParameterDescriptor("s", typeof(string))    
                ]));
    }

    [Fact]
    public void StepDefinitionsFromTwoStepClassesWithTheSameNameAreAddedToRegistry()
    {
        var source1 = SourceText.From(
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

        var source2 = SourceText.From(
            """
            using Reqnroll;

            namespace Sample.Other.Tests;

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

        var result = GeneratorDriver.RunGenerator([ source1, source2 ]);

        result.Diagnostics.Should().BeEmpty();

        using var assemblyContext = result.CompileAssembly();

        var registry = assemblyContext.Assembly.GetStepRegistry();

        registry.GetStepDefinitions().Should().HaveCount(2);

        var stepDefinition1 = registry.GetStepDefinitions().First();

        stepDefinition1.Should().BeEquivalentTo(
            new StepDefinitionDescriptor(
                "When Maker starts a game",
                [StepDefinitionType.When],
                StepTextPattern.CucumberExpression("Maker starts a game")));

        var stepDefinition2= registry.GetStepDefinitions().Last();

        stepDefinition2.Should().BeEquivalentTo(
            new StepDefinitionDescriptor(
                "When Maker starts a game",
                [StepDefinitionType.When],
                StepTextPattern.CucumberExpression("Maker starts a game")));
    }

    [Fact]
    public void StepDefinitionWithShorthandCharacterClassesInRegularExpressionIsAddedToRegistry()
    {
        var source = SourceText.From(
            """
            using Reqnroll;

            namespace Sample.Tests;

            [Binding]
            public class ScenarioSteps
            {
                [Given(@"there is a feature '(.*)' with (\d+) passing (\d+) failing (\d+) pending and (\d+) ignored scenarios")]
                public void GivenThereAreScenarios(string featureTitle, int passCount, int failCount, int pendingCount, int ignoredCount)
                {
                    throw new System.NotImplementedException("f1e3f2b2-a99b-4d87-9fff-139128a61d5b");
                }
            }
            """);

        var result = GeneratorDriver.RunGenerator(source);

        result.Diagnostics.Should().BeEmpty();

        using var assemblyContext = result.CompileAssembly();

        var registry = assemblyContext.Assembly.GetStepRegistry();

        registry.GetStepDefinitions().Should().HaveCount(1);

        var stepDefinition = registry.GetStepDefinitions().Single();

        stepDefinition.Should().BeEquivalentTo(
            new StepDefinitionDescriptor(
                "Given there is a feature '(.*)' with (\\d+) passing (\\d+) failing (\\d+) pending and (\\d+) ignored scenarios",
                [StepDefinitionType.Given],
                StepTextPattern.RegularExpression(
                    "there is a feature '(.*)' with (\\d+) passing (\\d+) failing (\\d+) pending and (\\d+) ignored scenarios"),
                [
                    new StepParameterDescriptor("featureTitle", typeof(string)),
                    new StepParameterDescriptor("passCount", typeof(int)),
                    new StepParameterDescriptor("failCount", typeof(int)),
                    new StepParameterDescriptor("pendingCount", typeof(int)),
                    new StepParameterDescriptor("ignoredCount", typeof(int))
                ]));
    }
}
