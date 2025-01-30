using FluentAssertions;
using Io.Cucumber.Messages.Types;
using Moq;
using Reqnroll.BoDi;
using Reqnroll.Configuration;
using Reqnroll.CucumberMessages.Configuration;
using Reqnroll.CucumberMessages.PayloadProcessing;
using Reqnroll.CucumberMessages.PayloadProcessing.Cucumber;
using Reqnroll.EnvironmentAccess;
using Reqnroll.SystemTests;
using Reqnroll.Tracing;
using System;
using System.Collections.Generic;
using System.Data.Common;
using System.Linq;
using System.Net.WebSockets;
using System.Reflection;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;

namespace CucumberMessages.Tests
{
    public class CucumberCompatibilityTestBase : SystemTestBase
    {
        protected override void TestCleanup()
        {
            // TEMPORARY: this is in place so that SystemTestBase.TestCleanup does not run (which deletes the generated code)
        }

        protected void EnableCucumberMessages()
        {
            Environment.SetEnvironmentVariable(CucumberConfigurationConstants.REQNROLL_CUCUMBER_MESSAGES_ENABLE_ENVIRONMENT_VARIABLE, "true");
        }

        protected void SetCucumberMessagesOutputFileName(string fileName)
        {
            Environment.SetEnvironmentVariable(CucumberConfigurationConstants.REQNROLL_CUCUMBER_MESSAGES_OUTPUT_FILENAME_ENVIRONMENT_VARIABLE, fileName);
        }

        protected void DisableCucumberMessages()
        {
            Environment.SetEnvironmentVariable(CucumberConfigurationConstants.REQNROLL_CUCUMBER_MESSAGES_ENABLE_ENVIRONMENT_VARIABLE, "false");
        }

        protected void ResetCucumberMessages(string? fileToDelete = null)
        {
            DeletePreviousMessagesOutput(fileToDelete);
            ResetCucumberMessagesOutputFileName();
            Environment.SetEnvironmentVariable(CucumberConfigurationConstants.REQNROLL_CUCUMBER_MESSAGES_ENABLE_ENVIRONMENT_VARIABLE, null);
            Environment.SetEnvironmentVariable(CucumberConfigurationConstants.REQNROLL_CUCUMBER_MESSAGES_ID_GENERATION_STYLE_ENVIRONMENT_VARIABLE, null);
        }

        protected void ResetCucumberMessagesOutputFileName()
        {
            Environment.SetEnvironmentVariable(CucumberConfigurationConstants.REQNROLL_CUCUMBER_MESSAGES_OUTPUT_FILENAME_ENVIRONMENT_VARIABLE, null);
        }

