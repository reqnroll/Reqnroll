using System;
using System.Diagnostics;
using System.Linq;
using System.Runtime.InteropServices;

namespace Reqnroll.Tools.MsBuild.Generation
{
    public class ProcessInfoDumper : IProcessInfoDumper
    {
        private readonly ITaskLoggingWrapper _taskLoggingWrapper;

        public ProcessInfoDumper(ITaskLoggingWrapper taskLoggingWrapper)
        {
            _taskLoggingWrapper = taskLoggingWrapper;
        }

        public void DumpProcessInfo()
        {
            try
            {
                var currentProcess = Process.GetCurrentProcess();
                _taskLoggingWrapper.LogDiagnosticMessage($"process: {currentProcess.ProcessName}, .NET: {RuntimeInformation.FrameworkDescription}, pid: {currentProcess.Id}, CD: {Environment.CurrentDirectory}");
            }
            catch (Exception e)
            {
                _taskLoggingWrapper.LogDiagnosticMessage($"Error when dumping process info: {e}");
            }
            DumpLoadedAssemblies();
        }

        public void DumpLoadedAssemblies()
        {
            try
            {
                _taskLoggingWrapper.LogDiagnosticMessage("Loaded assemblies:");

                foreach (var assembly in AppDomain.CurrentDomain.GetAssemblies().OrderBy(a => a.FullName))
                {
                    var location = assembly.IsDynamic ? "<dyn>" : assembly.Location;
                    _taskLoggingWrapper.LogDiagnosticMessage($"  {assembly.FullName};{location}");
                }
            }
            catch (Exception e)
            {
                _taskLoggingWrapper.LogDiagnosticMessage($"Error when dumping loaded assemblies: {e}");
            }
        }
    }
}
