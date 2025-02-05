using Io.Cucumber.Messages.Types;
using Reqnroll.CucumberMessages.PayloadProcessing.Cucumber;
using Reqnroll.Events;
using System.Collections.Generic;

namespace Reqnroll.CucumberMessages.ExecutionTracking
{
    internal class OutputAddedEventWrapper : IGenerateMessage
    {
        internal OutputAddedEvent OutputAddedEvent { get; }
        internal string TestRunStartedId { get; }
        internal string TestCaseStepId { get; }
        internal string TestCaseStartedId { get; }

        internal OutputAddedEventWrapper(OutputAddedEvent outputAddedEvent, string testRunStartedId, string testCaseStartedId, string testCaseStepId)
        {
            OutputAddedEvent = outputAddedEvent;
            TestRunStartedId = testRunStartedId;
            TestCaseStartedId = testCaseStartedId;
            TestCaseStepId = testCaseStepId;
        }

        public IEnumerable<Envelope> GenerateFrom(ExecutionEvent executionEvent)
        {
            return [Envelope.Create(CucumberMessageFactory.ToAttachment(this))];
        }
    }
}