using System;
using System.Collections.Generic;

namespace TechTalk.SpecFlow.TestProjectGenerator.Driver
{
    public class TestSuiteEnvironmentVariableGenerator
    {
        private readonly TestSuiteInitializationDriver _testSuiteInitializationDriver;

        public TestSuiteEnvironmentVariableGenerator(TestSuiteInitializationDriver testSuiteInitializationDriver)
        {
            _testSuiteInitializationDriver = testSuiteInitializationDriver;
        }

        public Dictionary<string, string> GenerateEnvironmentVariables()
        {
            var envVariables = new Dictionary<string, string>
            {
                {"DOTNET_CLI_UI_LANGUAGE", "en"}
            };

            if (_testSuiteInitializationDriver.OverrideTestSuiteStartupTime is DateTime testRunStartupTime)
            {
                envVariables.Add("SpecFlow_Messages_TestRunStartedTimeOverride", $"{testRunStartupTime:O}");
            }

            if (_testSuiteInitializationDriver.OverrideTestCaseStartedPickleId is Guid startedPickleId)
            {
                envVariables.Add("SpecFlow_Messages_TestCaseStartedPickleIdOverride", $"{startedPickleId:D}");
            }

            if (_testSuiteInitializationDriver.OverrideTestCaseStartedTime is DateTime testCaseStartupTime)
            {
                envVariables.Add("SpecFlow_Messages_TestCaseStartedTimeOverride", $"{testCaseStartupTime:O}");
            }

            if (_testSuiteInitializationDriver.OverrideTestCaseFinishedPickleId is Guid finishedPickleId)
            {
                envVariables.Add("SpecFlow_Messages_TestCaseFinishedPickleIdOverride", $"{finishedPickleId:D}");
            }

            if (_testSuiteInitializationDriver.OverrideTestCaseFinishedTime is DateTime testCaseFinishedTime)
            {
                envVariables.Add("SpecFlow_Messages_TestCaseFinishedTimeOverride", $"{testCaseFinishedTime:O}");
            }

            return envVariables;
        }
    }
}
