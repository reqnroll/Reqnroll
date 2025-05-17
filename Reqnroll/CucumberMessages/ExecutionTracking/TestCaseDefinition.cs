using Io.Cucumber.Messages.Types;
using System;
using System.Collections.Generic;
using System.Linq;

namespace Reqnroll.CucumberMessages.ExecutionTracking
{
    /// <summary>
    /// Data class that holds information about an executed TestCase that is discovered the first time the Scenario/TestCase is executed
    /// </summary>
    public class TestCaseDefinition
    {
        internal string TestCaseId { get; }
        internal string PickleId { get; }
        internal ITestCaseTracker Tracker { get; }
        internal List<TestStepDefinition> StepDefinitions { get; }

        internal TestCaseDefinition(string testCaseID, string pickleId, ITestCaseTracker owner)
        {
            TestCaseId = testCaseID;
            PickleId = pickleId;
            Tracker = owner;
            StepDefinitions = new();
        }

        internal string FindStepDefIDByStepPattern(string canonicalizedStepPattern)
        {
            return Tracker.StepDefinitionsByMethodSignature[canonicalizedStepPattern];
        }

        internal void AddStepDefinition(TestStepDefinition definition)
        {
            StepDefinitions.Add(definition);
        }
        internal TestStepDefinition FindTestStepDefByPickleId(string pickleId)
        {
            return StepDefinitions.OfType<TestStepDefinition>().First(sd => sd.PickleStepID == pickleId);
        }

        internal HookStepDefinition FindHookStepDefByHookId(string hookId)
        {
            return StepDefinitions.OfType<HookStepDefinition>().First(sd => sd.HookId == hookId);
        }
    }
}