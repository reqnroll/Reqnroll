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
        private const string DEFAULTSAMPLESDIRECTORYPLACEHOLDER = "[BaseDirectory]";

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
            if (String.IsNullOrEmpty(Path.GetDirectoryName(fileName)))
            {
                var path = Path.GetDirectoryName(ActualsResultLocationDirectory());
                fileName = Path.Combine(path!, fileName);
            }
            string messageFormatter = "{\"messages\" : { \"outputFilePath\" : \"" + fileName.Replace("\\", "\\\\") + "\" } }";

            Environment.SetEnvironmentVariable(CucumberConfigurationConstants.REQNROLL_CUCUMBER_MESSAGES_FORMATTERS_ENVIRONMENT_VARIABLE, messageFormatter);
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
        }

        protected void ResetCucumberMessagesOutputFileName()
        {
            Environment.SetEnvironmentVariable(CucumberConfigurationConstants.REQNROLL_CUCUMBER_MESSAGES_FORMATTERS_ENVIRONMENT_VARIABLE, null);
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
            var samplesDirectory = GetDefaultSamplesDirectory();
            configFileContent = configFileContent.Replace(DEFAULTSAMPLESDIRECTORYPLACEHOLDER, samplesDirectory.Replace(@"\", @"\\"));
            AddJsonConfigFileContent(configFileContent);
        }

        private static string GetDefaultSamplesDirectory() => Path.GetFullPath(Path.Combine(Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location)!, "..", "..", "..", "Samples"));

        protected static string ActualsResultLocationDirectory()
        {

            var objectContainerMock = new Mock<IObjectContainer>();
            var tracerMock = new Mock<ITraceListener>();
            objectContainerMock.Setup(x => x.Resolve<ITraceListener>()).Returns(tracerMock.Object);
            var env = new EnvironmentWrapper();
            var jsonConfigFileLocator = new ReqnrollJsonLocator();
            CucumberConfiguration configuration = new CucumberConfiguration(objectContainerMock.Object, env, jsonConfigFileLocator);
            string messagesConfiguration = configuration.FormatterConfiguration("messages");
            string outputFilePath = String.Empty;
            int colonIndex = messagesConfiguration.IndexOf(':');
            if (colonIndex != -1)
            {
                int firstQuoteIndex = messagesConfiguration.IndexOf('"', colonIndex);
                if (firstQuoteIndex != -1)
                {
                    int secondQuoteIndex = messagesConfiguration.IndexOf('"', firstQuoteIndex + 1);
                    if (secondQuoteIndex != -1)
                    {
                        outputFilePath = messagesConfiguration.Substring(firstQuoteIndex + 1, secondQuoteIndex - firstQuoteIndex - 1);
                    }
                }
            }
            if (String.IsNullOrEmpty(outputFilePath))
                outputFilePath = ".\\reqnroll_report.ndjson";

            string configurationPath = outputFilePath.Replace(DEFAULTSAMPLESDIRECTORYPLACEHOLDER, GetDefaultSamplesDirectory());
            return configurationPath;
        }

        protected void FileShouldExist(string v)
        {
            var directory = ActualsResultLocationDirectory();

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

        protected IEnumerable<Envelope> GetExpectedResults(string testName, string featureFileName)
        {
            var workingDirectory = Path.Combine(AppContext.BaseDirectory, "..", "..", "..");
            var expectedJsonText = File.ReadAllLines(Path.Combine(workingDirectory, "Samples", "Resources", testName, $"{featureFileName}.feature.ndjson"));

            foreach (var json in expectedJsonText)
            {
                var e = NdjsonSerializer.Deserialize(json);
                yield return e;
            };
        }

        record TestExecution(string id, List<Envelope> related);
        record TestCaseRecord(string id, string pickelId, Envelope testCaseEnvelope, Dictionary<string, TestExecution> executions);

        protected IEnumerable<Envelope> GetActualResults(string testName, string fileName)
        {

            string resultLocation = ActualsResultLocationDirectory();

            // Hack: the file name is hard-coded in the test row data to match the name of the feature within the Feature file for the example scenario

            var actualJsonText = File.ReadAllLines(resultLocation);
            actualJsonText.Should().HaveCountGreaterThan(0, "the test results ndjson file was emtpy.");

            var envelopes = actualJsonText.Select(json => NdjsonSerializer.Deserialize(json)).ToList();

            // The test cases (aka scenarios) might have been executed in any order
            // Comparison with the expected output ndjson assumes that tests are executed in the order the Pickles are listed.
            // So for the purposes of comparison, we're going to sort testCase messages (and related test execution messages) in pickle appearance order.

            var result = new List<Envelope>();

            // List of Pickle IDs in the order they are seen in the message stream
            var pickles = envelopes.Where(e => e.Content() is Pickle).Select(e => e.Pickle.Id).ToList();

            // Dictionary keyed by the ID of each test case.
            var testCases = new Dictionary<string, TestCaseRecord>();
            var allTestCaseEnvelopes = envelopes.Where(e => e.Content() is TestCase).ToList();
            var testCaseStartedToTestCaseMap = new Dictionary<string, string>();

            foreach (var tce in allTestCaseEnvelopes)
            {
                var tc = tce.Content() as TestCase;
                testCases.Add(tc!.Id, new TestCaseRecord(tc.Id, tc.PickleId, tce, new Dictionary<string, TestExecution>()));
            }
            int index = 0;
            bool testCasesBegun = false;
            // this loop sweeps all of the messages prior to the first testCase into the outgoing results collection.
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
                    var testCaseExecution = new TestExecution(tcsId, new List<Envelope>() { current });
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
                    TestRunHookStarted trhs => null,
                    TestRunHookFinished trhf => null,
                    _ => throw new ApplicationException("Unexpected Envelope type")
                };
                // attachments created by Before/After TestRun or Feature don't have a value for TestCaseStartedId, so don't attempt to add them to Test execution
                if (!String.IsNullOrEmpty(testCaseStartedId))
                {
                    var testCaseId = testCaseStartedToTestCaseMap[testCaseStartedId];
                    testCases[testCaseId].executions[testCaseStartedId].related.Add(current);
                }
                else
                    result.Add(current);
                index++;
            }


            // Now, sort the TestCaseRecords in order of their respective PickleId sequence
            var sortedTestCaseRecords = testCases.Values.OrderBy(tcr => pickles.IndexOf(tcr.pickelId)).ToList();
            var testCaseAndRelatedEnvelopes = sortedTestCaseRecords.SelectMany(tc => new List<Envelope>() { tc.testCaseEnvelope }.Concat(tc.executions.Values.SelectMany(e => e.related)));

            var testRunFinished = result.Last();
            result.Remove(testRunFinished);

            result.AddRange(testCaseAndRelatedEnvelopes);

            result.Add(testRunFinished);

            return result;
        }

    }
}
