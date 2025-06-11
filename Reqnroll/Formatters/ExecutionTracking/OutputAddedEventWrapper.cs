using Io.Cucumber.Messages.Types;
using Reqnroll.Formatters.PayloadProcessing.Cucumber;
using Reqnroll.Events;
using System.Collections.Generic;

namespace Reqnroll.Formatters.ExecutionTracking
{
    public class OutputAddedEventWrapper : IGenerateMessage
    {
        internal OutputAddedEvent OutputAddedEvent { get; }
        internal string TestRunStartedId { get; }
        internal string TestCaseStepId { get; }

        internal ICucumberMessageFactory _messageFactory;

        internal string TestCaseStartedId { get; }

        internal OutputAddedEventWrapper(OutputAddedEvent outputAddedEvent, string testRunStartedId, string testCaseStartedId, string testCaseStepId, ICucumberMessageFactory messageFactory)
        {
            OutputAddedEvent = outputAddedEvent;
            TestRunStartedId = testRunStartedId;
            TestCaseStartedId = testCaseStartedId;
            TestCaseStepId = testCaseStepId;
            _messageFactory = messageFactory;
        }

        public IEnumerable<Envelope> GenerateFrom(ExecutionEvent executionEvent)
        {
            return [Envelope.Create(_messageFactory.ToAttachment(this))];
        }
    }
}