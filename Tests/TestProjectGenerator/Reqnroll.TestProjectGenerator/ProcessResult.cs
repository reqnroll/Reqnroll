using System;

namespace Reqnroll.TestProjectGenerator
{
    public class ProcessResult
    {
        public ProcessResult(int exitCode, string stdOutput, string stdError, string combinedOutput, TimeSpan executionTime)
        {
            ExitCode = exitCode;
            StdOutput = stdOutput;
            StdError = stdError;
            CombinedOutput = combinedOutput;
            ExecutionTime = executionTime;
        }

        public string StdOutput { get; }
        public string StdError { get; }
        public string CombinedOutput { get; }
        public int ExitCode { get; }
        public TimeSpan ExecutionTime { get; }
    }
}
