using Io.Cucumber.Messages.Types;
using Reqnroll.Events;
using System.Collections.Generic;

namespace Reqnroll.CucumberMessages.ExecutionTracking
{
    internal class OutputAddedEventWrapper : IGenerateMessage
    {
        internal OutputAddedEvent OutputAddedEvent;
        internal string TestRunStartedId;
        internal string TestCaseStepId;
        internal string TestCaseStartedId;

        public OutputAddedEventWrapper(OutputAddedEvent outputAddedEvent, string testRunStartedId, string testCaseStartedId, string testCaseStepId)
        {
            OutputAddedEvent = outputAddedEvent;
            TestRunStartedId = testRunStartedId;
            TestCaseStartedId = testCaseStartedId;
            TestCaseStepId = testCaseStepId;
        }

        public IEnumerable<Envelope> GenerateFrom(ExecutionEvent executionEvent)
        {
            throw new System.NotImplementedException();
        }
    }
}