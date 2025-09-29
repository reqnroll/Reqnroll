using FluentAssertions;
using Io.Cucumber.Messages.Types;
using Moq;
using Reqnroll.Analytics.UserId;
using Reqnroll.BoDi;
using Reqnroll.Configuration;
using Reqnroll.Formatters.PayloadProcessing;
using Reqnroll.Formatters.PayloadProcessing.Cucumber;
using Reqnroll.EnvironmentAccess;
using Reqnroll.Formatters.Configuration;
using Reqnroll.SystemTests;
using Reqnroll.Tracing;
using Reqnroll.Utils;
using System.Reflection;

namespace Reqnroll.Formatters.Tests;

public class MessagesCompatibilityTestBase : SystemTestBase
{
    private const string DEFAULTSAMPLESDIRECTORYPLACEHOLDER = "[BaseDirectory]";

    protected override void TestCleanup()
    {
        // TEMPORARY: this is in place so that SystemTestBase.TestCleanup does not run (which deletes the generated code)
    }

    protected void EnableCucumberMessages()
    {
        _testSuiteInitializationDriver.OverrideCucumberEnable = true;
    }

    protected void SetCucumberMessagesOutputFileName(string fileName)
    {
        var baseFileName = Path.GetFileNameWithoutExtension(fileName);
        var ndjsonFileName = baseFileName + ".ndjson";
        var htmlFileName = baseFileName + ".html";
        var path = Path.GetDirectoryName(fileName);

        if (string.IsNullOrEmpty(path))
        {
            path = ActualResultLocationDirectory();
            ndjsonFileName = Path.Combine(path, ndjsonFileName);
            htmlFileName = Path.Combine(path, htmlFileName);
        }
        string formatters = "{\"formatters\" : {\"message\" : { \"outputFilePath\" : \"" + ndjsonFileName.Replace("\\", "\\\\") + "\" }," +
                            " \"html\" : { \"outputFilePath\" : \"" + htmlFileName.Replace("\\", "\\\\") + "\" } } }";

        _testSuiteInitializationDriver.OverrideCucumberMessagesFormatters = formatters;
    }

    protected void DisableCucumberMessages()
    {
        _testSuiteInitializationDriver.OverrideCucumberEnable = false;
    }

    protected void ResetCucumberMessages(string? fileToDelete = null)
    {
        fileToDelete = string.IsNullOrEmpty(fileToDelete) ? fileToDelete : fileToDelete + ".ndjson";
        DeletePreviousMessagesOutput(fileToDelete);
        ResetCucumberMessagesOutputFileName();
        Environment.SetEnvironmentVariable(EnvironmentOptions.REQNROLL_FORMATTERS_DISABLED_ENVIRONMENT_VARIABLE, null);
    }
    protected void ResetCucumberMessagesHtml(string? fileToDelete = null)
    {
        fileToDelete = string.IsNullOrEmpty(fileToDelete) ? fileToDelete : fileToDelete + ".html";
        DeletePreviousMessagesOutput(fileToDelete);
        ResetCucumberMessagesOutputFileName();
        Environment.SetEnvironmentVariable(EnvironmentOptions.REQNROLL_FORMATTERS_DISABLED_ENVIRONMENT_VARIABLE, null);
    }

    protected void ResetCucumberMessagesOutputFileName()
    {
        Environment.SetEnvironmentVariable(EnvironmentOptions.REQNROLL_FORMATTERS_ENVIRONMENT_VARIABLE, null);
    }

    protected void MimicGitHubActionsEnvironment()
    {
        Environment.SetEnvironmentVariable("GITHUB_ACTIONS", "true");
        Environment.SetEnvironmentVariable("GITHUB_SERVER_URL", "https://github.com");
        Environment.SetEnvironmentVariable("GITHUB_REPOSITORY", "reqnroll/reqnroll");
        Environment.SetEnvironmentVariable("GITHUB_RUN_ID", "1234567890");
        Environment.SetEnvironmentVariable("GITHUB_RUN_NUMBER", "1");
        Environment.SetEnvironmentVariable("GITHUB_REF_TYPE", "branch");
        Environment.SetEnvironmentVariable("GITHUB_REF_NAME", "main");
        Environment.SetEnvironmentVariable("GITHUB_SHA", "abcdef1234567890abcdef1234567890abcdef12");
    }

