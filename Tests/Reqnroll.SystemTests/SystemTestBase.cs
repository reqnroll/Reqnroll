using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text.RegularExpressions;
using FluentAssertions;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Reqnroll.SystemTests.Drivers;
using Reqnroll.TestProjectGenerator;
using Reqnroll.TestProjectGenerator.Data;
using Reqnroll.TestProjectGenerator.Driver;
using Reqnroll.TestProjectGenerator.Helpers;
using Scrutor;

namespace Reqnroll.SystemTests;
public abstract class SystemTestBase
{
    protected ProjectsDriver _projectsDriver = null!;
    protected ExecutionDriver _executionDriver = null!;
    protected VSTestExecutionDriver _vsTestExecutionDriver = null!;
    protected TestFileManager _testFileManager = new();
    protected FolderCleaner _folderCleaner = null!;
    protected IServiceProvider _testContainer = null!;
    protected TestRunConfiguration _testRunConfiguration = null!;
    protected CurrentVersionDriver _currentVersionDriver = null!;
    protected CompilationDriver _compilationDriver = null!;

    protected int _preparedTests = 0;
    private TestProjectFolders _testProjectFolders = null!;

    public TestContext TestContext { get; set; } = null!;

    [TestInitialize]
    public void TestInitializeMethod()
    {
        TestInitialize();
    }

    protected virtual IServiceCollection ConfigureServices()
    {
        var services = new ServiceCollection();

        services.AddSingleton<IOutputWriter, ConsoleOutputConnector>();

        services.Scan(scan => scan
                              .FromAssemblyOf<TestRunConfiguration>()
                              .AddClasses()
                              .UsingRegistrationStrategy(RegistrationStrategy.Skip)
                              .AsSelf()
                              .WithScopedLifetime());

        services.Scan(scan => scan
                              .FromAssemblyOf<ExecutionDriver>()
                              .AddClasses(c => c.InNamespaceOf<ExecutionDriver>())
                              .UsingRegistrationStrategy(RegistrationStrategy.Skip)
                              .AsSelf()
                              .WithScopedLifetime());

        return services;
    }

    protected virtual void TestInitialize()
    {
        var services = ConfigureServices();
        _testContainer = services.BuildServiceProvider();

        _testRunConfiguration = _testContainer.GetService<TestRunConfiguration>();
        _testRunConfiguration.ProgrammingLanguage = ProgrammingLanguage.CSharp;
        _testRunConfiguration.ProjectFormat = ProjectFormat.New;
        _testRunConfiguration.ConfigurationFormat = ConfigurationFormat.Json;
        _testRunConfiguration.TargetFramework = TargetFramework.Net80;
        _testRunConfiguration.UnitTestProvider = UnitTestProvider.MSTest;

        _currentVersionDriver = _testContainer.GetService<CurrentVersionDriver>();
        _currentVersionDriver.NuGetVersion = NuGetPackageVersion.Version;
        _currentVersionDriver.ReqnrollNuGetVersion = NuGetPackageVersion.Version;

        _folderCleaner = _testContainer.GetService<FolderCleaner>();
        _folderCleaner.EnsureOldRunFoldersCleaned();

        _projectsDriver = _testContainer.GetService<ProjectsDriver>();
        _executionDriver = _testContainer.GetService<ExecutionDriver>();
        _vsTestExecutionDriver = _testContainer.GetService<VSTestExecutionDriver>();
        _compilationDriver = _testContainer.GetService<CompilationDriver>();
        _testProjectFolders = _testContainer.GetService<TestProjectFolders>();
    }


    [TestCleanup]
    public void TestCleanupMethod()
    {
        TestCleanup();
    }

    protected virtual void TestCleanup()
    {
        _folderCleaner.CleanSolutionFolder();
    }

    protected void AddFeatureFileFromResource(string fileName, int? preparedTests = null)
    {
        var featureFileContent = _testFileManager.GetTestFileContent(fileName);
        AddFeatureFile(featureFileContent, preparedTests);
    }

