using System;
using System.Diagnostics;
using System.Threading;

namespace Reqnroll
{
    public interface IScenarioStepContext : IReqnrollContext
    {
        StepInfo StepInfo { get; }

        ScenarioExecutionStatus Status { get; set; }
        Exception StepError { get; set; }

    }

    public class ScenarioStepContext : ReqnrollContext, IScenarioStepContext
    {
        #region Singleton
        private static bool isCurrentDisabled = false;
        private static ScenarioStepContext current;
        public static ScenarioStepContext Current
        {
            get
            {
                if (isCurrentDisabled)
                    throw new ReqnrollException("The ScenarioStepContext.Current static accessor cannot be used in multi-threaded execution. Try injecting the scenario context to the binding class. See https:///doc-multithreaded for details.");
                if (current == null)
                {
                    Debug.WriteLine("Accessing NULL ScenarioStepContext");
                }
                return current;
            }
            internal set
            {
                if (!isCurrentDisabled)
                    current = value;
            }
        }

        internal static void DisableSingletonInstance()
        {
            isCurrentDisabled = true;
            Thread.MemoryBarrier();
            current = null;
        }
        #endregion

        public StepInfo StepInfo { get; private set; }

        public ScenarioExecutionStatus Status { get; set; }
        public Exception StepError { get; set; }

        internal ScenarioStepContext(StepInfo stepInfo)
        {
            StepInfo = stepInfo;
        }
    }
}