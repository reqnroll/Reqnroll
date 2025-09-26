using Reqnroll.Tracing;

namespace Reqnroll.Tools.MsBuild.Generation;

public class MSBuildTraceListener(IReqnrollTaskLoggingHelper reqnrollTaskLoggingHelper) : ITraceListener
{
    public void WriteTestOutput(string message)
    {
        reqnrollTaskLoggingHelper.LogTaskMessage(message);
    }

    public void WriteToolOutput(string message)
    {
        reqnrollTaskLoggingHelper.LogTaskMessage("-> " + message);
    }
}
