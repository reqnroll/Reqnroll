using System;
using Reqnroll.BoDi;
using Reqnroll;

// ReSharper disable once CheckNamespace
namespace TechTalk.SpecFlow;

public class ScenarioContext(Reqnroll.ScenarioContext originalContext) : IScenarioContext
{
    #region Singleton
    [Obsolete("Please get the ScenarioContext via Context Injection - https://go.reqnroll.net/doc-migrate-sc-current")]
    public static Reqnroll.ScenarioContext Current => Reqnroll.ScenarioContext.Current;
    #endregion

    public Exception TestError => originalContext.TestError;
    public ScenarioInfo ScenarioInfo => originalContext.ScenarioInfo;
    public RuleInfo RuleInfo => originalContext.RuleInfo;
    public ScenarioBlock CurrentScenarioBlock => originalContext.CurrentScenarioBlock;
    public IObjectContainer ScenarioContainer => originalContext.ScenarioContainer;
    public ScenarioExecutionStatus ScenarioExecutionStatus => originalContext.ScenarioExecutionStatus;
}
