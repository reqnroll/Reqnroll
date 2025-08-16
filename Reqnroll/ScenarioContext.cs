using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Threading;
using Reqnroll.BoDi;
using Reqnroll.Bindings;
using Reqnroll.Infrastructure;

namespace Reqnroll;

public class ScenarioContext : ReqnrollContext, IScenarioContext
{
    #region Singleton
    private static bool _isCurrentDisabled = false;
    private static ScenarioContext _current;

    [Obsolete("Please get the ScenarioContext via Context Injection - https://go.reqnroll.net/doc-migrate-sc-current")]
    public static ScenarioContext Current
    {
        get
        {
            if (_isCurrentDisabled)
                throw new ReqnrollException("The ScenarioContext.Current static accessor cannot be used in multi-threaded execution. Try injecting the scenario context to the binding class. See https://go.reqnroll.net/doc-parallel-execution for details.");
            if (_current == null)
            {
                Debug.WriteLine("Accessing NULL ScenarioContext");
            }
            return _current;
        }
        internal set
        {
            if (!_isCurrentDisabled)
                _current = value;
        }
    }

    internal static void DisableSingletonInstance()
    {
        _isCurrentDisabled = true;
        Thread.MemoryBarrier();
        _current = null;
    }
    #endregion

    public ScenarioInfo ScenarioInfo { get; }
    public RuleInfo RuleInfo { get; }
    public ScenarioBlock CurrentScenarioBlock { get; internal set; }
    public IObjectContainer ScenarioContainer { get; }

    public ScenarioExecutionStatus ScenarioExecutionStatus { get; internal set; }
    internal List<string> PendingSteps { get; }
    internal List<StepInstance> MissingSteps { get; }
    internal Stopwatch Stopwatch { get; }

    private readonly ITestObjectResolver _testObjectResolver;

    internal ScenarioContext(IObjectContainer scenarioContainer, ScenarioInfo scenarioInfo, RuleInfo ruleInfo, ITestObjectResolver testObjectResolver)
    {
        ScenarioContainer = scenarioContainer;
        _testObjectResolver = testObjectResolver;

        Stopwatch = new Stopwatch();
        Stopwatch.Start();

        CurrentScenarioBlock = ScenarioBlock.None;
        ScenarioInfo = scenarioInfo;
        RuleInfo = ruleInfo;
        ScenarioExecutionStatus = ScenarioExecutionStatus.OK;
        PendingSteps = new List<string>();
        MissingSteps = new List<StepInstance>();
    }

    public ScenarioStepContext StepContext => ScenarioContainer.Resolve<IContextManager>().StepContext;

    public void Pending()
    {
        throw new PendingStepException();
    }

    public static void StepIsPending()
    {
        throw new PendingStepException();
    }

    /// <summary>
    /// Called by Reqnroll infrastructure when an instance of a binding class is needed.
    /// </summary>
    /// <param name="bindingType">The type of the binding class.</param>
    /// <returns>The binding class instance</returns>
    /// <remarks>
    /// The binding classes are the classes with the [Binding] attribute, that might
    /// contain step definitions, hooks or step argument transformations. The method
    /// is called when any binding method needs to be called.
    /// </remarks>
    public object GetBindingInstance(Type bindingType)
    {
        return _testObjectResolver.ResolveBindingInstance(bindingType, ScenarioContainer);
    }
}