using Microsoft.Build.Framework;
using Microsoft.Build.Utilities;
using Reqnroll.Tracing;

namespace Reqnroll.Tools.MsBuild.Generation;

public class MSBuildTraceListener : ITraceListener
{
    private readonly ITaskLoggingWrapper _taskLoggingHelper;

    public MSBuildTraceListener(ITaskLoggingWrapper taskLoggingHelper) 
    {
        _taskLoggingHelper = taskLoggingHelper;
    }

    public void WriteTestOutput(string message)
    {
        _taskLoggingHelper.LogMessage(message);
    }

    public void WriteToolOutput(string message)
    {
        _taskLoggingHelper.LogMessage("-> " + message);
    }
}
