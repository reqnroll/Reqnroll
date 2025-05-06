using Reqnroll.Events;
using System;
using System.Linq;
using System.Collections.Generic;
using Io.Cucumber.Messages.Types;
using Reqnroll.CucumberMessages.PayloadProcessing.Cucumber;

namespace Reqnroll.CucumberMessages.ExecutionTracking
{
    /// <summary>
    /// Data class that holds information about a single execution of a TestCase.
    /// There will be mulitple of these for a given TestCase if it is Retried.
    /// </summary>
    internal class TestCaseExecutionRecord : IGenerateMessage
    {
        internal int AttemptId { get; }
        internal bool WillBeRetried { get; set; }

        //  The ID of this particular execution of this Test Case
        internal string TestCaseStartedId { get; }

        internal DateTime TestCaseStartedTimeStamp { get; private set; }
        internal DateTime TestCaseFinishedTimeStamp { get; private set; }
        internal ScenarioExecutionStatus ScenarioExecutionStatus { get; private set; }

        internal List<StepExecutionTrackerBase> StepExecutionTrackers { get; }
        internal StepExecutionTrackerBase CurrentStep { get { return StepExecutionTrackers.Last(); } }
        internal string _testCaseId;
        internal TestCaseDefinition _testCaseDefinition;

        internal TestCaseExecutionRecord(int attemptId, string testCaseStartedId, string testCaseId, TestCaseDefinition testCaseDefinition)
        {
            AttemptId = attemptId;
            TestCaseStartedId = testCaseStartedId;
            WillBeRetried = false;
            StepExecutionTrackers = new();
            _testCaseId = testCaseId;
            _testCaseDefinition = testCaseDefinition;
        }

        internal IEnumerable<Envelope> RuntimeMessages
        {
            get
            {
                while (_events.Count > 0)
                {
                    var (generator, execEvent) = _events.Dequeue();
                    foreach (var e in generator.GenerateFrom(execEvent))
                    {
                        yield return e;
                    }
                }
            }
        }

        internal void StoreMessageGenerator(IGenerateMessage generator, ExecutionEvent executionEvent)
        {
            _events.Enqueue((generator, executionEvent));
        }

        // This queue holds ExecutionEvents that will be processed in stage 2
        private Queue<(IGenerateMessage, ExecutionEvent)> _events = new();


        internal void RecordStart(ScenarioStartedEvent e)
        {
            TestCaseStartedTimeStamp = e.Timestamp;
            StoreMessageGenerator(this, e);
        }

        internal void RecordFinish(ScenarioFinishedEvent e)
        {
            TestCaseFinishedTimeStamp = e.Timestamp;
            ScenarioExecutionStatus = e.ScenarioContext.ScenarioExecutionStatus;
            StoreMessageGenerator(this, e);
        }

        public IEnumerable<Envelope> GenerateFrom(ExecutionEvent executionEvent)
        {
            switch (executionEvent)
            {
                case ScenarioStartedEvent scenarioStartedEvent:
                    // On the first execution of this TestCase we emit a TestCase Message.
                    // On subsequent retries we do not.
                    if (AttemptId == 0)
                    {
                        var testCase = CucumberMessageFactory.ToTestCase(_testCaseDefinition);
                        yield return Envelope.Create(testCase);
                    }
                    var testCaseStarted = CucumberMessageFactory.ToTestCaseStarted(this, _testCaseId);
                    yield return Envelope.Create(testCaseStarted);
                    break;
                case ScenarioFinishedEvent scenarioFinishedEvent:
                    yield return Envelope.Create(CucumberMessageFactory.ToTestCaseFinished(this));
                    break;
                default:
                    break;
            }
        }
    }
}