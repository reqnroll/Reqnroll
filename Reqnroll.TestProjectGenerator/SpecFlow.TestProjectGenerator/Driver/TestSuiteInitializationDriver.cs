using System;

namespace TechTalk.SpecFlow.TestProjectGenerator.Driver
{
    public class TestSuiteInitializationDriver
    {
        public DateTime? OverrideTestSuiteStartupTime { get; set; }

        public Guid? OverrideTestCaseStartedPickleId { get; set; }

        public DateTime? OverrideTestCaseStartedTime { get; set; }

        public Guid? OverrideTestCaseFinishedPickleId { get; set; }

        public DateTime? OverrideTestCaseFinishedTime { get; set; }

    }
}
