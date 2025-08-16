using System;
using System.Diagnostics;
using System.Threading;

namespace Reqnroll;

public class ScenarioStepContext : ReqnrollContext, IScenarioStepContext
{
    #region Singleton
    private static bool _isCurrentDisabled = false;
    private static ScenarioStepContext _current;
    public static ScenarioStepContext Current
    {
        get
        {
            if (_isCurrentDisabled)
                throw new ReqnrollException("The ScenarioStepContext.Current static accessor cannot be used in multi-threaded execution. Try injecting the scenario context to the binding class. See https:///doc-multithreaded for details.");
            if (_current == null)
            {
                Debug.WriteLine("Accessing NULL ScenarioStepContext");
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

    public StepInfo StepInfo { get; }
    public ScenarioExecutionStatus Status { get; set; }
    public Exception StepError { get; set; }

    internal ScenarioStepContext(StepInfo stepInfo)
    {
        StepInfo = stepInfo;
    }
}