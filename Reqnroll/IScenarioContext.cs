using Reqnroll.BoDi;

namespace Reqnroll;

public interface IScenarioContext : IReqnrollContext
{
    ScenarioInfo ScenarioInfo { get; }

    RuleInfo RuleInfo { get; }

    ScenarioBlock CurrentScenarioBlock { get; }

    IObjectContainer ScenarioContainer { get; }

    ScenarioExecutionStatus ScenarioExecutionStatus { get; }
}