    private int? GetTestCount(string gherkinContent)
    {
        var matches = Regex.Matches(gherkinContent, @"^\s*((?<scenario>Scenario:|Scenario Outline:)|(?<exmaples>Examples:)|(?<row>\|)).*?\s*$", RegexOptions.Multiline);
        int count = 0;
        var inExamples = false;
        foreach (Match match in matches)
        {
            if (match.Groups["scenario"].Success)
            {
                count++;
                inExamples = false;
            }
            else if (match.Groups["exmaples"].Success)
            {
                if (!inExamples)
                    count--; // was a scenario outline
                count--; // examples header row
                inExamples = true;
            }
            else if (inExamples && match.Groups["row"].Success)
            {
                count++;
            }
        }

        if (count == 0)
        {
            Console.WriteLine("No tests detected");
            return null;
        }

        Console.WriteLine($"Detected {count} tests");
        return count;
    }

    protected void AddFeatureFile(string featureFileContent, int? preparedTests = null)
    {
        _projectsDriver.AddFeatureFile(featureFileContent);
        UpdatePreparedTests(featureFileContent, preparedTests);
    }

    protected void AddScenario(string scenarioContent, int? preparedTests = null)
    {
        _projectsDriver.AddScenario(scenarioContent);
        UpdatePreparedTests(scenarioContent, preparedTests);
    }

    protected void AddSimpleScenario()
    {
        AddScenario(
            """
            Scenario: Sample Scenario
                When something happens
            """);
    }

    protected void AddSimpleScenarioOutline(int numberOfExamples = 2)
    {
        var examples = numberOfExamples == 2
            ? """
                  | me     |
                  | you    |
              """
            : string.Join(
                Environment.NewLine,
                Enumerable.Range(1, numberOfExamples).Select(i => $"    | example {i} |"));
        AddScenario(
            $"""
            Scenario Outline: Scenario outline with examples
                When something happens to <person>
            Examples:
               	| person |
            {examples}
            """);
    }

    protected void AddSimpleScenarioAndOutline()
    {
        AddSimpleScenario();
        AddSimpleScenarioOutline();
    }

    private void UpdatePreparedTests(string gherkinContent, int? preparedTests)
    {
        if (preparedTests == null)
        {
            var match = Regex.Match(gherkinContent, @"^#TEST_COUNT:\s*(?<count>\d+)\s*$", RegexOptions.Multiline);
            preparedTests = match.Success ? 
                int.Parse(match.Groups["count"].Value) : 
                GetTestCount(gherkinContent);
        }

        if (preparedTests != null) _preparedTests += preparedTests.Value;
    }

    protected void PrepareGeneratorAllInSamples()
    {
        AddFeatureFileFromResource("GeneratorAllInSample1.feature");
        AddFeatureFileFromResource("GeneratorAllInSample2.feature");
        _projectsDriver.AddPassingStepBinding();
    }

    protected void ExecuteTests()
    {
        _executionDriver.ExecuteTests();
    }

    protected void ShouldAllScenariosPass(int? expectedNrOfTestsSpec = null)
    {
        int expectedNrOfTests = ConfirmAllTestsRan(expectedNrOfTestsSpec);
        _vsTestExecutionDriver.LastTestExecutionResult.Succeeded.Should().Be(expectedNrOfTests, "all tests should pass");
    }

    protected void ShouldAllScenariosFail(int? expectedNrOfTestsSpec = null)
    {
        int expectedNrOfTests = ConfirmAllTestsRan(expectedNrOfTestsSpec);
        _vsTestExecutionDriver.LastTestExecutionResult.Failed.Should().Be(expectedNrOfTests, "all tests should fail");
    }

    protected void ShouldAllScenariosPend(int? expectedNrOfTestsSpec = null)
    {
        int expectedNrOfTests = ConfirmAllTestsRan(expectedNrOfTestsSpec);
        _vsTestExecutionDriver.LastTestExecutionResult.Pending.Should().Be(expectedNrOfTests, "all tests should Pend");
    }

