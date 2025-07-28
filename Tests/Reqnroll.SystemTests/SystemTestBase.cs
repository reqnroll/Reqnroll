using System;
using System.IO;
using System.Linq;
using System.Reflection;
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
using TestContext = Microsoft.VisualStudio.TestTools.UnitTesting.TestContext;

namespace Reqnroll.SystemTests;
public abstract class SystemTestBase
{
    protected ProjectsDriver _projectsDriver = null!;
    protected SolutionDriver _solutionDriver = null!;
    protected ExecutionDriver _executionDriver = null!;
    protected VSTestExecutionDriver _vsTestExecutionDriver = null!;
    protected ConfigurationFileDriver _configurationFileDriver = null!;
    protected TestSuiteInitializationDriver _testSuiteInitializationDriver = null!;
    protected TestFileManager _testFileManager = new();
    protected FolderCleaner _folderCleaner = null!;
    protected IServiceProvider _testContainer = null!;
    protected TestRunConfiguration _testRunConfiguration = null!;
    protected CurrentVersionDriver _currentVersionDriver = null!;
    protected CompilationDriver _compilationDriver = null!;
    protected BindingsDriver _bindingDriver = null!;
    protected TestProjectFolders _testProjectFolders = null!;
    protected JsonConfigurationLoaderDriver _jsonConfigurationLoaderDriver = null!;

    protected int _preparedTests = 0;

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

    protected TService GetServiceSafe<TService>() => 
        _testContainer.GetService<TService>() ?? throw new InvalidOperationException($"Unable to get service '{typeof(TService).FullName}'");

    protected virtual void TestInitialize()
    {
        var services = ConfigureServices();
        _testContainer = services.BuildServiceProvider();

        _testRunConfiguration = GetServiceSafe<TestRunConfiguration>();
        _testRunConfiguration.ProgrammingLanguage = ProgrammingLanguage.CSharp;
        _testRunConfiguration.ProjectFormat = ProjectFormat.New;
        _testRunConfiguration.ConfigurationFormat = ConfigurationFormat.Json;
        _testRunConfiguration.TargetFramework = TargetFramework.Net80;
        _testRunConfiguration.UnitTestProvider = UnitTestProvider.MSTest;

        _currentVersionDriver = GetServiceSafe<CurrentVersionDriver>();
        _currentVersionDriver.ReqnrollNuGetVersion = NuGetPackageVersion.Version;

        _folderCleaner = GetServiceSafe<FolderCleaner>();
        _folderCleaner.EnsureOldRunFoldersCleaned();

        _projectsDriver = GetServiceSafe<ProjectsDriver>();
        _solutionDriver = GetServiceSafe<SolutionDriver>();
        _executionDriver = GetServiceSafe<ExecutionDriver>();
        _vsTestExecutionDriver = GetServiceSafe<VSTestExecutionDriver>();
        _compilationDriver = GetServiceSafe<CompilationDriver>();
        _testProjectFolders = GetServiceSafe<TestProjectFolders>();
        _bindingDriver = GetServiceSafe<BindingsDriver>();
        _jsonConfigurationLoaderDriver = GetServiceSafe<JsonConfigurationLoaderDriver>();
        _configurationFileDriver = GetServiceSafe<ConfigurationFileDriver>();
        _testSuiteInitializationDriver = GetServiceSafe<TestSuiteInitializationDriver>();
    }


    [TestCleanup]
    public void TestCleanupMethod()
    {
        TestCleanup();
    }

    protected virtual void TestCleanup()
    {
        if (TestContext.CurrentTestOutcome == UnitTestOutcome.Passed)
            _folderCleaner.CleanSolutionFolder();
    }

    protected void AddFeatureFileFromResource(string fileName, string? resourceGroup = null, Assembly? assembly = null, int? preparedTests = null)
    {
        var featureFileContent = _testFileManager.GetTestFileContent(fileName, resourceGroup, assembly);
        AddFeatureFile(featureFileContent, preparedTests);
    }

    protected void AddBindingClassFromResource(string fileName, string? resourceGroup = null)
    {
        var bindingClassContent = _testFileManager.GetTestFileContent(fileName, resourceGroup);
        AddBindingClass(bindingClassContent);
    }

    protected void AddContentFileFromResource(string resourceFileName, string? targetFolder = null, string? resourceGroup = null)
    {
        var fileContent = _testFileManager.GetTestFileContent(resourceFileName, resourceGroup);
        var filePath = Path.GetFileName(resourceFileName);
        if (targetFolder != null)
        {
            filePath = Path.Combine(targetFolder, filePath);
        }
        _projectsDriver.AddFile(filePath, fileContent);
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

    protected void ShouldFinishWithoutTestExecutionWarnings()
    {
        _vsTestExecutionDriver.LastTestExecutionResult.Warnings.Should().BeEmpty();
    }

    protected int ConfirmAllTestsRan(int? expectedNrOfTestsSpec = null)
    {
        if (expectedNrOfTestsSpec == null && _preparedTests == 0)
            throw new ArgumentException($"If {nameof(_preparedTests)} is not set, the {nameof(expectedNrOfTestsSpec)} is mandatory.", nameof(expectedNrOfTestsSpec));
        var expectedNrOfTests = expectedNrOfTestsSpec ?? _preparedTests;

        _vsTestExecutionDriver.LastTestExecutionResult.Should().NotBeNull();
        _vsTestExecutionDriver.LastTestExecutionResult.Total.Should().Be(expectedNrOfTests, $"the run should contain {expectedNrOfTests} tests");
        return expectedNrOfTests;
    }

    protected void AddHookBinding(string eventType, string? name = null, string code = "", bool? asyncHook = null, int? order = null)
    {
        _projectsDriver.AddHookBinding(eventType, name, code: code, asyncHook: asyncHook, order: order);
    }

    protected void AddPassingStepBinding(string scenarioBlock = "StepDefinition", string stepRegex = ".*")
    {
        _projectsDriver.AddPassingStepBinding(scenarioBlock, stepRegex);
    }

    protected void AddBindingClass(string content)
    {
        _projectsDriver.AddBindingClass(content);
    }

    protected void AddJsonConfigFileContent(string reqnrollConfigContent)
    {
        _jsonConfigurationLoaderDriver.AddReqnrollJson(reqnrollConfigContent);
    }
}
