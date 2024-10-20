using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Moq;
using Reqnroll.FeatureSourceGenerator.SourceModel;

namespace Reqnroll.FeatureSourceGenerator.CSharp;
public class CSharpTestFixtureSourceGeneratorTests
{
    private readonly Mock<ITestFrameworkHandler> _mockHandler = new();

    private readonly Mock<ITestFixtureGenerator<CSharpCompilationInformation>> _mockGenerator = new();

    public CSharpTestFixtureSourceGeneratorTests()
    {
        _mockHandler
            .Setup(handler => handler.IsTestFrameworkReferenced(It.IsAny<CompilationInformation>()))
            .Returns(true);

        _mockHandler.SetupGet(handler => handler.TestFrameworkName).Returns("Mock");

        _mockHandler
            .Setup(handler => handler.GetTestFixtureGenerator(It.IsAny<CSharpCompilationInformation>()))
            .Returns(_mockGenerator.Object);


        _mockGenerator.SetupGet(generator => generator.TestFrameworkHandler).Returns(_mockHandler.Object);

        //_mockGenerator
        //    .Setup(generator => generator.GenerateTestMethod(
        //        It.IsAny<TestMethodGenerationContext<CSharpCompilationInformation>>(),
        //        It.IsAny<CancellationToken>()))
        //    .Returns(new CSharpTestMethod())

        _mockGenerator
            .Setup(generator => generator.GenerateTestFixtureClass(
                It.IsAny<TestFixtureGenerationContext<CSharpCompilationInformation>>(),
                It.IsAny<IEnumerable<TestMethod>>(),
                It.IsAny<CancellationToken>()))
            .Returns(new CSharpTestFixtureClass(
                new NamespaceString("Test") + new SimpleTypeIdentifier(new IdentifierString("Mock")),
                "Mock.feature",
                new FeatureInformation("Mock", null, "en")));
    }

    [Theory]
    [InlineData("reqnroll.emit_ignored_examples")]
    [InlineData("build_property.ReqnrollEmitIgnoredExamples")]
    public void GeneratorEmitsIgnoredScenariosWhenOptionEnabled(string setting)
    {
        var references = AppDomain.CurrentDomain.GetAssemblies()
            .Where(asm => !asm.IsDynamic)
            .Select(asm => MetadataReference.CreateFromFile(asm.Location));

        var compilation = CSharpCompilation.Create("test", references: references);

        var generator = new CSharpTestFixtureSourceGenerator([_mockHandler.Object]);

        const string featureText =
            """
            Feature: Sample Feature

            Scenario Outline: SO
            When the step <result>
            Examples:
                | result       |
                | passes       |
                | fails        |
                | is pending   |
                | is undefined |
            @ignore
            Examples:
                | result       |
                | ignored      |
            """;

        var optionsProvider = new FakeAnalyzerConfigOptionsProvider(
            new InMemoryAnalyzerConfigOptions(new Dictionary<string, string>
            {
                { setting, "true" }
            }));

        var driver = CSharpGeneratorDriver
            .Create(generator)
            .AddAdditionalTexts([new FeatureFile("Calculator.feature", featureText)])
            .WithUpdatedAnalyzerConfigOptions(optionsProvider)
            .RunGeneratorsAndUpdateCompilation(compilation, out var generatedCompilation, out var diagnostics);

        diagnostics.Should().BeEmpty();

        var generate = _mockGenerator.Invocations
            .Single(inv => inv.Method.Name == nameof(ITestFixtureGenerator<CSharpCompilationInformation>.GenerateTestMethod));

        var context = (TestMethodGenerationContext<CSharpCompilationInformation>)generate.Arguments[0];

        context.ScenarioInformation.Examples.Should().HaveCount(2);
    }

    [Theory]
    [InlineData("reqnroll.emit_ignored_examples")]
    [InlineData("build_property.ReqnrollEmitIgnoredExamples")]
    public void GeneratorOmitsIgnoredScenariosWhenOptionDisabled(string setting)
    {
        var references = AppDomain.CurrentDomain.GetAssemblies()
            .Where(asm => !asm.IsDynamic)
            .Select(asm => MetadataReference.CreateFromFile(asm.Location));

        var compilation = CSharpCompilation.Create("test", references: references);

        var generator = new CSharpTestFixtureSourceGenerator([_mockHandler.Object]);

        const string featureText =
            """
            Feature: Sample Feature

            Scenario Outline: SO
            When the step <result>
            Examples:
                | result       |
                | passes       |
                | fails        |
                | is pending   |
                | is undefined |
            @ignore
            Examples:
                | result       |
                | ignored      |
            """;

        var optionsProvider = new FakeAnalyzerConfigOptionsProvider(
            new InMemoryAnalyzerConfigOptions(new Dictionary<string, string>
            {
                { setting, "false" }
            }));

        var driver = CSharpGeneratorDriver
            .Create(generator)
            .AddAdditionalTexts([new FeatureFile("Calculator.feature", featureText)])
            .WithUpdatedAnalyzerConfigOptions(optionsProvider)
            .RunGeneratorsAndUpdateCompilation(compilation, out var generatedCompilation, out var diagnostics);

        diagnostics.Should().BeEmpty();

        var generate = _mockGenerator.Invocations
            .Single(inv => inv.Method.Name == nameof(ITestFixtureGenerator<CSharpCompilationInformation>.GenerateTestMethod));

        var context = (TestMethodGenerationContext<CSharpCompilationInformation>)generate.Arguments[0];

        context.ScenarioInformation.Examples.Should().HaveCount(1);
    }

    [Fact]
    public void GeneratorOmitsIgnoredScenariosByDefault()
    {
        var references = AppDomain.CurrentDomain.GetAssemblies()
            .Where(asm => !asm.IsDynamic)
            .Select(asm => MetadataReference.CreateFromFile(asm.Location));

        var compilation = CSharpCompilation.Create("test", references: references);

        var generator = new CSharpTestFixtureSourceGenerator([_mockHandler.Object]);

        const string featureText =
            """
            Feature: Sample Feature

            Scenario Outline: SO
            When the step <result>
            Examples:
                | result       |
                | passes       |
                | fails        |
                | is pending   |
                | is undefined |
            @ignore
            Examples:
                | result       |
                | ignored      |
            """;

        var optionsProvider = new FakeAnalyzerConfigOptionsProvider(
            new InMemoryAnalyzerConfigOptions([]));

        var driver = CSharpGeneratorDriver
            .Create(generator)
            .AddAdditionalTexts([new FeatureFile("Calculator.feature", featureText)])
            .WithUpdatedAnalyzerConfigOptions(optionsProvider)
            .RunGeneratorsAndUpdateCompilation(compilation, out var generatedCompilation, out var diagnostics);

        diagnostics.Should().BeEmpty();

        var generate = _mockGenerator.Invocations
            .Single(inv => inv.Method.Name == nameof(ITestFixtureGenerator<CSharpCompilationInformation>.GenerateTestMethod));

        var context = (TestMethodGenerationContext<CSharpCompilationInformation>)generate.Arguments[0];

        context.ScenarioInformation.Examples.Should().HaveCount(1);
    }
}
