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
                _taskLoggingWrapper.LogMessageWithLowImportance($"process: {currentProcess.ProcessName}, .NET: {RuntimeInformation.FrameworkDescription}, pid: {currentProcess.Id}, CD: {Environment.CurrentDirectory}");
            }
            catch (Exception e)
            {
                _taskLoggingWrapper.LogMessageWithLowImportance($"Error when dumping process info: {e}");
            }
            DumpLoadedAssemblies();
        }

        public void DumpLoadedAssemblies()
        {
            try
            {
                _taskLoggingWrapper.LogMessageWithLowImportance("Loaded assemblies:");

                foreach (var assembly in AppDomain.CurrentDomain.GetAssemblies().OrderBy(a => a.FullName))
                {
                    var location = assembly.IsDynamic ? "<dyn>" : assembly.Location;
                    _taskLoggingWrapper.LogMessageWithLowImportance($"  {assembly.FullName};{location}");
                }
            }
            catch (Exception e)
            {
                _taskLoggingWrapper.LogMessageWithLowImportance($"Error when dumping loaded assemblies: {e}");
            }
        }
    }
}
