using Reqnroll.CucumberMessages.PayloadProcessing.Cucumber;
using Reqnroll.Events;
using System;
using System.Collections.Generic;
using System.Text;

namespace Reqnroll.CucumberMessages.ExecutionTracking
{
    internal class TestRunHookTracker
    {
        public TestRunHookTracker(string id, HookBindingStartedEvent hookBindingStartedEvent, string testRunID)
        {
            TestRunHookId = id;
            HookBindingSignature = CucumberMessageFactory.CanonicalizeHookBinding(hookBindingStartedEvent.HookBinding);
            TestRunID = testRunID;
        }

        public string TestRunHookId { get; }
        public string HookBindingSignature { get; }
        public string TestRunID { get; }
    }
}
