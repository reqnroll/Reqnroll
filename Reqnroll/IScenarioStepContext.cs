using System;

namespace Reqnroll;

public interface IScenarioStepContext : IReqnrollContext
{
    StepInfo StepInfo { get; }

    ScenarioExecutionStatus Status { get; set; }
    Exception StepError { get; set; }
}
