using System;
using System.Collections.Generic;

namespace Reqnroll.TestProjectGenerator.Driver
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
                envVariables.Add("Reqnroll_Messages_TestRunStartedTimeOverride", $"{testRunStartupTime:O}");
            }

            if (_testSuiteInitializationDriver.OverrideTestCaseStartedPickleId is Guid startedPickleId)
            {
                envVariables.Add("Reqnroll_Messages_TestCaseStartedPickleIdOverride", $"{startedPickleId:D}");
            }

            if (_testSuiteInitializationDriver.OverrideTestCaseStartedTime is DateTime testCaseStartupTime)
            {
                envVariables.Add("Reqnroll_Messages_TestCaseStartedTimeOverride", $"{testCaseStartupTime:O}");
            }

            if (_testSuiteInitializationDriver.OverrideTestCaseFinishedPickleId is Guid finishedPickleId)
            {
                envVariables.Add("Reqnroll_Messages_TestCaseFinishedPickleIdOverride", $"{finishedPickleId:D}");
            }

            if (_testSuiteInitializationDriver.OverrideTestCaseFinishedTime is DateTime testCaseFinishedTime)
            {
                envVariables.Add("Reqnroll_Messages_TestCaseFinishedTimeOverride", $"{testCaseFinishedTime:O}");
            }

            if (_testSuiteInitializationDriver.OverrideCucumberEnable is bool formattersEnable)
            {
                envVariables.Add("REQNROLL_FORMATTERS_DISABLED", formattersEnable ? "false" : "true");
            }

            if (_testSuiteInitializationDriver.OverrideCucumberMessagesFormatters is string cucumberMessagesFormatters)
            {
                envVariables.Add("REQNROLL_FORMATTERS", cucumberMessagesFormatters);
            }

            return envVariables;
        }
    }
}
