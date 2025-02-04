using Reqnroll.CucumberMessages.PayloadProcessing.Cucumber;
using Reqnroll.Events;
using System;
using System.Collections.Generic;
using System.Text;

namespace Reqnroll.CucumberMessages.ExecutionTracking
{
    /// <summary>
    /// Captures information about TestRun Hooks (Before/After Feature)
    /// </summary>
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
        public DateTime TimeStamp { get; set; }
        public TimeSpan Duration { get; internal set; }
        public Exception Exception { get; internal set; }
    }
}
