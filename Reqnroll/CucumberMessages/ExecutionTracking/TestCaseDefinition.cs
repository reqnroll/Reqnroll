using System;
using System.Collections.Generic;

namespace Reqnroll.CucumberMessages.ExecutionTracking
{
    /// <summary>
    /// Data class that holds information about an executed TestCase that is discovered the first time the Scenario/TestCase is executed
    /// </summary>
    internal class TestCaseDefinition
    {
        internal string TestCaseId { get; }
        internal string PickleId { get; }
        internal TestCaseTracker Tracker { get; }
        internal List<TestStepDefinition> StepDefinitions { get; }

        internal TestCaseDefinition(string testCaseID, string pickleId, TestCaseTracker owner)
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