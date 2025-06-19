using System;
using Reqnroll.Formatters.PayloadProcessing.Cucumber;

namespace Reqnroll.Formatters.ExecutionTracking;

/// <summary>
/// Base class for tracking execution of steps (test steps and hooks)
/// </summary>
public abstract class StepExecutionTrackerBase(TestCaseExecutionTracker parentTracker, ICucumberMessageFactory messageFactory)
{
    public TestCaseExecutionTracker ParentTracker { get; } = parentTracker;
    protected ICucumberMessageFactory MessageFactory { get; } = messageFactory;

    public StepTracker StepTracker { get; protected internal set; }

    public ScenarioExecutionStatus Status { get; protected set; }
    public DateTime StepStarted { get; protected set; }
    public DateTime StepFinished { get; protected set; }
    public Exception Exception { get; protected set; }
    public TimeSpan Duration => StepFinished - StepStarted;

    public string TestCaseStartedId => ParentTracker.TestCaseStartedId;
    protected IPickleExecutionTracker PickleExecutionTracker => ParentTracker.ParentTracker;
    protected TestCaseTracker TestCaseTracker => PickleExecutionTracker.TestCaseTracker;
}