        protected void DeletePreviousMessagesOutput(string? fileToDelete = null)
        {
            var directory = ActualsResultLocationDirectory();

            if (fileToDelete != null)
            {
                var fileToDeletePath = Path.Combine(directory, fileToDelete);

                if (File.Exists(fileToDeletePath))
                {
                    File.Delete(fileToDeletePath);
                }
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
            AddJsonConfigFileContent(configFileContent);
        }

        protected static string ActualsResultLocationDirectory()
        {
            //var configFileLocation = Path.Combine(Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location)!, "CucumberMessages.configuration.json");

            //var config = System.Text.Json.JsonSerializer.Deserialize<ConfigurationDTO>(File.ReadAllText(configFileLocation), new JsonSerializerOptions { PropertyNamingPolicy = JsonNamingPolicy.CamelCase });

            var objectContainerMock = new Mock<IObjectContainer>();
            var tracerMock = new Mock<ITraceListener>();
            objectContainerMock.Setup(x => x.Resolve<ITraceListener>()).Returns(tracerMock.Object);
            var env = new EnvironmentWrapper();
            var jsonConfigFileLocator = new ReqnrollJsonLocator();
            CucumberConfiguration configuration = new CucumberConfiguration(objectContainerMock.Object, env, jsonConfigFileLocator);
            var resultLocation = Path.Combine(configuration.BaseDirectory, configuration.OutputDirectory);
            return resultLocation;
        }

        protected void SetEnvironmentVariableForGUIDIdGeneration()
        {
            Environment.SetEnvironmentVariable(CucumberConfigurationConstants.REQNROLL_CUCUMBER_MESSAGES_ID_GENERATION_STYLE_ENVIRONMENT_VARIABLE, CucumberConfigurationConstants.REQNROLL_CUCUMBER_MESSAGES_ID_GENERATION_STYLE_UUID);
        }

        protected void FileShouldExist(string v)
        {
            var directory = ActualsResultLocationDirectory();

            var file = Path.Combine(directory, v);

            File.Exists(file).Should().BeTrue(file, $"File {v} should exist");
        }

        protected void AddUtilClassWithFileSystemPath()
        {
            string location = AppContext.BaseDirectory;
            AddBindingClass(
                $"public class FileSystemPath {{  public static string GetFilePathForAttachments()  {{  return @\"{location}\\Samples\"; }}  }} ");
        }

        protected IEnumerable<Envelope> GetExpectedResults(string testName, string featureFileName)
        {
            var workingDirectory = Path.Combine(AppContext.BaseDirectory, "..\\..\\..");
            var expectedJsonText = File.ReadAllLines(Path.Combine(workingDirectory!, "Samples", $"{testName}\\{featureFileName}.feature.ndjson"));

            foreach (var json in expectedJsonText)
            {
                var e = NdjsonSerializer.Deserialize(json);
                yield return e;
            };
        }

        record TestExecution(string id, TestCaseStarted started, List<Envelope> related);
        record TestCaseRecord(string id, TestCase tc, Dictionary<string, TestExecution> executions);

        protected IEnumerable<Envelope> GetActualResults(string testName, string fileName)
        {

            string resultLocation = ActualsResultLocationDirectory();

            // Hack: the file name is hard-coded in the test row data to match the name of the feature within the Feature file for the example scenario

            var actualJsonText = File.ReadAllLines(Path.Combine(resultLocation, $"{fileName}.ndjson"));
            var envelopes = actualJsonText.Select(json => NdjsonSerializer.Deserialize(json)).ToList();
            var result = new List<Envelope>();

            // Dictionary keyed by the ID of each test case.
            var testCases = new Dictionary<string, TestCaseRecord>();

            var allTestCases = envelopes.Where(e => e.Content() is TestCase).Select(e => e.TestCase).ToList().OrderBy(tc => tc.PickleId).ToList();

            var testCaseStartedToTestCaseMap = new Dictionary<string, string>();

            foreach (var tc in allTestCases)
            {
                testCases.Add(tc.Id, new TestCaseRecord(tc.Id, tc, new Dictionary<string, TestExecution>()));
            }
            int index = 0;
            bool testCasesBegun = false;
            while (index < envelopes.Count && !testCasesBegun)
            {
                var current = envelopes[index];
                if (current.Content() is TestCase)
                {
                    testCasesBegun = true;
                }
                else
                {
                    result.Add(current);
                    index++;
                }
            }
            bool testCasesFinished = false;
            while (index < envelopes.Count && !testCasesFinished)
            {
                var current = envelopes[index];
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
                    var testCaseExecution = new TestExecution(tcsId, testCaseStarted, new List<Envelope>());
                    testCases[testCaseStarted.TestCaseId].executions.Add(tcsId, testCaseExecution);
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
                    _ => throw new ApplicationException("Unexpected Envelope type")
                };
                var testCaseId = testCaseStartedToTestCaseMap[testCaseStartedId];
                testCases[testCaseId].executions[testCaseStartedId].related.Add(current);
                index++;
            }

            foreach (var tc in testCases.Values)
            {
                result.Insert(result.Count - 1, Envelope.Create(tc.tc));
                foreach (var e in tc.executions.Values)
                {
                    result.Insert(result.Count - 1, Envelope.Create(e.started));
                    foreach (var r in e.related)
                        result.Insert(result.Count - 1, r);
                }
            }
            return result;
        }

    }
}
