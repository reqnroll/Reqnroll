using System;
using System.Text.RegularExpressions;
using FluentAssertions;
using Reqnroll.BoDi;
using Reqnroll.SystemTests.Drivers;
using Reqnroll.TestProjectGenerator;
using Reqnroll.TestProjectGenerator.Data;
using Reqnroll.TestProjectGenerator.Driver;
using Reqnroll.TestProjectGenerator.Helpers;
using Xunit.Abstractions;

namespace Reqnroll.SystemTests;
public abstract class SystemTestBase
{
    protected readonly ITestOutputHelper _testOutputHelper;
    protected readonly ProjectsDriver _projectsDriver;
    protected readonly ExecutionDriver _executionDriver;
    protected readonly VSTestExecutionDriver _vsTestExecutionDriver;
    protected readonly TestFileManager _testFileManager = new();
    protected readonly FolderCleaner _folderCleaner;
    protected readonly ObjectContainer _testContainer;
    protected readonly TestRunConfiguration _testRunConfiguration;
    protected readonly CurrentVersionDriver _currentVersionDriver;

    protected int _preparedTests = 0;

    protected SystemTestBase(ITestOutputHelper testOutputHelper)
    {
        _testOutputHelper = testOutputHelper;

        _testContainer = new ObjectContainer();
        _testContainer.RegisterInstanceAs(_testOutputHelper);
        _testContainer.RegisterTypeAs<XUnitOutputConnector, IOutputWriter>();

        _testRunConfiguration = _testContainer.Resolve<TestRunConfiguration>();
        _testRunConfiguration.ProgrammingLanguage = ProgrammingLanguage.CSharp;
        _testRunConfiguration.ProjectFormat = ProjectFormat.New;
        _testRunConfiguration.ConfigurationFormat = ConfigurationFormat.Json;
        _testRunConfiguration.TargetFramework = TargetFramework.Net60;
        _testRunConfiguration.UnitTestProvider = UnitTestProvider.MSTest;

        _currentVersionDriver = _testContainer.Resolve<CurrentVersionDriver>();
        _currentVersionDriver.NuGetVersion = NuGetPackageVersion.Version;
        _currentVersionDriver.ReqnrollNuGetVersion = NuGetPackageVersion.Version;

        _folderCleaner = _testContainer.Resolve<FolderCleaner>();
        _folderCleaner.EnsureOldRunFoldersCleaned();

        _projectsDriver = _testContainer.Resolve<ProjectsDriver>();
        _executionDriver = _testContainer.Resolve<ExecutionDriver>();
        _vsTestExecutionDriver = _testContainer.Resolve<VSTestExecutionDriver>();
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
            _testOutputHelper.WriteLine("No tests detected");
            return null;
        }

        _testOutputHelper.WriteLine($"Detected {count} tests");
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
        if (expectedNrOfTestsSpec == null && _preparedTests == 0) 
            throw new ArgumentException($"If {nameof(_preparedTests)} is not set, the {nameof(expectedNrOfTestsSpec)} is mandatory.", nameof(expectedNrOfTestsSpec));
        var expectedNrOfTests = expectedNrOfTestsSpec ?? _preparedTests;

        _vsTestExecutionDriver.LastTestExecutionResult.Should().NotBeNull();
        _vsTestExecutionDriver.LastTestExecutionResult.Total.Should().Be(expectedNrOfTests, $"the run should contain {expectedNrOfTests} tests");
        _vsTestExecutionDriver.LastTestExecutionResult.Succeeded.Should().Be(expectedNrOfTests, "all tests should pass");
        _folderCleaner.CleanSolutionFolder();
    }
}
