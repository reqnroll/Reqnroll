using System;
using System.Collections.Generic;

namespace Reqnroll.TestProjectGenerator.Driver
{
    public class TestSuiteInitializationDriver
    {
        public DateTime? OverrideTestSuiteStartupTime { get; set; }

        public Guid? OverrideTestCaseStartedPickleId { get; set; }

        public DateTime? OverrideTestCaseStartedTime { get; set; }

        public Guid? OverrideTestCaseFinishedPickleId { get; set; }

        public DateTime? OverrideTestCaseFinishedTime { get; set; }

        public bool? OverrideCucumberEnable { get; set; }

        public string OverrideCucumberMessagesFormatters { get; set; }

        internal Dictionary<string, string> EnvironmentVariableOverrides { get; } = [];
        public void AddEnvironmentVariableOverride(string envKey, string envVar)
        {
            EnvironmentVariableOverrides.Add(envKey, envVar);
        }
    }
}
