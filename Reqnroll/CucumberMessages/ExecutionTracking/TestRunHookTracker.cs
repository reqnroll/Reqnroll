using Reqnroll.CucumberMessages.PayloadProcessing.Cucumber;
using Reqnroll.Events;
using System;
using System.Collections.Generic;
using System.Text;

namespace Reqnroll.CucumberMessages.ExecutionTracking
{
    internal class TestRunHookTracker
    {
        public TestRunHookTracker(string id, string hookDefinitionId, HookBindingStartedEvent hookBindingStartedEvent, string testRunID)
        {
            TestRunHookId = id;
            TestRunHook_HookId = hookDefinitionId;
            TestRunID = testRunID;
            TimeStamp = hookBindingStartedEvent.Timestamp;
        }

        public string TestRunHookId { get; }
        public string TestRunHook_HookId { get; }
        public string TestRunID { get; }
        public DateTime TimeStamp { get; }
    }
}