    protected void MimicAzurePipelinesEnvironment()
    {
        Environment.SetEnvironmentVariable("TF_BUILD", "true");
        Environment.SetEnvironmentVariable("BUILD_BUILDURI", "https://dev.azure.com/reqnroll/reqnroll/_build");
        Environment.SetEnvironmentVariable("BUILD_BUILDNUMBER", "20231001.1");
        Environment.SetEnvironmentVariable("BUILD_REPOSITORY_URI", "https://dev.azure.com/reqnroll/reqnroll/_git/reqnroll");
        Environment.SetEnvironmentVariable("BUILD_SOURCEBRANCHNAME", "1b1c2588e46d5c995d54da1082b618fa13553eb3");
        Environment.SetEnvironmentVariable("BUILD_SOURCEVERSION", "main");
        Environment.SetEnvironmentVariable("Build_SOURCEBRANCH", "refs/tags/v1.0.0");
    }
    protected void DeletePreviousMessagesOutput(string? fileToDelete = null)
    {
        var directory = ActualResultLocationDirectory();

        if (fileToDelete != null)
        {
            var fileToDeletePath = Path.Combine(directory, fileToDelete);

            if (File.Exists(fileToDeletePath))
            {
                File.Delete(fileToDeletePath);
            }
        }
    }

    protected void AddFeatureFilesFromResources(string featureFileName, string folder, Assembly assembly)
    {
        var assemblyName = assembly.GetName().Name;
        string prefixToRemove = $"{assemblyName}.{folder}.Resources.{featureFileName}.";
        // list of features in the scenario directory
        var featuresToCompile = assembly.GetManifestResourceNames()
                           .Where(rn => rn.StartsWith(prefixToRemove) && rn.EndsWith(".feature"))
                           .Select(rn => rn.Substring(prefixToRemove.Length));
        foreach (var feature in featuresToCompile)
        {
            AddFeatureFileFromResource($"{featureFileName}/{feature}", folder, Assembly.GetExecutingAssembly());
        }

    }

    protected void AddBindingClassFromResource(string fileName, string? prefix = null, Assembly? assemblyToLoadFrom = null)
    {
        var bindingCLassFileContent = _testFileManager.GetTestFileContent(fileName, prefix, assemblyToLoadFrom);
        AddBindingClass(bindingCLassFileContent);
    }

    protected void ShouldAllScenariosPend(int? expectedNrOfTestsSpec = null)
    {
        int expectedNrOfTests = ConfirmAllTestsRan(expectedNrOfTestsSpec);
        _vsTestExecutionDriver.LastTestExecutionResult.Pending.Should().Be(expectedNrOfTests, "all tests should pend");
    }

    protected void AddBinaryFilesFromResource(string scenarioName, string prefix, Assembly assembly)
    {
        foreach (var fileName in GetTestBinaryFileNames(scenarioName, prefix, assembly))
        {
            var content = _testFileManager.GetTestFileContent(fileName, $"{prefix}.{scenarioName}", assembly);
            _projectsDriver.AddFile(fileName, content);
        }
    }

    protected IEnumerable<string> GetTestBinaryFileNames(string scenarioName, string prefix, Assembly? assembly)
    {
        var testAssembly = assembly ?? Assembly.GetExecutingAssembly();
        string prefixToRemove = $"{prefix}.{scenarioName}.";
        return testAssembly.GetManifestResourceNames()
                           .Where(rn => !rn.EndsWith(".feature") && !rn.EndsWith(".cs") && !rn.EndsWith(".feature.ndjson") && rn.StartsWith(prefixToRemove))
                           .Select(rn => rn.Substring(prefixToRemove.Length));
    }

