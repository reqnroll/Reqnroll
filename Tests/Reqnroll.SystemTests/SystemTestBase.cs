﻿using System;
using System.Text.RegularExpressions;
using FluentAssertions;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Reqnroll.BoDi;
using Reqnroll.SystemTests.Drivers;
using Reqnroll.TestProjectGenerator;
using Reqnroll.TestProjectGenerator.Data;
using Reqnroll.TestProjectGenerator.Driver;
using Reqnroll.TestProjectGenerator.Helpers;

namespace Reqnroll.SystemTests;
public abstract class SystemTestBase
{
    protected ProjectsDriver _projectsDriver = null!;
    protected ExecutionDriver _executionDriver = null!;
    protected VSTestExecutionDriver _vsTestExecutionDriver = null!;
    protected TestFileManager _testFileManager = new();
    protected FolderCleaner _folderCleaner = null!;
    protected ObjectContainer _testContainer = null!;
    protected TestRunConfiguration _testRunConfiguration = null!;
    protected CurrentVersionDriver _currentVersionDriver = null!;
    protected CompilationDriver _compilationDriver = null!;

    protected int _preparedTests = 0;

    public TestContext TestContext { get; set; } = null!;

    [TestInitialize]
    public void TestInitializeMethod()
    {
        TestInitialize();
    }

    protected virtual void TestInitialize()
    {
        _testContainer = new ObjectContainer();
        _testContainer.RegisterTypeAs<ConsoleOutputConnector, IOutputWriter>();

        _testRunConfiguration = _testContainer.Resolve<TestRunConfiguration>();
        _testRunConfiguration.ProgrammingLanguage = ProgrammingLanguage.CSharp;
        _testRunConfiguration.ProjectFormat = ProjectFormat.New;
        _testRunConfiguration.ConfigurationFormat = ConfigurationFormat.Json;
        _testRunConfiguration.TargetFramework = TargetFramework.Net80;
        _testRunConfiguration.UnitTestProvider = UnitTestProvider.MSTest;

        _currentVersionDriver = _testContainer.Resolve<CurrentVersionDriver>();
        _currentVersionDriver.NuGetVersion = NuGetPackageVersion.Version;
        _currentVersionDriver.ReqnrollNuGetVersion = NuGetPackageVersion.Version;

        _folderCleaner = _testContainer.Resolve<FolderCleaner>();
        _folderCleaner.EnsureOldRunFoldersCleaned();

        _projectsDriver = _testContainer.Resolve<ProjectsDriver>();
        _executionDriver = _testContainer.Resolve<ExecutionDriver>();
        _vsTestExecutionDriver = _testContainer.Resolve<VSTestExecutionDriver>();
        _compilationDriver = _testContainer.Resolve<CompilationDriver>();
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
