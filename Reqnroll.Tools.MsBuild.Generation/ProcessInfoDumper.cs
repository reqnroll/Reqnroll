using System;
using System.Diagnostics;
using System.Linq;
using System.Runtime.InteropServices;

namespace Reqnroll.Tools.MsBuild.Generation
{
    public class ProcessInfoDumper : IProcessInfoDumper
    {
        private readonly IReqnrollTaskLoggingHelper _log;

        public ProcessInfoDumper(IReqnrollTaskLoggingHelper log)
        {
            _log = log;
        }

        public void DumpProcessInfo()
        {
            try
            {
                var currentProcess = Process.GetCurrentProcess();
                _log.LogTaskDiagnosticMessage($"process: {currentProcess.ProcessName}, .NET: {RuntimeInformation.FrameworkDescription}, pid: {currentProcess.Id}, CD: {Environment.CurrentDirectory}");
            }
            catch (Exception e)
            {
                _log.LogTaskDiagnosticMessage($"Error when dumping process info: {e}");
            }
            DumpLoadedAssemblies();
        }

        public void DumpLoadedAssemblies()
        {
            try
            {
                _log.LogTaskDiagnosticMessage("Loaded assemblies:");

                foreach (var assembly in AppDomain.CurrentDomain.GetAssemblies().OrderBy(a => a.FullName))
                {
                    var location = assembly.IsDynamic ? "<dyn>" : assembly.Location;
                    _log.LogTaskDiagnosticMessage($"  {assembly.FullName};{location}");
                }
            }
            catch (Exception e)
            {
                _log.LogTaskDiagnosticMessage($"Error when dumping loaded assemblies: {e}");
            }
        }
    }
}