    protected void CucumberMessagesAddConfigurationFile(string configFileName)
    {
        var configFileContent = File.ReadAllText(configFileName);
        var samplesDirectory = GetDefaultSamplesDirectory();
        configFileContent = configFileContent.Replace(DEFAULTSAMPLESDIRECTORYPLACEHOLDER, samplesDirectory.Replace(@"\", @"\\"));
        AddJsonConfigFileContent(configFileContent);
    }

    private static string GetDefaultSamplesDirectory() => Path.GetFullPath(Path.Combine(Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location)!, "..", "..", "..", "Samples"));

    protected static string ActualResultLocationDirectory()
    {
        var objectContainerMock = new Mock<IObjectContainer>();
        var tracerMock = new Mock<ITraceListener>();
        objectContainerMock.Setup(x => x.Resolve<ITraceListener>()).Returns(tracerMock.Object);
        var env = new EnvironmentWrapper();
        var envOptions = new EnvironmentOptions(env);
        var jsonConfigFileLocator = new ReqnrollJsonLocator();
        var fileSystem = new FileSystem();
        var fileService = new FileService();
        var configFileResolver = new FileBasedConfigurationResolver(jsonConfigFileLocator, fileSystem, fileService);
        var jsonEnvConfigResolver = new JsonEnvironmentConfigurationResolver(envOptions);

        var keyValueEnvironmentConfigurationResolverMock = new Mock<IKeyValueEnvironmentConfigurationResolver>();
        keyValueEnvironmentConfigurationResolverMock.Setup(r => r.Resolve()).Returns(new Dictionary<string, IDictionary<string, object>>());

        FormattersConfigurationProvider configurationProvider = new FormattersConfigurationProvider(
            configFileResolver,
                                                                        jsonEnvConfigResolver,
                                                                        keyValueEnvironmentConfigurationResolverMock.Object,
                                                                        new FormattersDisabledOverrideProvider(envOptions));
        configurationProvider.GetFormatterConfigurationByName("message").TryGetValue("outputFilePath", out var outputFilePathElement);

        var outputFilePath = outputFilePathElement!.ToString();
        if (string.IsNullOrEmpty(outputFilePath))
            outputFilePath = "[BASEDIRECTORY]\\CucumberMessages\\reqnroll_report.ndson";

        string actualResultLocationDirectory = outputFilePath.Replace(DEFAULTSAMPLESDIRECTORYPLACEHOLDER, GetDefaultSamplesDirectory());
        actualResultLocationDirectory = Path.GetDirectoryName(actualResultLocationDirectory)!;
        return actualResultLocationDirectory;
    }

    protected void FileShouldExist(string v)
    {
        var directory = ActualResultLocationDirectory();

        var file = Path.Combine(directory, v);

        File.Exists(file).Should().BeTrue(file, $"File {v} should exist");
    }

    protected void AddUtilClassWithFileSystemPath()
    {
        string location = Path.Combine(AppContext.BaseDirectory, "Samples", "Resources");
        AddBindingClass($$"""
                          public class FileSystemPath 
                          { 
                              public static string GetFilePathForAttachments() => @"{{location}}";
                          } 
                          """);
    }

    protected IEnumerable<Envelope> GetExpectedResults(string testName)
    {
        string[] expectedJsonText = GetExpectedJsonText(testName);

        foreach (var json in expectedJsonText)
        {
            var e = NdjsonSerializer.Deserialize(json);
            yield return e;
        }
    }

    protected string[] GetExpectedJsonText(string testName)
    {
        var fileName = testName + "." + testName + ".ndjson";
        var assemblyToLoadFrom = Assembly.GetExecutingAssembly();
        var expectedJsonText = _testFileManager.GetTestFileContent(fileName, "Samples", assemblyToLoadFrom).Split(new[] { '\n', '\r' }, StringSplitOptions.RemoveEmptyEntries);
        return expectedJsonText;
    }

    protected static string[] GetActualGeneratedHtml(string featureFileName)
    {
        string resultLocation = ActualResultLocationDirectory();
        var expectedJsonText = File.ReadAllLines(Path.Combine(resultLocation, $"{featureFileName}.html"));
        return expectedJsonText;
    }

    // ReSharper disable NotAccessedPositionalProperty.Local
    record TestExecution(string Id, List<Envelope> Related);
    record TestCaseRecord(string Id, string PickleId, Envelope TestCaseEnvelope, Dictionary<string, TestExecution> Executions);
    // ReSharper restore NotAccessedPositionalProperty.Local

    protected IEnumerable<Envelope> GetActualResults(string testName)
    {
        string[] actualJsonText = GetActualJsonText(testName);
        actualJsonText.Should().HaveCountGreaterThan(0, "the test results ndjson file was empty.");

        var envelopes = actualJsonText.Select(NdjsonSerializer.Deserialize).ToList();

        // The test cases (aka scenarios) might have been executed in any order
        // Comparison with the expected output ndjson assumes that tests are executed in the order the Pickles are listed.
        // So for the purposes of comparison, we're going to sort testCase messages (and related test execution messages) in pickle appearance order.

        var result = new List<Envelope>();

        // List of Pickle IDs in the order they are seen in the message stream
        var pickleMessages = envelopes.Where(e => e.Content() is Pickle).Select(e => e.Pickle).ToArray();
        var pickles = pickleMessages.Select(p => p.Id).ToList();

        // Dictionary keyed by the ID of each test case.
        var testCases = new Dictionary<string, TestCaseRecord>();
        var allTestCaseEnvelopes = envelopes.Where(e => e.Content() is TestCase).ToList();
        var testCaseStartedToTestCaseMap = new Dictionary<string, string>();

        string FindTestCaseStartedFromStepPickleId(string pickleStepId)
        {
            var pickleId = pickleMessages.First(p => p.Steps.Any(ps => ps.Id == pickleStepId)).Id;
            var testCaseStartedId = testCases.Values.First(tcr => tcr.PickleId == pickleId).Executions.Last().Value.Id;
            return testCaseStartedId;
        }

        foreach (var tce in allTestCaseEnvelopes)
        {
            var tc = tce.Content() as TestCase;
            testCases.Add(tc!.Id, new TestCaseRecord(tc.Id, tc.PickleId, tce, new Dictionary<string, TestExecution>()));
        }
        List<Type> staticMessageTypes = new List<Type>
        {
            typeof(Meta),
            typeof(Source),
            typeof(GherkinDocument),
            typeof(Pickle),
            typeof(ParameterType),
            typeof(StepDefinition),
            typeof(Hook)
        };
        bool IsStaticMessage(Envelope envelope)
        {
            return staticMessageTypes.Contains(envelope.Content().GetType());
        }
        int index = 0;
        bool testCasesFinished = false;
        while (index < envelopes.Count && !testCasesFinished)
        {
            var current = envelopes[index];
            if (IsStaticMessage(current))
            {
                result.Add(current);
                index++;
                continue;
            }
            if (current.Content() is TestRunStarted)
            {
                result.Add(current);
                index++;
                continue;
            }
            if (current.Content() is TestRunFinished)
            {
                testCasesFinished = true;
                result.Add(current);
                index++;
                continue;
            }
            if (current.Content() is TestCase)
            {
                // as TestCases were already identified and inserted into the testCases dictionary, no direct work required here; skip to the next Message
                index++;
                continue;
            }
            // handle test case started and related
            if (current.Content() is TestCaseStarted testCaseStarted)
            {
                var tcsId = testCaseStarted.Id;
                var testCaseExecution = new TestExecution(tcsId, new List<Envelope>() { current });
                testCases[testCaseStarted.TestCaseId].Executions.Add(tcsId, testCaseExecution);
                testCaseStartedToTestCaseMap.Add(tcsId, testCaseStarted.TestCaseId);
                index++;
                continue;
            }
            var testCaseStartedId = current.Content() switch
            {
                TestStepStarted started => started.TestCaseStartedId,
                TestStepFinished finished => finished.TestCaseStartedId,
                TestCaseFinished tcFin => tcFin.TestCaseStartedId,
                Attachment att => att.TestCaseStartedId,
                TestRunHookStarted => null,
                TestRunHookFinished => null,
                Suggestion suggestion => FindTestCaseStartedFromStepPickleId(suggestion.PickleStepId),

                _ => throw new ApplicationException($"Unexpected Envelope type: {current.Content()}")
            };
            // attachments created by Before/After TestRun or Feature don't have a value for TestCaseStartedId, so don't attempt to add them to Test execution
            if (!string.IsNullOrEmpty(testCaseStartedId))
            {
                var testCaseId = testCaseStartedToTestCaseMap[testCaseStartedId];
                testCases[testCaseId].Executions[testCaseStartedId].Related.Add(current);
            }
            else
                result.Add(current);
            index++;
        }


        // Now, sort the TestCaseRecords in order of their respective PickleId sequence
        var sortedTestCaseRecords = testCases.Values.OrderBy(tcr => pickles.IndexOf(tcr.PickleId)).ToList();
        var testCaseAndRelatedEnvelopes = sortedTestCaseRecords.SelectMany(tc => new List<Envelope>() { tc.TestCaseEnvelope }.Concat(tc.Executions.Values.SelectMany(e => e.Related)));

        var testRunFinished = result.Last();
        result.Remove(testRunFinished);

        result.AddRange(testCaseAndRelatedEnvelopes);

        result.Add(testRunFinished);

        return result;
    }

    protected static string[] GetActualJsonText(string testName)
    {
        string resultLocation = ActualResultLocationDirectory();
        var fileName = Path.Combine(resultLocation, testName + ".ndjson");

        var actualJsonText = File.ReadAllLines(fileName);
        return actualJsonText;
    }

}