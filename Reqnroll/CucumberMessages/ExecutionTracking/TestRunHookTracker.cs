﻿using Io.Cucumber.Messages.Types;
using Reqnroll.CucumberMessages.PayloadProcessing.Cucumber;
using Reqnroll.Events;
using System;
using System.Collections.Generic;

namespace Reqnroll.CucumberMessages.ExecutionTracking
{
    /// <summary>
    /// Captures information about TestRun Hooks (Before/After TestRun and Before/After Feature)
    /// </summary>
    internal class TestRunHookTracker : IGenerateMessage
    {
        internal TestRunHookTracker(string id, string hookDefinitionId, DateTime timestamp, string testRunID)
        {
            TestRunHookId = id;
            TestRunHook_HookId = hookDefinitionId;
            TestRunID = testRunID;
            TimeStamp = timestamp;
        }

        internal string TestRunHookId { get; }
        internal string TestRunHook_HookId { get; }
        internal string TestRunID { get; }
        internal DateTime TimeStamp { get; set; }
        internal TimeSpan Duration { get; set; }
        internal System.Exception Exception { get; set; }
        internal ScenarioExecutionStatus Status
        {
            get { return Exception == null ? ScenarioExecutionStatus.OK : ScenarioExecutionStatus.TestError; }
        }

        public IEnumerable<Io.Cucumber.Messages.Types.Envelope> GenerateFrom(ExecutionEvent executionEvent)
        {
            return executionEvent switch
            {
                HookBindingStartedEvent started => new List<Envelope>() { Envelope.Create(CucumberMessageFactory.ToTestRunHookStarted(this)) },
                HookBindingFinishedEvent finished => new List<Envelope>() { Envelope.Create(CucumberMessageFactory.ToTestRunHookFinished(this)) },
                _ => throw new ArgumentOutOfRangeException(nameof(executionEvent), executionEvent, null),
            };
        }
    }
}
