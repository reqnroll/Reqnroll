using Reqnroll.Events;
using System;

namespace Reqnroll.CucumberMessages.ExecutionTracking
{
    /// <summary>
    /// Captures information about TestRun Hooks (Before/After Feature)
    /// </summary>
    internal class TestRunHookTracker
    {
        internal TestRunHookTracker(string id, string hookDefinitionId, HookBindingStartedEvent hookBindingStartedEvent, string testRunID)
        {
            TestRunHookId = id;
            TestRunHook_HookId = hookDefinitionId;
            TestRunID = testRunID;
            TimeStamp = hookBindingStartedEvent.Timestamp;
        }

        internal string TestRunHookId { get; }
        internal string TestRunHook_HookId { get; }
        internal string TestRunID { get; }
        internal DateTime TimeStamp { get; set; }
        internal TimeSpan Duration { get; set; }
        internal Exception Exception { get; set; }
        internal ScenarioExecutionStatus Status
        {
            get { return Exception == null ? ScenarioExecutionStatus.OK : ScenarioExecutionStatus.TestError; }
        }
    }
}
