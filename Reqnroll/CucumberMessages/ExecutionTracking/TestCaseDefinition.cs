using System;
using System.Collections.Generic;

namespace Reqnroll.CucumberMessages.ExecutionTracking
{
    /// <summary>
    /// Data class that holds information about an executed TestCase that is discovered the first time the Scenario/TestCase is executed
    /// </summary>
    public class TestCaseDefinition
    {
        public string TestCaseId { get; }
        public string PickleId { get; private set; }
        public TestCaseTracker Tracker { get; }
        public List<TestStepDefinition> StepDefinitions { get; }

        public TestCaseDefinition(string testCaseID, string pickleId, TestCaseTracker owner)
        {
            TestCaseId = testCaseID;
            PickleId = pickleId;
            Tracker = owner;
            StepDefinitions = new();
        }

        internal string FindStepDefIDByStepPattern(string canonicalizedStepPattern)
        {
            return Tracker.StepDefinitionsByPattern[canonicalizedStepPattern];
        }
    }
}