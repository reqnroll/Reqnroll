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
    public class TestCaseExecutionRecord : IGenerateMessage
    {
        public int AttemptId { get; }
        public bool WillBeRetried { get; set; }

        //  The ID of this particular execution of this Test Case
        public string TestCaseStartedId { get; }

        public DateTime TestCaseStartedTimeStamp;
        public DateTime TestCaseFinishedTimeStamp;
        public ScenarioExecutionStatus ScenarioExecutionStatus { get; private set; }
        public TestCaseTracker TestCaseTracker { get; }

        public List<StepExecutionTrackerBase> StepExecutionTrackers { get; }
        public StepExecutionTrackerBase CurrentStep { get { return StepExecutionTrackers.Last(); } }



        public TestCaseExecutionRecord(int attemptId, string testCaseStartedId, TestCaseTracker testCaseTracker)
        {
            AttemptId = attemptId;
            TestCaseStartedId = testCaseStartedId;
            WillBeRetried = false;
            StepExecutionTrackers = new();
            TestCaseTracker = testCaseTracker;
        }

        public IEnumerable<Envelope> RuntimeMessages
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

        public void StoreMessageGenerator(IGenerateMessage generator, ExecutionEvent executionEvent)
        {
            _events.Enqueue((generator, executionEvent));
        }

        // This queue holds ExecutionEvents that will be processed in stage 2
        private Queue<(IGenerateMessage, ExecutionEvent)> _events = new();


        public void RecordStart(ScenarioStartedEvent e)
        {
            TestCaseStartedTimeStamp = e.Timestamp;
            StoreMessageGenerator(this, e);
        }

        public void RecordFinish(ScenarioFinishedEvent e)
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
                        var testCase = CucumberMessageFactory.ToTestCase(TestCaseTracker.TestCaseDefinition);
                        yield return Envelope.Create(testCase);
                    }
                    var testCaseStarted = CucumberMessageFactory.ToTestCaseStarted(this, TestCaseTracker.TestCaseId);
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