    protected void ShouldAllScenariosBeIgnored(int? expectedNrOfTestsSpec = null)
    {
        int expectedNrOfTests = ConfirmAllTestsRan(expectedNrOfTestsSpec);
        _vsTestExecutionDriver.LastTestExecutionResult.Ignored.Should().Be(expectedNrOfTests, "all tests should be Ignored");
    }

    protected void ShouldAllUndefinedScenariosNotBeExecuted(int? expectedNrOfTestsSpec = null)
    {
        int expectedNrOfTests = ConfirmAllTestsRan(expectedNrOfTestsSpec);
        _vsTestExecutionDriver.LastTestExecutionResult.Succeeded.Should().Be(0, "none of the tests should pass");
        _vsTestExecutionDriver.LastTestExecutionResult.Failed.Should().Be(0, "none of the tests should fail");

        // MSTest treats tests with undefined steps as Ignored, the other two frameworks treat them as Pending
        int pendingExpected = _testRunConfiguration.UnitTestProvider == UnitTestProvider.MSTest ? 0 : expectedNrOfTests;
        _vsTestExecutionDriver.LastTestExecutionResult.Pending.Should().Be(pendingExpected, "All of the tests should Pend");

        int ignoredExpected = _testRunConfiguration.UnitTestProvider == UnitTestProvider.MSTest ? expectedNrOfTests : 0;
        _vsTestExecutionDriver.LastTestExecutionResult.Ignored.Should().Be(ignoredExpected, "None of the tests should be Ignored");
    }

    protected int ConfirmAllTestsRan(int? expectedNrOfTestsSpec)
    {
        if (expectedNrOfTestsSpec == null && _preparedTests == 0)
            throw new ArgumentException($"If {nameof(_preparedTests)} is not set, the {nameof(expectedNrOfTestsSpec)} is mandatory.", nameof(expectedNrOfTestsSpec));
        var expectedNrOfTests = expectedNrOfTestsSpec ?? _preparedTests;

        _vsTestExecutionDriver.LastTestExecutionResult.Should().NotBeNull();
        _vsTestExecutionDriver.LastTestExecutionResult.Total.Should().Be(expectedNrOfTests, $"the run should contain {expectedNrOfTests} tests");
        return expectedNrOfTests;
    }

    protected void AddHookBinding(string eventType, string? name = null, string? code = null)
    {
        _projectsDriver.AddHookBinding(eventType, name, code: code ?? "global::Log.LogHook();");
    }

    protected void AddPassingStepBinding(string scenarioBlock = "StepDefinition", string stepRegex = ".*")
    {
        _projectsDriver.AddPassingStepBinding(scenarioBlock, stepRegex);
    }

    protected void AddFailingStepBinding(string scenarioBlock = "StepDefinition", string stepRegex = ".*")
    {
        _projectsDriver.AddFailingStepBinding(scenarioBlock, stepRegex);
    }

    protected void AddPendingStepBinding(string scenarioBlock = "StepDefinition", string stepRegex = ".*")
    {
        _projectsDriver.AddStepBinding(scenarioBlock, stepRegex, "throw new PendingStepException();", "ScenarioContext.Current.Pending()");
    }

    protected void AddBindingClass(string content)
    {
        _projectsDriver.AddBindingClass(content);
    }

    //TODO: Consider moving this to the TestProjectGenerator as a driver method
    public void CheckAreStepsExecutedInOrder(IEnumerable<string> methodNames)
    {
        _testProjectFolders.PathToSolutionDirectory.Should().NotBeNullOrWhiteSpace();

        var pathToLogFile = Path.Combine(_testProjectFolders.PathToSolutionDirectory, "steps.log");
        var lines = File.ReadAllLines(pathToLogFile);
        var methodNameLines = methodNames.Select(m => $"-> step: {m}");
        lines.Should().ContainInOrder(methodNameLines);
    }
}
